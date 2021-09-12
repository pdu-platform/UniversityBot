using Blowin.Result;

namespace UniversityBot.Core.Statistic
{
    public readonly struct NotFoundStoreEntity
    {
        public string Word { get; }

        private NotFoundStoreEntity(string word)
        {
            Word = word;
        }

        public static Result<NotFoundStoreEntity> Create(string word)
        {
            if (string.IsNullOrEmpty(word))
                return Result.Fail("word can't be empty");

            var res = new NotFoundStoreEntity(word);
            return Result.Success(res);
        }
    }
}