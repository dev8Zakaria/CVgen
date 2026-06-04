namespace AiCv.CvService.Modules.Messaging;

public sealed class RabbitMqOptions
{
    public bool Enabled { get; set; }
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "aicv.events";
    public string AuditQueueName { get; set; } = "aicv.audit";
    public string CvGenerationQueueName { get; set; } = "cv.generation.requests";
}
