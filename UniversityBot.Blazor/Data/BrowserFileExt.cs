using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Blowin.Result;
using Microsoft.AspNetCore.Components.Forms;

namespace UniversityBot.Blazor.Data
{
    public static class BrowserFileExt
    {
        public static bool IsImage<T>(this T self)
            where T : IBrowserFile
        {
            return FileUtil.IsImage(self?.ContentType);
        }

        private static async Task<int> ReadByteAsync(Stream stream, byte[] buff, CancellationToken cancellationToken = default)
        {
            var readCount = await stream.ReadAsync(buff, 0, 1, cancellationToken);
            if (readCount <= 0)
                return -1;
                    
            return buff[0];
        }

        public static async Task<Result<byte[], string>> ReadFileContentAsArrayAsync<T>(this T self, int maxAllowedSize = 512000,
            CancellationToken cancellationToken = default(CancellationToken))
            where T : IBrowserFile
        {
            var data = await self.ReadFileContentAsync(maxAllowedSize, cancellationToken: cancellationToken);
            if (data.IsFail)
                return Result.Fail(data.Error);
            
            try
            {
                var result = new byte[data.Value.RentSize];
                Array.Copy(data.Value.Data, result, result.Length);
                return result;
            }
            finally
            {
                data.Value.Dispose();
            }
        }

        public static async Task<Result<PoolSharedData<byte>, string>> ReadFileContentAsync(this IBrowserFile self, int maxAllowedSize = 512000, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var res = await ReadFileContentCoreAsync(self, maxAllowedSize, cancellationToken);
                return Result.Success(res);
            }
            catch (IOException)
            {
                return Result.Fail("Файл слишком большой");
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }
        
        // https://jonskeet.uk/csharp/readbinary.html
        private static async Task<PoolSharedData<byte>> ReadFileContentCoreAsync(this IBrowserFile self, int maxAllowedSize = 512000, CancellationToken cancellationToken = default (CancellationToken))
        {
            await using var stream = self.OpenReadStream(maxAllowedSize, cancellationToken);

            var fileSize = (int)self.Size;
            var data = ArrayPool<byte>.Shared.RentAsPoolSharedData(fileSize);
            var readBytes = new byte[1];
            
            int read = 0;
            while (true)
            {
                var chunk = await stream.ReadAsync(data.Data, read, (fileSize - read), cancellationToken);
                if(chunk <= 0)
                    break;
                
                read += chunk;
                if (read != data.RentSize) 
                    continue;
                
                var nextByte = await ReadByteAsync(stream, readBytes, cancellationToken);
                if (nextByte <= 0)
                    return data;
                
                var newData = ArrayPool<byte>.Shared.RentAsPoolSharedData(data.RentSize * 2);
                Array.Copy(data.Data, newData.Data, data.RentSize);
                data.Dispose();
                data = newData;
                data.Data[read] = (byte)nextByte;
                read += 1;
            }

            return new PoolSharedData<byte>(data.Data, read, data.ClearArray);
        }
    }
}