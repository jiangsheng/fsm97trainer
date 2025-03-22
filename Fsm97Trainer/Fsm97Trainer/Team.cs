using System.Collections.Generic;

namespace Fsm97Trainer
{
    public class Team
    {
        public string Name { get; set; }
        public string FanGroupName { get; set; }
        public string Abbreviation { get; set; }
        public string ManagerFirstName { get; set; }
        public string ManagerLastName { get; set; }
        public LinkedList<PlayerNode> PlayerNodes { get; set; }

        //00588D12 Arsenal Team Name (16 Chars)
        //00588D2b Fan Group Name
        //00588D3d ARS
        //00588DA6 Manager First
        //00588DC1 Manager Last
        //00588E48 ->struct DoubleLinkedList{Player,Next,Prev}        
        //....last team 5a4012


    }
}
