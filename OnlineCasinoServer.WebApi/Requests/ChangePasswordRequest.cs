using System.ComponentModel.DataAnnotations;

namespace OnlineCasinoServer.WebApi.Requests
{
    public class ChangePasswordRequest
    {
        [Required]
        [MinLength(6)]
        [MaxLength(32)]
        public string OldPassword { get; set; }
        [Required]
        [MinLength(6)]
        [MaxLength(32)]
        public string NewPassword { get; set; }
    }
}