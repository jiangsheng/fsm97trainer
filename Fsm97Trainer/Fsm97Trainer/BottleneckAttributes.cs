using FSM97Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fsm97Trainer
{
    public class BottleneckAttributes
    {
        public double Rounds { get; set; }
        public PlayerAttribute AttributeIndex { get; set; }
        public int Repeat { get; set; }
        public override string ToString()
        {
            return string.Format("{0}*{1}", AttributeIndex, Repeat);
        }
    }
}
