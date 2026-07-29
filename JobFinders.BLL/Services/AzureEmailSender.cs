
using Azure.Communication.Email;

using Microsoft.Extensions.Configuration;

namespace JobFinders.BLL.Services
{
    public class AzureEmailSender
    {
        private readonly IConfiguration _configuration;

        public AzureEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(string? to, string? subject, string? body)
        {
            var sender = _configuration["AzureEmailSender"];
            var connectionString = _configuration["AzureCommunicationService"];

            var client = new EmailClient(connectionString);

            EmailSendOperation? operation;

            try
            {
                operation = await client.SendAsync(
                    Azure.WaitUntil.Completed,
                    sender,
                    to,
                    subject,
                    body
                );
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return operation?.Value.Status == EmailSendStatus.Succeeded;
        }
    }
}
