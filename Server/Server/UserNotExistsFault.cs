﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    // User not Exists fault
    [DataContract]
    public class UserNotExistsFault
    {
        [DataMember]
        public string Message { get; set; }
    }
}
