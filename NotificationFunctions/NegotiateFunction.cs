using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Extensions.SignalRService;

namespace NotificationFunctions;

public sealed class NegotiateFunction
{
    [Function("negotiate")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData request,
        [SignalRConnectionInfoInput(HubName = "notifications")] SignalRConnectionInfo connectionInfo)
    {
        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new
        {
            url = connectionInfo.Url,
            accessToken = connectionInfo.AccessToken
        });

        return response;
    }
}
