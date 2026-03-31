using FSM97Lib;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Xml.Linq;

namespace Fsm97Trainer
{
    public class TeamNode
    {
        public int Address { get; set; }
        public LinkedList<PlayerNode> PlayerNodes { get; set; }
        public TeamModel Data { get; set; } = new TeamModel();

        public override string ToString()
        {
            return string.Format("{0:X}:{1}", Address, Data);
        }
    }
}
