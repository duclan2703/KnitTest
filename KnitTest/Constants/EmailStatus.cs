namespace KnitTest.Constants
{
    public enum EmailStatus
    {
        /// <summary>
        /// email containing OTP has been sent successfully
        /// </summary>
        STATUS_EMAIL_OK = 1,
        /// <summary>
        /// email address does not exist or sending to the email has failed
        /// </summary>
        STATUS_EMAIL_FAIL = 2,
        /// <summary>
        /// email address is invalid
        /// </summary>
        STATUS_EMAIL_INVALID = 3
    }

    public enum OTPStatus
    {
        /// <summary>
        /// OTP is valid and checked
        /// </summary>
        STATUS_OTP_OK = 1,
        /// <summary>
        /// OTP is wrong after 10 tries
        /// </summary>
        STATUS_OTP_FAIL = 2,
        /// <summary>
        /// timeout after 1 min
        /// </summary>
        STATUS_OTP_TIMEOUT = 3
    }
}