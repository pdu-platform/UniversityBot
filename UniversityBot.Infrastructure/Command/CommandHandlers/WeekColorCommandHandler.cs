using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UniversityBot.Core.Command.Attributes;

namespace UniversityBot.Infrastructure.Command.CommandHandlers
{
    [SlashCommandHandler("week_color", "Какая неделя?", ServiceLifetime.Singleton)]
    public sealed class WeekColorCommandHandler : ICommandHandler
    {
        private readonly DateTime _whiteWeek;

        public WeekColorCommandHandler(IConfiguration configuration, ILogger<WeekColorCommandHandler> logger)
        {
            var whiteWeekObj = configuration["WhiteWeek"];
            if (string.IsNullOrWhiteSpace(whiteWeekObj))
            {
                WriteInvalidFormatLogMessage(logger);
                whiteWeekObj = "09/01/2020";
            }

            if (!DateTime.TryParse(whiteWeekObj, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                WriteInvalidFormatLogMessage(logger);
                date = new DateTime(1, 9, 2020, 0, 0, 0, DateTimeKind.Utc);
            }
            
            // TO UTC
            _whiteWeek = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, DateTimeKind.Utc).Subtract(TimeSpan.FromHours(3));
        }
        
        public Task Handle<TActivity>(CommandRequest request, ITurnContext<TActivity> turnContext, CancellationToken cancellationToken) 
            where TActivity : IActivity
        {
            var totalDays = GetTotalDaysFromLastWhiteWeek();
            
            var weekColor = IsGreenWeek(totalDays) ? "Зеленая" : "Белая";

            var msg = MessageFactory.Text(weekColor);
            return turnContext.SendActivityAsync(msg, cancellationToken);
        }

        private static bool IsGreenWeek(long totalDays) 
            => totalDays % 2 == 0;
        
        private long GetTotalDaysFromLastWhiteWeek()
        {
            var totalDays = DateTime.UtcNow.Subtract(_whiteWeek).TotalDays;
            
            return (long)Math.Truncate(totalDays);
        }
        
        private static void WriteInvalidFormatLogMessage(ILogger<WeekColorCommandHandler> logger)
        {
            logger.LogError("Неверный формат для даты. Необходимо месяц/день/год. Например 01/05/2020 это эквивалентно 05.01.2020 ");
        }
    }
}