using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Extensions.SignalRService;

namespace NotificationFunctions;

public sealed class NotifyFunction
{
    [Function("notify")]
    [SignalROutput(HubName = "notifications")]
    public async Task<SignalRMessageAction> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
    {
        var notifyRequest = await JsonSerializer.DeserializeAsync<NotifyRequest>(
            request.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new NotifyRequest();

        var message = string.IsNullOrWhiteSpace(notifyRequest.Message)
            ? "Leeg bericht"
            : notifyRequest.Message;

        var sender = string.IsNullOrWhiteSpace(notifyRequest.Sender)
            ? "function-app"
            : notifyRequest.Sender;

        return new SignalRMessageAction("ReceiveMessage")
        {
            Arguments = new object[]
            {
                message,
                sender,
                DateTimeOffset.UtcNow
            }
        };
    }

    private sealed class NotifyRequest
    {
        public string Message { get; init; } = string.Empty;
        public string Sender { get; init; } = string.Empty;
    }
}
