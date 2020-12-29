using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    // Wrong password fault 
    [DataContract]
    public class WrongPasswordFault
    {
        [DataMember]
        public string Message { get; set; }
    }
}
