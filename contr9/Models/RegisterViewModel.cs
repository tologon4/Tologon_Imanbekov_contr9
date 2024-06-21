using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace contr9.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Заполните поле Email")]
    [Remote(action: "CheckEmail", controller:"Validation", ErrorMessage = "Данный Email уже занят!")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Заполните поле Phone Number")]
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Заполните в формате x-цифра(0-9): 0 xxx xx xx xx")]
    public string PhoneNumber { get; set; }
    [Required(ErrorMessage = "Заполните поле Username")]
    [Remote(action: "CheckUsername", controller:"Validation", ErrorMessage = "Данный UserName уже занят!")]
    public string UserName { get; set; }
    [Required(ErrorMessage = "Заполните поле Password")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    [Required(ErrorMessage = "Заполните поле Confirm Password")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Неверный Confirm Password, попробуйте еще раз!")]
    public string ConfirmPassword { get; set; }
}