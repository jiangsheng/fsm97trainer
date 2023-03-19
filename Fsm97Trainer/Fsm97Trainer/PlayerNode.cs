using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Fsm97Trainer
{
    public class PlayerNode
    {
        public int NodeAddress { get; set; }
        public int DataAddress { get; set; }
        public int NextNode{ get; set; }
        public int PreviousNode{ get; set; }
        public Player Data { get; set; }
    }
    public class PlayerNodeList : List<PlayerNode>
    {
        public Process GameProcess { get; set; }
        public Encoding Encoding { get; set; }

        Team currentTeam { get; set; }
    }
}
