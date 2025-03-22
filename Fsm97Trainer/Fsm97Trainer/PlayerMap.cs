using CsvHelper.Configuration;

namespace Fsm97Trainer
{
    internal class PlayerMap : CsvClassMap<Player>
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
