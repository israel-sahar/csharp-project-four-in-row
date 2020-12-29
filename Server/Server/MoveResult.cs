using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
        [DataContract]
        public enum MoveResult
        {
            [EnumMember]
            Winning,
            [EnumMember]
            Draw,
            [EnumMember]
            GameOn,
        [EnumMember]
        OtherUserDisconnected
    }
}
