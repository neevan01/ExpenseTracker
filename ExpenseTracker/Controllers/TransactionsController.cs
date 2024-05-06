using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExpenseTracker.Models;

namespace ExpenseTracker.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext Context;

        public TransactionsController(ApplicationDbContext context)
        {
            Context = context;
        }

        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = Context.Transactions.Include(t => t.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Transactions/AddOrEdit
        public IActionResult AddOrEdit(int id = 0)
        {
            PopulateCategories();
            if (id == 0)
            {
                return View(new Transaction());
            }
            else
            {
                return View(Context.Transactions.Find(id));
            };
        }

        // POST: Transactions/AddOrEdit
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit([Bind("TransactionId,CategoryId,Amount,Note,Date")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                if (transaction.TransactionId == 0)
                {
                    Context.Add(transaction);
                }
                else
                {
                    Context.Update(transaction);
                }
                await Context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateCategories();
            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await Context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                Context.Transactions.Remove(transaction);
            }

            await Context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [NonAction]
        public void PopulateCategories()
        {
            var categoryList = Context.Categories.ToList();
            Category defaultCategory = new Category() { CategoryId = 0, Title = "Choose a Category" };
            categoryList.Insert(0, defaultCategory);
            ViewBag.Categories = categoryList;
        }
    }
}
