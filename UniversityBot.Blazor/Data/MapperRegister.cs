using Mapster;
using UniversityBot.Core.DAL;

namespace UniversityBot.Blazor.Data
{
    public class MapperRegister
    {
        public void Register()
        {
            var map = TypeAdapterConfig.GlobalSettings;
            map.ForType<BotQuestioner, QuestionDto>().Compile();
        }
    }
}