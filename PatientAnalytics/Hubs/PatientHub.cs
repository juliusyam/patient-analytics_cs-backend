using Microsoft.AspNetCore.SignalR;

namespace PatientAnalytics.Hubs;

public class PatientHub : Hub
{ 
    public async Task TestSendMessage(string message)
    {
        await Clients.All.SendAsync("TestReceiveMessage", message);
    }
    
    public async Task AddToGroup(string username, string userRole)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, userRole);
        
        if (userRole == "SuperAdmin") 
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admin");

        await Clients.GroupExcept(userRole, Context.ConnectionId).SendAsync("UserLoggedIn", username);
    }
    
    public async Task RemoveFromGroup(string username, string userRole)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, userRole);
        
        if (userRole == "SuperAdmin")
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admin");
        
        await Clients.GroupExcept(userRole, Context.ConnectionId).SendAsync("UserLoggedOut", username);
    }
}