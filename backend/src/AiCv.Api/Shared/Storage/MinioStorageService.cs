using Minio;
using Minio.DataModel.Args;

namespace AiCv.Api.Shared.Storage;

public class MinioStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName = "cv-assets";

    public MinioStorageService(IConfiguration configuration)
    {
        _minioClient = new MinioClient()
            .WithEndpoint(configuration["Minio:Endpoint"])
            .WithCredentials(configuration["Minio:AccessKey"], configuration["Minio:SecretKey"])
            .Build();
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        // 1. Créer le bucket (dossier principal) s'il n'existe pas
        var beArgs = new BucketExistsArgs().WithBucket(_bucketName);
        bool found = await _minioClient.BucketExistsAsync(beArgs);
        if (!found)
        {
            var mbArgs = new MakeBucketArgs().WithBucket(_bucketName);
            await _minioClient.MakeBucketAsync(mbArgs);
        }

        // 2. Générer un nom de fichier unique
        var uniqueFileName = $"{Guid.NewGuid()}-{fileName}";

        // 3. Uploader le fichier
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(uniqueFileName)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putObjectArgs);

        // 4. Retourner le chemin d'accès
        return $"{_bucketName}/{uniqueFileName}";
    }
}