using System.ComponentModel.DataAnnotations;


namespace CPE.DapperIdentity.JWT.Models
{
    public class ForgotPasswordRequest
    {
        [Required]
        public string? Email { get; set; }
    }
}
