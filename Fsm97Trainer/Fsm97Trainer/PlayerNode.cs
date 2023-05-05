using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Fsm97Trainer
{
    public class PlayerNode: IObjectWithPersonName
    {
        public int NodeAddress { get; set; }
        public int DataAddress { get; set; }
        public int NextNode{ get; set; }
        public int PreviousNode{ get; set; }
        public Player Data { get; set; }
        public string LastName { get => ((IObjectWithPersonName)Data).LastName; set => ((IObjectWithPersonName)Data).LastName = value; }
        public string FirstName { get => ((IObjectWithPersonName)Data).FirstName; set => ((IObjectWithPersonName)Data).FirstName = value; }
    }
}
