using Microsoft.AspNetCore.SignalR;

namespace PatientAnalytics.Hubs;

public class PatientHub : Hub
{
    public async Task TestSendMessage(string message)
    {
        await Clients.All.SendAsync("TestReceiveMessage", message);
    }

    public async Task SendMessageToGroup(string groupName, string message)
    {
        await Clients.GroupExcept(groupName, Context.ConnectionId).SendAsync("ReceiveMessage", $"{message}");
    }

    public async Task AddToGroup(string userName, string userRole)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, userRole);
        await SendMessageToGroup(userRole, $"{userName} has signed in.");
        
        if (userRole == "SuperAdmin")
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admin");
            await SendMessageToGroup("Admin", $"{userName} has signed in.");
        }
    }

    public async Task RemoveFromGroup(string userName, string userRole)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, userRole);
        await SendMessageToGroup(userRole, $"{userName} has left the group {userRole}.");

        if (userRole == "SuperAdmin")
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admin");
            await SendMessageToGroup("Admin", $"{userName} has left the group Admin.");
        }
    }

    public Task SendPrivateMessage(string user, string message)
    {
        return Clients.User(user).SendAsync("ReceiveMessage", message);
    }
}