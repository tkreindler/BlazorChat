using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorChat.Server
{
    public class ServerUserData : Shared.UserData
    {
        public string ConnectionId { get; set; }
    }
}
