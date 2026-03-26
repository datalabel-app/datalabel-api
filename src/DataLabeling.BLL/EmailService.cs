using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLabeling.BLL
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly string _replyToEmail;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiKey = _configuration["EmailSettings:Password"] ?? throw new ArgumentNullException("SendGrid API Key is not configured");
            _senderEmail = _configuration["EmailSettings:Username"] ?? throw new ArgumentNullException("Sender email is not configured");
            _senderName = _configuration["EmailSettings:Sender"] ?? "Data Labeling";
            _replyToEmail = _configuration["EmailSettings:ReplyTo"] ?? "diodo.spsp@gmail.com";
        }

        public async Task<bool> SendAccountCreationEmailAsync(string recipientEmail, string fullName, string password)
        {
            var subject = "Tài khoản Data Labeling của bạn đã được tạo";
            var htmlContent = EmailTemplate.GetAccountCreationEmail(fullName, recipientEmail, password);
            var plainTextContent = $"Xin chào {fullName},\n\nTài khoản của bạn đã được tạo.\nEmail: {recipientEmail}\nMật khẩu: {password}\n\nVui lòng đổi mật khẩu sau khi đăng nhập lần đầu.";

            return await SendEmailAsync(recipientEmail, fullName, subject, htmlContent, plainTextContent);
        }

        public async Task<bool> SendForgotPasswordOtpEmailAsync(string recipientEmail, string fullName, string otp)
        {
            var subject = "Mã OTP đặt lại mật khẩu - Data Labeling";
            var htmlContent = EmailTemplate.GetForgotPasswordOtpEmail(fullName, otp);
            var plainTextContent = $"Xin chào {fullName},\n\nMã OTP của bạn là: {otp}\n\nMã này có hiệu lực trong 5 phút.\n\nNếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.";

            return await SendEmailAsync(recipientEmail, fullName, subject, htmlContent, plainTextContent);
        }

        public async Task<bool> SendTaskReadyForReviewEmailAsync(string recipientEmail, string fullName, int taskId, string datasetName, string roundDescription)
        {
            var subject = "Task sẵn sàng để review - Data Labeling";
            var htmlContent = EmailTemplate.GetTaskReadyForReviewEmail(fullName, taskId, datasetName, roundDescription);
            var plainTextContent = $"Xin chào {fullName},\n\nTask #{taskId} đã được annotate xong và sẵn sàng để bạn review.\n\nDataset: {datasetName}\nMô tả: {roundDescription}\n\nVui lòng đăng nhập vào hệ thống để bắt đầu review.";

            return await SendEmailAsync(recipientEmail, fullName, subject, htmlContent, plainTextContent);
        }

        public async Task<bool> SendEmailAsync(string recipientEmail, string recipientName, string subject, string htmlContent, string plainTextContent)
        {
            try
            {
                var client = new SendGridClient(_apiKey);
                var from = new EmailAddress(_senderEmail, _senderName);
                var to = new EmailAddress(recipientEmail, recipientName);
                
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                msg.ReplyTo = new EmailAddress(_replyToEmail);

                var response = await client.SendEmailAsync(msg);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                return false;
            }
        }
    }
}
