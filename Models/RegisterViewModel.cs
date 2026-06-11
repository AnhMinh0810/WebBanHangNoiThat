using System.ComponentModel.DataAnnotations;

namespace furniture_store.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Họ và tên không được để trống")]
    [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [StringLength(150, ErrorMessage = "Email không được vượt quá 150 ký tự")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải dài tối thiểu 6 ký tự")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
}
