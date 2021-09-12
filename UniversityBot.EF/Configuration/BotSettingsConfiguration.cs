using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniversityBot.Core.DAL;

namespace UniversityBot.EF.Configuration
{
    public class BotSettingsConfiguration : BotEntityConfigurationBase<BotSettings>
    {
        protected override string EntityName => "tbx_Bot_Settings";
        
        protected override void ConfigureEntity(EntityTypeBuilder<BotSettings> builder)
        {
            builder.Property(e => e.SplitWelcomeMessage)
                .HasColumnName("split_welcome_message")
                .IsRequired();

            builder.Property(e => e.MinKeywordMatchCount)
                .HasColumnName("min_keyword_match_count")
                .IsRequired();

            builder.Property(e => e.OnlyAnswerWithMaxMatch)
                .HasColumnName("only_answer_with_max_match")
                .IsRequired();

            builder.Property(e => e.DontDisplayChildAndParentQuestionInAnswer)
                .HasColumnName("dont_display_child_and_parent_question_in_answer")
                .IsRequired();

            builder.Property(e => e.NotFoundAnswerMessage)
                .HasColumnName("not_found_answer_message")
                .IsRequired();

            builder.Property(e => e.UseFuzzySearch)
                .HasColumnName("use_fuzzy_search")
                .IsRequired();
            
            builder.HasData(BotSettings.CreateDefault(Guid.NewGuid()));
        }
    }
}