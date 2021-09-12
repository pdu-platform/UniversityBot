using System;
using System.Collections.Generic;
using Dawn;
using UniversityBot.Core.Util;

namespace UniversityBot.Core.DAL
{
    public class BotCommand : BotEntity<BotCommand>
    {
        // Имя которое используется для отображения по вводу команды /commands в списке (для пользователя)
        public string UserFriendlyName { get; set; }

        // показывать ли команду в общем списке по написанию /show_all
        public bool ShowInAllCommandList { get; set; } = true;
        
        // список комманд, на которые будет реагировать данная команда. например /show_all, покажи все
        public List<BotCommandQuestion> Questions { get; private set; }
        
        public string Answer { get; set; }
        
        private BotCommand() : base(Guid.Empty)
        {
        }

        public BotCommand(Guid id, string userFriendlyName, bool showInAllCommandList, BotCommandQuestion questions, 
            string answer) 
            : this(id, userFriendlyName, showInAllCommandList, 
                questions != null ? new List<BotCommandQuestion> { questions } : null, answer)
        {
        }
        
        public BotCommand(Guid id, string userFriendlyName, bool showInAllCommandList, List<BotCommandQuestion> questions, 
            string answer) : base(id)
        {
            UserFriendlyName = userFriendlyName;
            ShowInAllCommandList = showInAllCommandList;
            Questions = questions;
            Answer = answer;
        }

        public static BotCommand ForRemove(Guid botCommandId) => new ()
        {
            Id = botCommandId,
            Questions = new List<BotCommandQuestion>()
        };
        
        protected override bool EqualsCore(BotCommand other)
        {
            return UserFriendlyName == other.UserFriendlyName &&
                   ShowInAllCommandList == other.ShowInAllCommandList &&
                   CollectionsUtil.AreSame(Questions, other.Questions);
        }

        protected override int GetHashCodeCore()
            => HashCode.Combine(UserFriendlyName, ShowInAllCommandList, CollectionsUtil.HashCode(Questions));
        
        private static void ThrowIfNull<T>(ICollection<T> collection, string propertyName) 
            => Guard.Argument(collection,propertyName).NotNull().NotEmpty();
    }
}