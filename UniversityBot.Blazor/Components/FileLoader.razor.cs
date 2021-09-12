using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blowin.Optional;
using Blowin.Result;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using UniversityBot.Blazor.Data;
using UniversityBot.Core.Util;
using UniversityBot.Infrastructure;

namespace UniversityBot.Blazor.Components
{
    public partial class FileLoader
    {
        [Parameter] public List<LoadFile> InitialItems { get; set; }

        [Parameter] public bool Multiple { get; set; } = true;

        private string dropClass = "";

        public IFileStore Store { get; private set; }

        private string Error { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Error = null;
            await base.OnInitializedAsync();

            Store = Multiple ? new MultipleFileStore(FileTransformer) : new SingleFileStore(FileTransformer);

            if (InitialItems != null)
                Store.Select(InitialItems);
        }

        private async Task OnChangeInputFile(InputFileChangeEventArgs obj)
        {
            Error = null;
            var result = await Store.Select(obj);
            if (result.IsFail)
                Error = result.FailMessage;
            
            StateHasChanged();
        }

        private Task RemoveItem(LoadFile file)
        {
            Store.Remove(file);
            return Task.CompletedTask;
        }

        private void HandleDragEnter(DragEventArgs obj) => dropClass = "dropzone-drag";

        private void HandleDragLeave(DragEventArgs obj) => dropClass = "";
        
        public interface IFileStore
        {
            int Count { get; }
            IEnumerable<LoadFile> Files { get; }
            void Select(IEnumerable<LoadFile> loadFiles);
            ValueTask<Result> Select(InputFileChangeEventArgs obj);
            void Remove(LoadFile file);
        }

        private sealed class SingleFileStore : IFileStore
        {
            private LoadFile _file;
            private FileTransformer _fileTransformer;

            public int Count => _file == null ? 0 : 1;

            public IEnumerable<LoadFile> Files
            {
                get
                {
                    if (_file != null)
                        yield return _file;
                }
            }

            public SingleFileStore(FileTransformer fileTransformer)
            {
                _fileTransformer = fileTransformer;
            }

            public void Select(IEnumerable<LoadFile> loadFiles)
            {
                _file = loadFiles.FirstOrDefault();
            }

            public async ValueTask<Result> Select(InputFileChangeEventArgs obj)
            {
                _file = null;
                var (isOk, value, fail) = await LoadFile.ToLoadFile(obj.File, _fileTransformer);
                if (isOk)
                {
                    _file = value;
                    return Result.Success();
                }

                return Result.Fail(fail);
            }

            public void Remove(LoadFile file)
            {
                if (file == _file)
                    _file = null;
            }
        }

        private sealed class MultipleFileStore : IFileStore
        {
            private FileTransformer _fileTransformer;
            private HashSet<LoadFile> _content = new();

            public int Count => _content.Count;

            public IEnumerable<LoadFile> Files => _content;

            public MultipleFileStore(FileTransformer fileTransformer)
            {
                _fileTransformer = fileTransformer;
            }

            public void Select(IEnumerable<LoadFile> loadFiles)
            {
                foreach (var loadFile in loadFiles)
                    _content.Add(loadFile);
            }

            public async ValueTask<Result> Select(InputFileChangeEventArgs obj)
            {
                var fails = new List<string>();
                foreach (var multipleFile in obj.GetMultipleFiles())
                {
                    var loadFile = await LoadFile.ToLoadFile(multipleFile, _fileTransformer);
                    if (loadFile.IsOk)
                    {
                        _content.Add(loadFile.Value);
                    }
                    else
                    {
                        fails.Add($"{multipleFile.Name} - {loadFile.Error}");
                    }
                }

                return fails.Count > 0 ? Result.Fail(string.Join(Environment.NewLine, fails)) : Result.Success();
            }

            public void Remove(LoadFile file) => _content.Remove(file);
        }

        public record LoadFile(string Name, byte[] Content, string ContentType, Optional<string> Base64)
        {
            public int Size => Content?.Length ?? 0;
            public bool IsImage => FileUtil.IsImage(ContentType);
            
            public static async Task<Result<LoadFile, string>> ToLoadFile(IBrowserFile e, FileTransformer fileTransformer)
            {
                var contentResult = await e.ReadFileContentAsArrayAsync(int.MaxValue);
                if (contentResult.IsFail)
                {
                    Result<LoadFile, string> fail = Result.Fail(contentResult.Error);
                    return fail;
                }

                string base64 = null;
                    
                if (e.IsImage())
                {
                    var (isOk, value, _) = fileTransformer.ToBase64(e.ContentType, contentResult.Value, contentResult.Value.Length);
                    if (isOk)
                        base64 = value;
                }
                var res = new LoadFile(e.Name, contentResult.Value, e.ContentType, Optional.From(base64));
                return Result.Success(res);
            }
        }
    }
}