using System.Text;
using UniversityBot.Core.Util;
using Xunit;

namespace UniversityBot.Test.Tests
{
    public class StringBuilderTest
    {
        [Theory]
        [InlineData("")]
        [InlineData("d")]
        [InlineData(" d")]
        [InlineData(" d ")]
        [InlineData("d ")]
        [InlineData(" dqw")]
        [InlineData(" d  fq  w    ")]
        [InlineData("fqw fqd ")]
        public void Trim(string data)
        {
            var expected = data.Trim();
            var sb = new StringBuilder(data);
            
            var res = sb.Trim().ToString();
            
            Assert.Equal(expected, res);
        }
    }
}