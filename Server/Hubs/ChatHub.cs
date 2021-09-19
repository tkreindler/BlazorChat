using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace BlazorChat.Server.Hubs
{
    public class ChatHub : Hub
    {
        private static readonly Dictionary<string, ServerUserData> connectedUsers = new ();
        private static readonly Dictionary<Guid, ServerUserData> users = new();

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            var sendUsersList = connectedUsers.Values.Select((user, _) => (Shared.UserData)user).ToArray();

            await Clients.Caller.SendAsync("ReceiveUsers", sendUsersList);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);

            connectedUsers.Remove(Context.ConnectionId);

            var sendUsersList = connectedUsers.Values.Select((user, _) => (Shared.UserData)user).ToArray();

            await Clients.Others.SendAsync("ReceiveUsers", sendUsersList);
        }

        public async Task RegisterUser(Guid guid, string user)
        {
            var userData = new ServerUserData
            {
                UserName = user,
                Guid = guid,
                ConnectionId = Context.ConnectionId,
            };

            connectedUsers.Add(Context.ConnectionId, userData);
            users.Add(guid, userData);

            var sendUsersList = connectedUsers.Values.Select((user, _) => (Shared.UserData)user).ToArray();

            await Clients.All.SendAsync("ReceiveUsers", sendUsersList);
        }

        public async Task Call(Guid me, Guid otherUser)
        {
            bool success = users.TryGetValue(otherUser, out var userData);

            if (!success)
            {
                throw new Exception("Other user doesn't exist");
            }

            // null if the other user already disconnected, in that case do nothing
            if (userData.ConnectionId != null)
            {
                await Clients.Client(userData.ConnectionId).SendAsync("ReceiveCall", me);
            }

        }

        public async Task AcceptCall(Guid me, Guid otherUser)
        {
            bool success = users.TryGetValue(otherUser, out var userData);

            if (!success)
            {
                throw new Exception("Other user doesn't exist");
            }

            // null if the other user already disconnected, in that case do nothing
            if (userData.ConnectionId != null)
            {
                await Clients.Client(userData.ConnectionId).SendAsync("ReceiveAcceptCall", me);
            }

        }


        public async Task SendRtcData(Guid me, Guid otherUser, string type, string data)
        {
            bool success = users.TryGetValue(otherUser, out var userData);

            if (!success)
            {
                throw new Exception("Other user doesn't exist");
            }

            // null if the other user already disconnected, in that case do nothing
            if (userData.ConnectionId != null)
            {
                await Clients.Client(userData.ConnectionId).SendAsync("ReceiveRtcData", me, type, data);
            }
        }

        
    }
}

