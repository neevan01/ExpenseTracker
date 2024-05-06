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
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext Context;

        public CategoriesController(ApplicationDbContext context)
        {
            Context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            return View(await Context.Categories.ToListAsync());
        }

        // GET: Categories/Create
        public IActionResult AddOrEdit(int id = 0)
        {
            if (id == 0)
            {
                return View(new Category());
            }
            else
            {
                return View(Context.Categories.Find(id));
            }
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit([Bind("CategoryId,Title,Icon,Type")] Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.CategoryId == 0)
                {
                    Context.Add(category);
                }
                else
                {
                    Context.Update(category);
                }
                await Context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await Context.Categories.FindAsync(id);
            if (category != null)
            {
                Context.Categories.Remove(category);
            }

            await Context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
