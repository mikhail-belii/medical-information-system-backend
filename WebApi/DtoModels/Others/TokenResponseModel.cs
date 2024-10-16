using System.ComponentModel.DataAnnotations;

namespace WebApi.DtoModels.Others;

public class TokenResponseModel
{
    [Required]
    public string Token { get; set; }
}