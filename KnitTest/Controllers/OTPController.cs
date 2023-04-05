using KnitTest.Constants;
using KnitTest.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;
using KnitTest.Services;
using Microsoft.Extensions.Options;
using KnitTest.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace KnitTest.Controllers
{
    public class OTPController : Controller
    {
        private readonly IOTPService _otpService;
        private readonly IOptions<SendGridConfig> _options;
        private readonly ILogger<OTPController> _logger;
        public OTPController(IOTPService otpService, ILogger<OTPController> logger, IOptions<SendGridConfig> options)
        {
            _otpService = otpService;
            _logger = logger;
            _options = options;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("otp/getotp")]
        public async Task<ActionResult> GetOTP(string email)
        {
            try
            {
                string otp = Generate_OTP_email(email);
                if (string.IsNullOrWhiteSpace(otp))
                {
                    string msgErr = "email is invalid";
                    return new JsonResult(msgErr);
                }
                //insert OTP to db
                bool created = await _otpService.InsertOTP(new OTPCheck
                {
                    CreatedDate = DateTime.Now,
                    Email = email,
                    OTP = otp,
                });
                if (created)
                {
                    string message = string.Format($"Your OTP code is {otp}. The code is valid for 1 minute.");
                    var status = await SendOTP(email, message);
                    await _otpService.UpdateStatus(otp, email, status);
                    return new JsonResult(status);
                }
                return View("Error");
            }
            catch
            {
                return View("Error");
            }
        }

        [HttpPost]
        [Route("otp/checkotp")]
        public async Task<ActionResult> CheckOTP(string otpcode, string email)
        {
            var otpcheck = await _otpService.GetOTP(otpcode, email);
            if (otpcheck == null)
            {
                return new JsonResult("please retry");
            }
            if (otpcheck.CreatedDate.AddSeconds(60) < DateTime.Now)
            {
                return new JsonResult(OTPStatus.STATUS_OTP_FAIL);
            }
            return new JsonResult(OTPStatus.STATUS_OTP_OK);
        }

        /// <summary>
        /// generate OTP and check if email domain is valid
        /// </summary>
        /// <param name="user_email"></param>
        /// <returns></returns>
        private protected static string Generate_OTP_email(string user_email)
        {
            //validate user email
            bool matched = Regex.IsMatch(user_email, ".dso.org.sg");
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
        private async Task<EmailStatus> SendOTP(string email, string message)
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
                _logger.LogError(ex, ex.Message);
                return EmailStatus.STATUS_EMAIL_FAIL;
            }

        }
    }
}
