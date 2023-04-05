using KnitTest.Constants;
using KnitTest.Entities;
using Microsoft.EntityFrameworkCore;

namespace KnitTest.Services
{
    public interface IOTPService
    {
        Task<bool> InsertOTP(OTPCheck entity);
        Task<OTPCheck> GetOTP(string otpcode, string email);
        Task<bool> UpdateStatus(string otpcode, string email, EmailStatus status);
    }

    public class OTPService : IOTPService
    {
        private readonly OTPContext _context;
        private readonly ILogger<OTPService> _logger;
        public OTPService(OTPContext context, ILogger<OTPService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<OTPCheck> GetOTP(string otpcode, string email)
        {
            var otpck = await _context.OTPChecks
                .Where(x => x.OTP.Equals(otpcode) && x.Email.Equals(email))
                .OrderBy(x => x.CreatedDate)
                .FirstOrDefaultAsync();
            if (otpck == null) { return null; }
            return otpck;
        }

        public async Task<bool> InsertOTP(OTPCheck entity)
        {
            try
            {
                _context.Add(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }

        }

        public async Task<bool> UpdateStatus(string otpcode, string email, EmailStatus status)
        {
            try
            {
                var otpck = await _context.OTPChecks
                    .Where(x => x.OTP.Equals(otpcode) && x.Email.Equals(email))
                    .OrderBy(x => x.CreatedDate)
                    .FirstOrDefaultAsync();
                if (otpck == null) { return false; }
                otpck.SendStatus = status;
                _context.Update(otpck);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }

        }
    }
}
