using Microsoft.AspNetCore.SignalR;
using PatientAnalytics.Models;

namespace PatientAnalytics.Hubs;

public class PatientHub : Hub
{
    public async Task SendNewPatient(Patient patient)
    {
        await Clients.All.SendAsync("ReceiveNewPatient", patient);
    }

    public async Task SendUpdatedPatient(Patient patient)
    {
        await Clients.All.SendAsync("ReceiveUpdatedPatient", patient);
    }

    public async Task SendDeletedPatient(Patient patient)
    {
        await Clients.All.SendAsync("ReceiveDeletedPatient", patient);
    }

    public async Task TestSendMessage(string message)
    {
        await Clients.All.SendAsync("TestReceiveMessage", message);
    }
}