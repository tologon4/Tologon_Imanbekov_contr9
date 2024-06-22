using System.ComponentModel.DataAnnotations;

namespace contr9.Models;

public class TransactionViewModel
{
    [Required(ErrorMessage = "Введите номер счета получателя!")]
    public string RecipientAccountNumber { get; set; }
    public string? SenderAccountNumber { get; set; }

    [Required(ErrorMessage = "Введите сумму перевода!")]
    [Range(1, double.MaxValue, ErrorMessage = "Сумма перевода должна быть больше нуля")]
    public decimal Amount { get; set; }
}