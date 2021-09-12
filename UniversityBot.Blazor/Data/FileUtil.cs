namespace UniversityBot.Blazor.Data
{
    public static class FileUtil
    {
        public static bool IsImage(string contentType)
        {
            var type = contentType ?? string.Empty;
            return type.StartsWith("image");
        }
    }
}