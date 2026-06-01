using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace AiCv.CvService.Modules.Cvs.Storage;

public sealed class CvObjectStorage : ICvObjectStorage
{
    private const string Region = "us-east-1";
    private const string Service = "s3";

    private readonly HttpClient _httpClient;
    private readonly string _endpoint;
    private readonly string _accessKey;
    private readonly string _secretKey;
    private readonly string _bucketName;

    public CvObjectStorage(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _endpoint = NormalizeEndpoint(configuration["Minio:Endpoint"] ?? "localhost:9000");
        _accessKey = configuration["Minio:AccessKey"] ?? "minioadmin";
        _secretKey = configuration["Minio:SecretKey"] ?? "minioadmin";
        _bucketName = configuration["Minio:BucketName"] ?? "generated-cvs";
    }

    public string BucketName => _bucketName;

    public async Task UploadPdfAsync(string objectKey, byte[] content, CancellationToken cancellationToken = default)
    {
        await EnsureBucketAsync(cancellationToken);

        var objectUri = BuildUri(_bucketName, objectKey);
        using var request = new HttpRequestMessage(HttpMethod.Put, objectUri)
        {
            Content = new ByteArrayContent(content)
        };
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        Sign(request, content);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<byte[]?> GetPdfAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        var objectUri = BuildUri(_bucketName, objectKey);
        using var request = new HttpRequestMessage(HttpMethod.Get, objectUri);
        Sign(request, []);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    private async Task EnsureBucketAsync(CancellationToken cancellationToken)
    {
        var bucketUri = BuildUri(_bucketName, "");
        using var headRequest = new HttpRequestMessage(HttpMethod.Head, bucketUri);
        Sign(headRequest, []);

        using var headResponse = await _httpClient.SendAsync(headRequest, cancellationToken);
        if (headResponse.IsSuccessStatusCode)
        {
            return;
        }

        if (headResponse.StatusCode != HttpStatusCode.NotFound)
        {
            headResponse.EnsureSuccessStatusCode();
        }

        using var createRequest = new HttpRequestMessage(HttpMethod.Put, bucketUri);
        Sign(createRequest, []);

        using var createResponse = await _httpClient.SendAsync(createRequest, cancellationToken);
        if (createResponse.StatusCode == HttpStatusCode.Conflict)
        {
            return;
        }

        createResponse.EnsureSuccessStatusCode();
    }

    private Uri BuildUri(string bucket, string objectKey)
    {
        var encodedKey = string.Join("/", objectKey
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.EscapeDataString));

        var path = string.IsNullOrWhiteSpace(encodedKey)
            ? $"{bucket}"
            : $"{bucket}/{encodedKey}";

        return new Uri($"{_endpoint}/{path}");
    }

    private void Sign(HttpRequestMessage request, byte[] payload)
    {
        var now = DateTimeOffset.UtcNow;
        var amzDate = now.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);
        var dateStamp = now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var payloadHash = ToHex(SHA256.HashData(payload));
        var host = request.RequestUri!.Authority;

        request.Headers.Remove("Host");
        request.Headers.TryAddWithoutValidation("Host", host);
        request.Headers.Remove("x-amz-content-sha256");
        request.Headers.TryAddWithoutValidation("x-amz-content-sha256", payloadHash);
        request.Headers.Remove("x-amz-date");
        request.Headers.TryAddWithoutValidation("x-amz-date", amzDate);

        var canonicalUri = request.RequestUri.AbsolutePath;
        var canonicalHeaders = $"host:{host}\nx-amz-content-sha256:{payloadHash}\nx-amz-date:{amzDate}\n";
        const string signedHeaders = "host;x-amz-content-sha256;x-amz-date";
        var canonicalRequest = string.Join("\n", new[]
        {
            request.Method.Method,
            canonicalUri,
            "",
            canonicalHeaders,
            signedHeaders,
            payloadHash
        });

        var credentialScope = $"{dateStamp}/{Region}/{Service}/aws4_request";
        var stringToSign = string.Join("\n", new[]
        {
            "AWS4-HMAC-SHA256",
            amzDate,
            credentialScope,
            ToHex(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalRequest)))
        });

        var signingKey = GetSignatureKey(_secretKey, dateStamp, Region, Service);
        var signature = ToHex(HmacSha256(signingKey, stringToSign));
        var authorization =
            $"AWS4-HMAC-SHA256 Credential={_accessKey}/{credentialScope}, SignedHeaders={signedHeaders}, Signature={signature}";

        request.Headers.Remove("Authorization");
        request.Headers.TryAddWithoutValidation("Authorization", authorization);
    }

    private static byte[] GetSignatureKey(string key, string dateStamp, string regionName, string serviceName)
    {
        var kDate = HmacSha256(Encoding.UTF8.GetBytes($"AWS4{key}"), dateStamp);
        var kRegion = HmacSha256(kDate, regionName);
        var kService = HmacSha256(kRegion, serviceName);
        return HmacSha256(kService, "aws4_request");
    }

    private static byte[] HmacSha256(byte[] key, string data)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    private static string ToHex(byte[] bytes)
    {
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string NormalizeEndpoint(string endpoint)
    {
        return endpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               endpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? endpoint.TrimEnd('/')
            : $"http://{endpoint.TrimEnd('/')}";
    }
}
