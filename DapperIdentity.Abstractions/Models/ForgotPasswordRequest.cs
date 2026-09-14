using System.ComponentModel.DataAnnotations;


namespace CPE.DapperIdentity.Abstractions.Models
{
    public class ForgotPasswordRequest
    {
        [Required]
        public string? Email { get; set; }
    }
}
