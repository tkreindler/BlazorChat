using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorChat.Shared;

namespace BlazorChat.Client
{
    public class ClientUserData : UserData
    {
        public CallState CallState { get; set; }

        public ClientUserData()
        {
        }

        public ClientUserData(UserData input)
        {
            this.UserName = input.UserName;
            this.Guid = input.Guid;
            this.CallState = CallState.None;
        }
    }

    public enum CallState
    {
        None,
        Outgoing,
        Incoming,
        Current,
    }
}
