using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using UniversityBot.Core;

namespace UniversityBot.Infrastructure.Extension
{
    public static class TurnContextExt
    {
        public static FormatRequest ToFormatRequest<TActivity>(this ITurnContext<TActivity> self) 
            where TActivity : IActivity
        {
            var localOffset = self.Activity.LocalTimestamp ?? DateTimeOffset.Now;
            var userName = self.Activity?.From?.Name;
            return new FormatRequest
            {
                UserName = userName,
                NowTime = localOffset.LocalDateTime
            };
        }
    }
}