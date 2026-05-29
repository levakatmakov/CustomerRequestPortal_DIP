using System.ComponentModel.DataAnnotations;

namespace CustomerRequestPortal.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "ФИО обязательно")]
        [Display(Name = "ФИО")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = string.Empty;

    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "ФИО обязательно")]
        [Display(Name = "ФИО")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Роль обязательна")]
        [Display(Name = "Роль")]
        public string Role { get; set; } = "Executor";

        [Display(Name = "Должность")]
        public string? StaffPosition { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }
}
