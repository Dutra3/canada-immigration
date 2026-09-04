using System.Net;

namespace CanadaImmigration.Api.Tests.TestHelpers;

// Simula respostas HTTP sem fazer nenhuma chamada de rede de verdade.
// Você passa uma função que decide o que responder pra cada request.
public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

    public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responder = responder;
    }

    public static FakeHttpMessageHandler ReturningJson(string json, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new FakeHttpMessageHandler(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        });
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_responder(request));
    }
}
