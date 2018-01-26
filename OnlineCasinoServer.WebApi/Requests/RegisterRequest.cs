using System.ComponentModel.DataAnnotations;

namespace OnlineCasinoServer.WebApi.Requests
{
    public class RegisterRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        [MinLength(6)]
        [MaxLength(32)]
        public string Password { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Email { get; set; }
    }
}