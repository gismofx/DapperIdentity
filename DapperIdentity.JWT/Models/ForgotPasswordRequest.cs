using System.ComponentModel.DataAnnotations;


namespace DapperIdentity.JWT.Models
{
    public class ForgotPasswordRequest
    {
        [Required]
        public string? Email { get; set; }
    }
}
