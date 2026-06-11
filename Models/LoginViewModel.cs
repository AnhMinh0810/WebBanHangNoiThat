using System.ComponentModel.DataAnnotations;

namespace furniture_store.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
}
