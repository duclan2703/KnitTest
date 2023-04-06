using OTPModule.Constants;
using OTPModule.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;
using OTPModule.Entities;
using Microsoft.Extensions.Options;
using OTPModule.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using OTPModule.Utils;

namespace KnitTest.Controllers
{
    public class OTPController : Controller
    {
        private readonly IOTPService _otpService;
        private readonly IOptions<SendGridConfig> _options;
        private readonly ILogger<OTPController> _logger;
        private OTPUtil _otpUtil;

        public OTPController(IOTPService otpService, ILogger<OTPController> logger, IOptions<SendGridConfig> options)
        {
            _otpService = otpService;
            _logger = logger;
            _options = options;
            _otpUtil = new OTPUtil(_options);
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
                string otp = _otpUtil.Generate_OTP_email(email);
                if (string.IsNullOrWhiteSpace(otp))
                {
                    string msgErr = "email is invalid";
                    return new JsonResult(msgErr);
                }
                //insert OTP to db
                bool created = await _otpService.InsertOTP(new OTPCheck
                {
                    CreatedDate = DateTime.UtcNow,
                    Email = email,
                    OTP = otp,
                });
                if (created)
                {
                    string message = string.Format($"Your OTP code is {otp}. The code is valid for 1 minute.");
                    var status = await _otpUtil.SendOTPAsync(email, message);
                    await _otpService.UpdateStatus(otp, email, status);
                    return new JsonResult(status.ToString());
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
        public async Task<ActionResult> CheckOTP(string otpcode, string email, int attempts)
        {
            var otpcheck = await _otpService.GetOTP(otpcode, email);
            if (otpcheck == null)
            {
                return new JsonResult("please retry");
            }
            if (otpcheck.CreatedDate.AddSeconds(60) < DateTime.UtcNow && attempts == 10)
            {
                return new JsonResult(OTPStatus.STATUS_OTP_FAIL.ToString());
            }
            return new JsonResult(OTPStatus.STATUS_OTP_OK);
        }
    }
}
