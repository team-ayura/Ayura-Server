namespace Ayura.API.Features.OTP.DTOs;

public class OtpVerifierDto
{
    // User mobile number and OTP
    public string MobileNumber { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}