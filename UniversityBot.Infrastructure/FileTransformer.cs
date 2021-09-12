using System;
using System.Text;
using Blowin.Result;
using Microsoft.Extensions.ObjectPool;
using UniversityBot.Core.DAL;
using UniversityBot.Core.Util;
using UniversityBot.Infrastructure.Extension;

namespace UniversityBot.Infrastructure
{
    public class FileTransformer
    {
        private readonly ObjectPool<StringBuilder> _sbPool;

        public FileTransformer(ObjectPool<StringBuilder> sbPool)
        {
            _sbPool = sbPool;
        }

        public Result<string> ToBase64(BotFile file)
        {
            if (file == null)
                return Result.Fail("Не указан файл");

            var contentType = MimeTypesMap.GetMimeType(file.Extension);
            return ToBase64(contentType, file.Content, (int)file.Size);
        }
        
        public Result<string> ToBase64(string contentType, byte[] content, int length, int offset = 0)
        {
            if (content == null)
                return Result.Fail("Нет содержимого");
            if (string.IsNullOrEmpty(contentType))
                return Result.Fail("Необходимо указать ContentType");

            try
            {
                using var item = _sbPool.GetScoped();

                var predictSize = PredictOfSizeBase64(length);
            
                item.Item.EnsureCapacity((int)(predictSize + contentType.Length + "data:".Length + ";base64,".Length));

                var base64 = Convert.ToBase64String(content, offset, length);
            
                var res = item.Item.Append("data:").Append(contentType).Append(";base64,").Append(base64).ToString();
                return Result.Success(res);
            }
            catch (Exception e)
            {
                return Result.Fail(e);
            }
        }

        private static long PredictOfSizeBase64(int originalSizeInBytes)
        {
            return 4 * (int)Math.Ceiling(originalSizeInBytes / 3.0);
        }
    }
}