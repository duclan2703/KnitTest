using OTPModule.Constants;
using System.ComponentModel.DataAnnotations;

namespace OTPModule.Entities
{
    public class OTPCheck
    {
        [Required]
        public Guid Id { get; set; } = Guid.NewGuid();
        [StringLength(100)]
        public string Email { get; set; }
        [StringLength(6)]
        public string OTP { get; set; }
        public DateTime CreatedDate { get; set;} = DateTime.Now;
        public EmailStatus SendStatus { get; set; } 
    }
}
