using System;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Components.Forms;
using UniversityBot.Core.Util;

namespace UniversityBot.Blazor.Data
{
    public sealed record ByteBrowserFile : IBrowserFile
    {
        private readonly byte[] _content;
        
        public string Name { get; }
        public string Extension { get; }
        public DateTimeOffset LastModified { get; }
        public long Size => _content.Length;
        public string ContentType => !string.IsNullOrWhiteSpace(Extension) ? MimeTypesMap.GetMimeType(Extension) : string.Empty;
        
        public ByteBrowserFile(string name, byte[] content, string extension, DateTimeOffset lastModified = default)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _content = content ?? throw new ArgumentNullException(nameof(content));
            Extension = extension;
            LastModified = lastModified;
        }
        
        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = new()) 
            => new MemoryStream(_content, false);
    }
}