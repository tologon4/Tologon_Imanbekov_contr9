namespace contr9.Models;

public class Transaction
{
    public int Id { get; set; }
    public int? SenderUserId { get; set; }
    public User? SenderUser { get; set; }
    public int RecipientUserId { get; set; }
    public User? RecipientUser { get; set; }
    public DateTime? SendTime { get; set; }
    public decimal TransactionAmount { get; set; }
}