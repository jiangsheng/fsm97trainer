﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fsm97Trainer
{
    internal class ObjectWithValue {
        public string Value { get; set; }
    }
    internal class QueryUpdatePlayerNameResult
    {
        public ObjectWithValue Player{ get; set; }
        public ObjectWithValue Name { get; set; }
    }
}
