using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    //User not found fault
    [DataContract]
    public class UserNotFoundFault
    {
        [DataMember]
        public string Message { get; set; }
    }
}
