namespace AiCv.Api.Tests.Infrastructure;

public static class HttpClientExtensions
{
    public static HttpClient WithTestAuth(
        this HttpClient client,
        string subject = "test-subject",
        string email = "test@example.com",
        string name = "Test User")
    {
        client.DefaultRequestHeaders.Remove("X-Test-Auth");
        client.DefaultRequestHeaders.Remove("X-Test-Subject");
        client.DefaultRequestHeaders.Remove("X-Test-Email");
        client.DefaultRequestHeaders.Remove("X-Test-Name");

        client.DefaultRequestHeaders.Add("X-Test-Auth", "true");
        client.DefaultRequestHeaders.Add("X-Test-Subject", subject);
        client.DefaultRequestHeaders.Add("X-Test-Email", email);
        client.DefaultRequestHeaders.Add("X-Test-Name", name);

        return client;
    }
}
