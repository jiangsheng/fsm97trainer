using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fsm97Trainer
{
    internal class PlayerMap:CsvClassMap<Player>
    {
        public PlayerMap()
        {
            AutoMap();
            Map(m => m.Team).Ignore();
            Map(m => m.BirthDateOffset).Ignore();
            
        }

    }
}
