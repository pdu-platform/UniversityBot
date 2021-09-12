using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blowin.Optional;
using Blowin.Result;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using UniversityBot.Blazor.Components;
using UniversityBot.Blazor.Data;
using UniversityBot.Blazor.Extension;
using UniversityBot.Core.DAL;
using UniversityBot.Core.DAL.Attachment;
using UniversityBot.Core.Util;
using UniversityBot.EF;
using static UniversityBot.Blazor.Extension.ConverterFactory;

namespace UniversityBot.Blazor.Pages.Questioner
{
    public partial class QuestionerEdit
    {
        private FileLoader _loader;

        [Parameter] public Guid Id { get; set; }

        private List<QuestionDto> _child;

        private string CreateReactionKeyword { get; set; }
        private bool RequiredReactionKeyword { get; set; }
        
        private string StrParentId { get; set; }
        private QuestionDtoWithParentId _onOpenQuestion;
        private UnitOfWork _uow;
        private List<Guid> _allChild;
        private BotQuestioner EditBotQuestion { get; set; }

        private Task _searchTask = Task.CompletedTask;
        private CancellationTokenSource _tokenSource = new();

        private List<FileLoader.LoadFile> _dbFiles = new();
        private Dictionary<FileLoader.LoadFile, BotMessageAttachment> _dbFileMap = new();
        private List<BotReactionKeyword> _deleteReactionKeywords;

        protected override async Task OnInitializedAsync()
        {
            _uow = new UnitOfWork(Context);
            _child = new List<QuestionDto>();
            _deleteReactionKeywords = new List<BotReactionKeyword>();
            
            var editBotQuestionTask = _uow.QuestionRepository
                .Query()
                .Include(e => e.Attachments).ThenInclude(e => e.File)
                .Include(e => e.Parent)
                .Include(e => e.ReactionKeywords)
                .FirstAsync(e => e.Id == Id);

            var allChildTask = _uow.QuestionRepository.AllChildForQuestionAsync(Id);

            EditBotQuestion = await editBotQuestionTask;
            _allChild = await allChildTask;

            foreach (var botMessageAttachment in EditBotQuestion.Attachments)
            {
                var file = botMessageAttachment.File;

                var mimeType = MimeTypesMap.GetMimeType(file.Extension);
                
                string base64 = null;
                if (FileUtil.IsImage(mimeType))
                {
                    var base64Res = FileTransformer.ToBase64(file);
                    if (base64Res.IsOk)
                        base64 = base64Res.Value;
                }
                
                var insertFile = new FileLoader.LoadFile(file.Name + file.Extension, file.Content, mimeType, Optional.From(base64));
                
                _dbFiles.Add(insertFile);
                _dbFileMap.Add(insertFile, botMessageAttachment);
            }

            _onOpenQuestion = EditBotQuestion.Map(To<QuestionDtoWithParentId>());

            StrParentId = EditBotQuestion.ParentId?.ToString();
        }

        private void Cancel() => NavigationManager.NavigateTo("/");

        private async Task Save()
        {
            await using (_uow.AsyncSaveScope())
            {
                var tran = await _uow.TransactScopeAsync();
                var newParentId = ConvertParentIdToGuid(StrParentId);
                if (_onOpenQuestion.ParentId != newParentId)
                    EditBotQuestion.ParentId = newParentId;
                
                var removeAttachment = _dbFileMap
                    .Where(e => !_loader.Store.Files.Contains(e.Key)).Select(e => e.Value)
                    .ToList();

                _uow.AttachmentRepository.RemoveAll(removeAttachment);

                _uow.ReactionKeywordRepository.RemoveAll(_deleteReactionKeywords);

                var insertEntity = EditBotQuestion.ReactionKeywords.Where(e => e.Id == Guid.Empty);
                await _uow.ReactionKeywordRepository.InsertAsync(insertEntity);
                
                await InsertNotExistsFilesAsync();

                await tran.DisposeAsync();
            }

            NavigationManager.NavigateTo("/");
        }

        private async Task InsertNotExistsFilesAsync()
        {
            var notExistFiles = _loader.Store.Files.Where(e => !_dbFileMap.ContainsKey(e)).ToList();
            var notExistsFilesTasks = new List<Result<BotFile, string>>();
            foreach (var e in notExistFiles)
            {
                var extension = Path.GetExtension(e.Name);
                if (string.IsNullOrWhiteSpace(extension))
                {
                    notExistsFilesTasks.Add(Result.Fail("Не удалось определить расширене файла"));
                    continue;
                }

                var file = new BotFile(Guid.Empty, e.Content, e.Size, e.Name, extension);
                notExistsFilesTasks.Add(Result.Success(file));
            }
           
            var successFiles = notExistsFilesTasks.Where(e => e.IsOk).Select(e => e.Value).ToList();

            await _uow.Database.Files.AddRangeAsync(successFiles);

            var insertAttachment = successFiles.Select(e => new BotMessageAttachment(Guid.Empty, EditBotQuestion, e));

            await _uow.AttachmentRepository.AddAll(insertAttachment);
        }

        private async Task OnSearch(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            if (!_searchTask.IsCompleted)
            {
                _tokenSource.Cancel();
                _tokenSource = new CancellationTokenSource();
            }

            var pattern = $"%{value}%";
            var tmp = _uow.QuestionRepository.Query()
                .AsNoTracking()
                .Where(e => !_allChild.Contains(e.Id) && e.Id != Id &&
                            Microsoft.EntityFrameworkCore.EF.Functions.Like(e.Question, pattern))
                .Project(To<QuestionDto>())
                .ToListAsync(_tokenSource.Token);

            _searchTask = tmp;

            var unwrap = (await tmp).ToList();

            _child.Clear();
            _child.AddRange(unwrap);

            StateHasChanged();
        }

        private void DeleteReactionKeyword(BotReactionKeyword value)
        {
            if(value == null)
                return;

            var idx = EditBotQuestion.ReactionKeywords.FindIndex(e => ReferenceEquals(e, value));
            if(idx < 0)
                return;
            
            if(value.Id != Guid.Empty)
                _deleteReactionKeywords.Add(value);
            
            EditBotQuestion.ReactionKeywords.RemoveAt(idx);
        }
        
        private void AddReactionKeyword()
        {
            if(string.IsNullOrWhiteSpace(CreateReactionKeyword))
                return;

            var keyword = KeywordFactory.Create(CreateReactionKeyword);
            if(EditBotQuestion.ReactionKeywords.Any(e => e.Word == keyword.Word))
                return;
            
            EditBotQuestion.ReactionKeywords.Add(new BotReactionKeyword(Guid.Empty, keyword, Id, null, RequiredReactionKeyword));

            CreateReactionKeyword = null;
            RequiredReactionKeyword = false;
            StateHasChanged();
        }
        
        private static Guid? ConvertParentIdToGuid(string parentId)
        {
            if (string.IsNullOrWhiteSpace(parentId))
                return null;

            if (Guid.TryParse(parentId, out var r))
                return r;

            return null;
        }
    }
}