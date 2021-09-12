using Blowin.Result;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using UniversityBot.Core.DAL;

namespace UniversityBot.Infrastructure.Extension
{
    public static class BotCommandExt
    {
        public static Result<Activity, string> ToActivity(this BotCommand self)
        {
            var txt = MessageFactory.Text(self.Answer);
            return Result.Success(txt);
        }
    }
}