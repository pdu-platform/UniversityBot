using System;

namespace UniversityBot.Blazor.Data
{
    public class QuestionDto : DtoBase
    {
        public string Question { get; set; }
    }

    public class QuestionDtoWithParentId : QuestionDto
    {
        public Guid? ParentId { get; set; }
    }
}