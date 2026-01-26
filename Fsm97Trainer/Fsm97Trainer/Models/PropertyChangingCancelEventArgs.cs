using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Fsm97Trainer.Models
{
    public class PropertyChangingCancelEventArgs : CancelEventArgs
    {
        public Object OldValue { get; }
        public Object NewValue { get; }
        public string PropertyName { get; }
        public PropertyChangingCancelEventArgs(string propertyName,Object oldValue, Object newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
            PropertyName = propertyName;
            Cancel = false; // Default to not canceling
        }
    }
}
