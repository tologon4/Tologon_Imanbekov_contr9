using contr9.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace contr9.Controllers;

public class TransactionController : Controller
{
    private WalletDb _db;
    private UserManager<User> _userManager;


    public TransactionController(WalletDb db, UserManager<User> userManager)
    {
        _db = db;
        _userManager = userManager;
    }


    [HttpGet]
    public async Task<IActionResult> Create()
    {
        return View();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TransactionViewModel model)
    {
        User? fromUser = await _userManager.GetUserAsync(User);
        User? toUser = await _db.Users.FirstOrDefaultAsync(u => u.Account == model.RecipientAccountNumber);
        ViewBag.IsCurrentUser = !(fromUser == null);
        if (toUser == null)
        {
            ModelState.AddModelError("RecipientAccountNumber", "Счет получателя не найден");
            ViewBag.Result = "Счет получателя не найден";
            return View(model);
        }
        if (model.Amount < 0)
        {
            ModelState.AddModelError("Amount", "Сумма не может быть отрицательной");
            ViewBag.Result = "Сумма не может быть отрицательной";
            return View(model);
        }
        Transaction transactionSender = new Transaction();
        toUser.Balance += model.Amount;
        Transaction transactionRecipient = new Transaction
        {
            RecipientUserId = toUser.Id,
            SendTime = DateTime.UtcNow.AddHours(6),
            TransactionAmount = model.Amount
        };
        if (fromUser != null)
        {
            if (fromUser.Account == toUser.Account)
            {
                ModelState.AddModelError("RecipientAccountNumber", "Нельзя перевести со своего счета на свой счет");
                ViewBag.Result = "Нельзя перевести со своего счета на свой счет";
                return View(model);
            }
            if (fromUser.Balance < model.Amount)
            {
                ModelState.AddModelError("Amount", "Недостаточно средств на счету");
                ViewBag.Result = "Недостаточно средств на счету";
                return View(model);
            }
            fromUser.Balance -= model.Amount;

            transactionSender.SenderUserId = fromUser.Id;
            transactionSender.SendTime = DateTime.UtcNow.AddHours(6);
            transactionSender.TransactionAmount =-model.Amount;
            transactionSender.RecipientUserId = toUser.Id;
            
            _db.Transactions.Add(transactionSender);
            _db.Users.Update(fromUser);

        }
        _db.Transactions.Add(transactionRecipient);
        _db.Users.Update(toUser);
        await _db.SaveChangesAsync();
        ViewBag.Result = "Перевод выполнен успешно!";
        return View();
    }
    

    public IActionResult Index()
    {
        return View();
    }
}