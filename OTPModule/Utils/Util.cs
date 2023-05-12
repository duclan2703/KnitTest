using OTPModule.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Options;
using OTPModule.Models;

namespace OTPModule.Utils
{
    public class OTPUtil
    {
        private readonly IOptions<SendGridConfig> _options;
        private const string EMAIL_REGEX = @"^[a-zA-Z0-9._+-]+@dso\.org\.sg$";

        public OTPUtil(IOptions<SendGridConfig> options)
        {
            _options = options;
        }

        /// <summary>
        /// generate OTP and check if email domain is valid
        /// </summary>
        /// <param name="user_email"></param>
        /// <returns></returns>
        public string Generate_OTP_email(string user_email)
        {
            //validate user email
            bool matched = Regex.IsMatch(user_email, EMAIL_REGEX);
#if DEBUG
            matched = true;
#endif
            if (matched)
            {
                Random rd = new Random();
                return (rd.Next(100000, 999999)).ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// send OTP to recipient
        /// </summary>
        /// <param name="email"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task<EmailStatus> SendOTPAsync(string email, string message)
        {
            try
            {
                var sendGridConfig = _options.Value;
                var client = new SendGridClient(sendGridConfig.APIKey);
                var msg = MailHelper.CreateSingleEmail(new EmailAddress("langosu1995@gmail.com"), new EmailAddress(email), "OTP Code", message, message);
                var response = await client.SendEmailAsync(msg);
                if (response.StatusCode == HttpStatusCode.Accepted)
                    return EmailStatus.STATUS_EMAIL_OK;
                return EmailStatus.STATUS_EMAIL_INVALID;
            }
            catch (Exception ex)
            {
                return EmailStatus.STATUS_EMAIL_FAIL;
            }
        }
    }
}
