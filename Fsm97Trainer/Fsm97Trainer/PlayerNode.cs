using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
}
