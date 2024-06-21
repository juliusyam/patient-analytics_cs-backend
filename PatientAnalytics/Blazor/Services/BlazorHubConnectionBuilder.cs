using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;

namespace PatientAnalytics.Blazor.Services;

public class BlazorHubConnectionBuilder
{
    private readonly IHubConnectionBuilder _hubConnectionBuilder;
    
    public BlazorHubConnectionBuilder(System.Uri uri, string token)
    {
        _hubConnectionBuilder = new HubConnectionBuilder()
            .WithUrl(uri, options =>
            {
                options.SkipNegotiation = true;
                options.Transports = HttpTransportType.WebSockets;
                options.AccessTokenProvider = () => Task.FromResult(token);
            })
            .AddJsonProtocol(protocolOptions =>
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };

                jsonOptions.Converters.Add(new JsonStringEnumConverter());

                protocolOptions.PayloadSerializerOptions = jsonOptions;
            });
    }

    public HubConnection Build()
    {
        return _hubConnectionBuilder.Build();
    }
}