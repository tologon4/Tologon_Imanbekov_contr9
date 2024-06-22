using System.ComponentModel.DataAnnotations;

namespace contr9.Models;

public class ServicePayViewModel
{
    [Required(ErrorMessage = "Заполните поле!")]
    public int ServiceId { get; set; }

    [Required(ErrorMessage = "Заполните поле!")]
    public string Account { get; set; }

    [Required(ErrorMessage = "Заполните поле!")]
    [Range(0, double.MaxValue, ErrorMessage = "Сумма должна быть больше нуля")]
    public decimal Amount { get; set; }
}