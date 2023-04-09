using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Fsm97Trainer
{
    public class PlayerNodeList : List<PlayerNode>
    {
        public Process GameProcess { get; set; }
        public Encoding Encoding { get; set; }

        public Team currentTeam { get; set; }
        public MenusProcess MenusProcess { get; set; }
}
}
