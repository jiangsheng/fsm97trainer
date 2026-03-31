using CsvHelper.Configuration;
using FSM97Lib;

namespace Fsm97Trainer
{
    internal class PlayerMap : CsvClassMap<PlayerModel>
    {
        public PlayerMap()
        {
            AutoMap();
            Map(m => m.Team).Ignore();
            Map(m => m.BirthDateOffset).Ignore();
            Map(m => m.Attributes).Ignore();
        }

    }
}
