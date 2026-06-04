using System.Net.Http.Headers;

namespace AiCv.BffGateway.Endpoints;

public static class ProxyEndpoints
{
    private static readonly string[] ProxiedMethods = ["GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"];

    public static IEndpointRouteBuilder MapProxyEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapMethods("/api/profile/{**catchAll}", ProxiedMethods, ForwardTo("Profile"))
            .RequireAuthorization("require-auth");
        app.MapMethods("/api/opportunity/{**catchAll}", ProxiedMethods, ForwardTo("Opportunity"))
            .RequireAuthorization("require-auth");
        app.MapMethods("/api/cvs/{**catchAll}", ProxiedMethods, ForwardTo("Cv"))
            .RequireAuthorization("require-auth");
        app.MapMethods("/api/cv/{**catchAll}", ProxiedMethods, ForwardTo("Cv"))
            .RequireAuthorization("require-auth");
        app.MapMethods("/api/files/{**catchAll}", ProxiedMethods, ForwardTo("Cv"))
            .RequireAuthorization("require-auth");

        return app;
    }

    private static RequestDelegate ForwardTo(string downstreamServiceName)
    {
        return async context =>
        {
            var configuration = context.RequestServices.GetRequiredService<IConfiguration>();
            var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
            var baseUrl = configuration[$"DownstreamServices:{downstreamServiceName}"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                context.Response.StatusCode = StatusCodes.Status502BadGateway;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = $"Downstream service '{downstreamServiceName}' is not configured."
                });
                return;
            }

            var downstreamPath = context.Request.Path.Value?.StartsWith("/api", StringComparison.OrdinalIgnoreCase) == true
                ? context.Request.Path.Value[4..]
                : context.Request.Path.Value ?? "/";
            var targetUri = new Uri($"{baseUrl.TrimEnd('/')}{downstreamPath}{context.Request.QueryString}");

            using var requestMessage = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUri);
            CopyRequestHeaders(context, requestMessage);

            if (context.Request.ContentLength > 0 || context.Request.Headers.ContainsKey("Transfer-Encoding"))
            {
                requestMessage.Content = new StreamContent(context.Request.Body);
                CopyRequestContentHeaders(context, requestMessage.Content.Headers);
            }

            var client = httpClientFactory.CreateClient();
            using var responseMessage = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

            context.Response.StatusCode = (int)responseMessage.StatusCode;
            CopyResponseHeaders(context, responseMessage);

            if (responseMessage.Content is not null)
            {
                await responseMessage.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
            }
        };
    }

    private static void CopyRequestHeaders(HttpContext context, HttpRequestMessage requestMessage)
    {
        foreach (var header in context.Request.Headers)
        {
            if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
    }

    private static void CopyRequestContentHeaders(HttpContext context, HttpContentHeaders contentHeaders)
    {
        foreach (var header in context.Request.Headers)
        {
            if (header.Key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
            {
                contentHeaders.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }
    }

    private static void CopyResponseHeaders(HttpContext context, HttpResponseMessage responseMessage)
    {
        foreach (var header in responseMessage.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }

        foreach (var header in responseMessage.Content.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }

        context.Response.Headers.Remove("Transfer-Encoding");
    }
}
