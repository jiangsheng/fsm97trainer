using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fsm97Trainer
{
    public interface IObjectWithPersonName
    {
         string LastName { get; set; }
         string FirstName { get; set; }
    }
}
