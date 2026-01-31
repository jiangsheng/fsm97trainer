using NameParser;
using System;

namespace Fsm97Trainer
{
    internal class QueryUpdatePlayerNameResult
    {
        public string EntityId { get; set; }
        public string EnglishName { get; set; }
        public string ChineseName { get; set; }
        public string BirthDayText { get; set; }
        public DateTime? BirthDay { get; set; }
        public HumanName HumanName { get; set; }
    }
}
