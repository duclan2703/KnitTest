using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OTPModule.Constants;
using OTPModule.Entities;
using OTPModule.Models;
using OTPModule.Services;
using OTPModule.Utils;
using Xunit;

namespace UnitTests
{
    public class OTPTest
    {
        private readonly OTPUtil _util;
        private readonly IOptions<SendGridConfig> _options;
        private readonly IOTPService _otpService;
        private readonly ServiceProvider _serviceProvider;
        const string msg = "Your OTP code is {0}. The code is valid for 1 minute.";

        public OTPTest()
        {
            var services = new ServiceCollection();
            services.AddOptions<SendGridConfig>();
            services.AddTransient<IOTPService, OTPService>();
            services.AddDbContext<OTPContext>();
            _serviceProvider = services.BuildServiceProvider();
            _otpService = _serviceProvider.GetRequiredService<IOTPService>();
        }

        [Theory]
        [InlineData("user@dso.org.sg", EmailStatus.STATUS_EMAIL_OK)]
        [InlineData("user@example.com", EmailStatus.STATUS_EMAIL_INVALID)]
        public async Task GenerateOTPEmail_ValidatesEmail_SendEmail(string email, EmailStatus expectedStatus)
        {
            // Act
            var otp = _util.Generate_OTP_email(email);
            var result = await _util.SendOTPAsync(email, string.Format(msg, otp));

            // Assert
            Assert.Equal(expectedStatus, result);
        }

        [Fact]
        public async Task CheckOTPSuccess_Fail()
        {
            string otp = "660781";

            // Mock email sending and set OTP directly
            var sendEmailresult = await _util.SendOTPAsync("duclan2703@gmail.com", string.Format(msg, "otp"));

            // Act
            var otpcheck = await _otpService.GetOTP(otp, "duclan2703@gmail.com");
            int retry = 1;
            var result = OTPStatus.STATUS_OTP_TIMEOUT;
            while (retry < 10)
            {
                result = CheckOTP(otpcheck, retry);
                if (result == OTPStatus.STATUS_OTP_OK)
                {
                    break;
                }
                retry++;
            }

            // Assert
            Assert.Equal(OTPStatus.STATUS_OTP_OK, result);
            Assert.Equal(OTPStatus.STATUS_OTP_FAIL, result);
        }

        private OTPStatus CheckOTP(OTPCheck otpcheck, int attempts)
        {
            if (otpcheck.CreatedDate.AddSeconds(60) < DateTime.UtcNow && attempts == 10)
            {
                return OTPStatus.STATUS_OTP_FAIL;
            }
            return OTPStatus.STATUS_OTP_OK;
        }
    }
}
