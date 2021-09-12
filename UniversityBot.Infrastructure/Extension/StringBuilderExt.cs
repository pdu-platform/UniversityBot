using System;
using System.Collections.Generic;
using System.Text;
using Dawn;

namespace UniversityBot.Infrastructure.Extension
{
    public static class StringBuilderExt
    {
        public static StringBuilder JointToString<T>(this StringBuilder self, IEnumerable<T> seq, 
          Func<T, string> valueConverter = null,
          string separator = ", ",
          string prefix = null, string postfix = null,
          int? restrictionCount = null, string restrictionStr = "...")
        {
          Guard.Argument(self, nameof(self)).NotNull();
          Guard.Argument(seq, nameof(seq)).NotNull();

          valueConverter ??= arg => arg?.ToString();
          separator ??= string.Empty;
          
          var enumerator = seq.GetEnumerator();
          try
          {
            if (enumerator.MoveNext())
            {
              if (prefix != null)
                self.Append(prefix);
              
              if (restrictionCount.HasValue)
              {
                var unwrapRestriction = restrictionCount.Value;
                if (unwrapRestriction > 0)
                {
                  var iterateCount = 1;
                  
                  var value = valueConverter(enumerator.Current);
                  self.Append(value);
                  while (iterateCount < unwrapRestriction && enumerator.MoveNext())
                  {
                    value = valueConverter(enumerator.Current);
                    self.Append(separator).Append(value);
                    
                    iterateCount += 1;
                  }

                  if (iterateCount == unwrapRestriction && enumerator.MoveNext() && !string.IsNullOrEmpty(restrictionStr))
                  {
                    self.Append(separator).Append(restrictionStr);
                  }
                }
              }
              else
              {
                var value = valueConverter(enumerator.Current);
                self.Append(value);
                while (enumerator.MoveNext())
                {
                  value = valueConverter(enumerator.Current);
                  self.Append(separator).Append(value); 
                }
              }
            }
            
            if (postfix != null)
              self.Append(postfix);
          }
          finally
          {
            enumerator.Dispose();
          }

          return self;
        }
    }
}