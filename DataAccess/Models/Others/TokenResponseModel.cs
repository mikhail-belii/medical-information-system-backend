using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models.Others;

public class TokenResponseModel
{
    [Required]
    public string Token { get; set; }
}