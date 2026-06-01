namespace AiCv.CvService.Modules.Cvs.Storage;

public interface ICvObjectStorage
{
    string BucketName { get; }
    Task UploadPdfAsync(string objectKey, byte[] content, CancellationToken cancellationToken = default);
    Task<byte[]?> GetPdfAsync(string objectKey, CancellationToken cancellationToken = default);
}
