using Finamon.Service.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Finamon.Service.Services
{

    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly string _templatePath;

        public EmailTemplateService()
        {
            _templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
        }

        public async Task<string> GetAccountEmailTemplateAsync(string email, string password)
        {
            var template = await File.ReadAllTextAsync(Path.Combine(_templatePath, "AccountEmailTemplate.html"));
            return template
                .Replace("{email}", email)
                .Replace("{password}", password)
                .Replace("{year}", DateTime.Now.Year.ToString());
        }

        public async Task<string> GetPasswordResetTemplateAsync(string email, string password)
        {
            var template = await File.ReadAllTextAsync(Path.Combine(_templatePath, "PasswordResetTemplate.html"));
            return template
                .Replace("{email}", email)
                .Replace("{password}", password)
                .Replace("{year}", DateTime.Now.Year.ToString());
        }

        public async Task<string> GetVerificationEmailTemplateAsync(string verificationCode)
        {
            var template = await File.ReadAllTextAsync(Path.Combine(_templatePath, "VerificationEmailTemplate.html"));
            return template
                .Replace("{verificationCode}", verificationCode)
                .Replace("{year}", DateTime.Now.Year.ToString());
        }
    }
} 