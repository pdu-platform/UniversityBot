using System.Text;

namespace UniversityBot.Core.Util
{
    public static class StringBuilderExt
    {
        public static StringBuilder Trim(this StringBuilder self)
        {
            if (self.Length == 0)
                return self;

            var count = 0;
            for (var i = 0; i < self.Length; i++)
            {
                if (!char.IsWhiteSpace(self[i]))
                    break;
                count++;
            }

            if (count > 0)
            {
                self.Remove(0, count);
                count = 0;
            }

            for (var i = self.Length - 1; i >= 0; i--)
            {
                if (!char.IsWhiteSpace(self[i]))
                    break;
                count++;
            }

            if (count > 0)
                self.Remove(self.Length - count, count);

            return self;
        }
    }
}