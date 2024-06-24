using contr9.Models;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound("");
        

        var transactions  = await _db.Transactions
            .Where(t => t.SenderUserId == user.Id || t.RecipientUserId == user.Id)
            .Include(t => t.SenderUser)  
            .Include(t => t.RecipientUser)  
            .ToListAsync();

        if (fromDate.HasValue)
            transactions = transactions.Where(t => t.SendTime >= fromDate.Value).ToList();
        if (toDate.HasValue)
            transactions = transactions.Where(t => t.SendTime <= toDate.Value).ToList();
        
        return View(transactions);
    }
    

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ServicePay()
    {
        ViewBag.Services = await _db.Services.ToListAsync();
        return View();
    }

    [HttpPost]
    [Authorize] 
    [ValidateAntiForgeryToken] 
    public async Task<IActionResult> ServicePay(ServicePayViewModel model)
    {
        Transaction transaction = new Transaction();
        if (ModelState.IsValid)
        {
             var currenUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(_userManager.GetUserId(User)));
             if (currenUser == null)
                 return NotFound("Пользователь не найден");
             
             if (currenUser.Balance < model.Amount)
             {
                 ModelState.AddModelError("Amount", "Недостаточно средств для оплаты");
                 ViewBag.Result = "Недостаточно средств для оплаты";
                 ViewBag.Services = await _db.Services.ToListAsync();
                 return View(model);
             }

             ServiceUser? serviceUser = await _db.ServiceUsers.Include(u => u.Service)
                 .FirstOrDefaultAsync(u => u.ServiceId == model.ServiceId && u.UserAccount == model.Account);
             if (serviceUser == null)
             {
                 serviceUser = new ServiceUser()
                 {
                     UserAccount = currenUser.Account,
                     ServiceId = model.ServiceId,
                     Balance = model.Amount
                 };
                 _db.ServiceUsers.Add(serviceUser);
             }
             else
             {
                 serviceUser.Balance += model.Amount;
                 _db.ServiceUsers.Update(serviceUser);
             }
            
             currenUser.Balance -= model.Amount;
             transaction.SenderUserId = currenUser.Id;
             transaction.RecipientUserId = null;
             transaction.SendTime = DateTime.UtcNow.AddHours(6);
             transaction.TransactionAmount = - model.Amount;
             _db.Transactions.Add(transaction);
             _db.Users.Update(currenUser);

             await _db.SaveChangesAsync();
             ViewBag.Result = "Оплата успешно выполнена";
             ViewBag.Services = await _db.Services.ToListAsync();
             return View();
        }
        ViewBag.Services = await _db.Services.ToListAsync();
        return View(model);
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
            transactionSender.TransactionAmount = -model.Amount;
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
    

    
}