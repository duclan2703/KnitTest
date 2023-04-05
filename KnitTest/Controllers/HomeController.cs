using KnitTest.Constants;
using KnitTest.Entities;
using KnitTest.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.Http;

namespace KnitTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOTPService _otpService;
        private readonly ILogger<HomeController> _logger;
        public HomeController(IOTPService otpService, ILogger<HomeController> logger)
        {
            _otpService = otpService;
            _logger = logger;
        }

        public ActionResult GetOTP()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GetOTP(string email)
        {
            try
            {
                string otp = Generate_OTP_email(email);
                if (string.IsNullOrWhiteSpace(otp))
                {
                    string msgErr = "email not valid";
                    return new JsonResult(msgErr);
                }
                bool created = await _otpService.InsertOTP(new OTPCheck
                {
                    CreatedDate = DateTime.Now,
                    Email = email,
                    OTP = otp,
                });
                if (created)
                {
                    string message = string.Format($"Your OTP code is {otp}. The code is valid for 1 minute.");
                    var status = SendOTP(email, message);
                    await _otpService.UpdateStatus(otp, email, status);
                }
                return View();
            }
            catch
            {
                return View("Error");
            }
        }

        public ActionResult CheckOTP(string email)
        {
            return View();
        }

        public async Task<ActionResult> CheckOTP(string otpcode, string email)
        {
            var otpcheck = await _otpService.GetOTP(otpcode, email);
            if (otpcheck == null)
            {
                return View("Error");
            }
            if (otpcheck.CreatedDate.AddSeconds(60) < DateTime.Now)
            {
                return new JsonResult(OTPStatus.STATUS_OTP_FAIL);
            }
            return new JsonResult(OTPStatus.STATUS_OTP_OK);
        }

        private protected static string Generate_OTP_email(string user_email)
        {
            //validate user email
            bool matched = Regex.IsMatch(user_email, ".dso.org.sg");
            if (matched)
            {
                Random rd = new Random();
                return (rd.Next(100000, 999999)).ToString();
            }
            return string.Empty;
        }

        private EmailStatus SendOTP(string email, string message)
        {
            try
            {
                var smtpclient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("langosu1995@gmail.com", "nishikinomaki1904"),
                    EnableSsl = true,
                };
                var mailMsg = new MailMessage
                {
                    From = new MailAddress("langosu1995@gmail.com"),
                    Subject = "OTP Code",
                    Body = $"<h1>{message}</h1>",
                    IsBodyHtml = true,
                };
                mailMsg.To.Add(email);
                smtpclient.Send(mailMsg);
                return EmailStatus.STATUS_EMAIL_OK;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return EmailStatus.STATUS_EMAIL_FAIL;
            }

        }
    }
}
