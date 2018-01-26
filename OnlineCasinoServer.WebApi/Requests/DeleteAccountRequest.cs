using System.ComponentModel.DataAnnotations;

namespace OnlineCasinoServer.WebApi.Requests
{
    public class DeleteAccountRequest
    {
        [Required]
        [MinLength(6)]
        [MaxLength(32)]
        public string Password { get; set; }
    }
}