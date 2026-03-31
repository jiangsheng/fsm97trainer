using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FSM97Lib
{
    interface ITeamMember
    {
        int Position { get; set; }
        int BestPosition { get; }
        string PositionName { get; }
        string TeamName { get; set; }
        string TeamAbbrivation { get; set; }
        int Number { get; set; }
        int Status { get; set; }
        int ContractWeeks { get; set; }
        double Salary { get; set; }
        int Goals { get; set; }
        int MVP { get; set; }

        int Form { get; set; }
        int Moral { get; set; }
        int Energy { get; set; }
        int GamesThisSeason { get; set; }
        TeamModel Team { get; set; }
        int Statistics { get; }
        int PositionRating { get; }
        int BestPositionRating { get; }
    }
}
