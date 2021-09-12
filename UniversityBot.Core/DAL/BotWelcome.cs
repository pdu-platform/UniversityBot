using System;

namespace UniversityBot.Core.DAL
{
    public class BotWelcome : BotEntity<BotWelcome>
    {
        public string Text { get; set; }
        
        public int Order { get; set; }
        
        private BotWelcome() : base(Guid.Empty)
        {
        }
        
        public BotWelcome(Guid id, string text, int order) : base(id)
        {
            Text = text;
            Order = order;
        }

        public static BotWelcome ForRemove(Guid id) => new BotWelcome()
        {
            Id = id
        };
        
        protected override bool EqualsCore(BotWelcome other) => Order == other.Order && Text == other.Text;

        protected override int GetHashCodeCore() => HashCode.Combine(Order, Text);
    }
}