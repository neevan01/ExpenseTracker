using ExpenseTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ExpenseTracker.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext Context;
        public DashboardController(ApplicationDbContext context)
        {
            Context = context;
        }
        public async Task<ActionResult> Index()
        {
            //last 7 days 
            DateTime startDate = DateTime.Today.AddDays(-6);
            DateTime endDate = DateTime.Today;

            List<Transaction> transactions = await Context.Transactions
                .Include(x => x.Category)
                .Where(y => y.Date >= startDate && y.Date <= endDate).ToListAsync();

            //total income
            decimal totalIncome = transactions.Where(x => x.Category.Type == "Income")
                .Sum(y => y.Amount);
            ViewBag.TotalIncome = totalIncome.ToString("C2");

            decimal totalExpense = transactions.Where(x => x.Category.Type == "Expense")
                .Sum(y => y.Amount);
            ViewBag.TotalExpense = totalExpense.ToString("C2");

            //Balance
            decimal balance = totalIncome - totalExpense;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1;
            ViewBag.Balance = string.Format(culture, "{0:C2}", balance);

            //Donut Chart - Expense By Category
            ViewBag.DonutChartData = transactions.Where(x => x.Category.Type == "Expense")
                .GroupBy(y => y.Category.CategoryId)
                .Select(z => new
                {
                    Amount = z.Sum(y => y.Amount),
                    FormattedAmount = z.Sum(y => y.Amount).ToString("C2"),
                    CategoryTitleWithIcon = z.First().Category.Icon + " " + z.First().Category.Title
                }).OrderByDescending(x => x.Amount)
                .ToList();

            //Spline Chart - Income Vs Expense
            //Income
            List<SplineChartData> incomeSummary = transactions.Where(x => x.Category.Type == "Income")
                .GroupBy(y => y.Date)
                .Select(z => new SplineChartData()
                {
                    Day = z.First().Date.ToString("dd-MMM"),
                    Income = z.Sum(y => y.Amount)
                }).ToList();

            //Expense
            List<SplineChartData> expenseSummary = transactions.Where(x => x.Category.Type == "Expense")
                .GroupBy(y => y.Date)
                .Select(z => new SplineChartData()
                {
                    Day = z.First().Date.ToString("dd-MMM"),
                    Expense = z.Sum(y => y.Amount)
                }).ToList();
            //combine Income & Expense
            string[] last7Days = Enumerable.Range(0, 7)
                .Select(x => startDate.AddDays(x).ToString("dd-MMM")).ToArray();

            ViewBag.SplineChartData = from day in last7Days
                                      join income in incomeSummary on day equals income.Day into dayIncomeJoined
                                      from income in dayIncomeJoined.DefaultIfEmpty()
                                      join expense in expenseSummary on day equals expense.Day into expenseJoined
                                      from expense in expenseJoined.DefaultIfEmpty()
                                      select new
                                      {
                                          Day = day,
                                          Income = income == null ? 0 : income.Income,
                                          Expense = expense == null ? 0 : expense.Expense,
                                      };

            //Recent Transactions
            ViewBag.RecentTransactions = await Context.Transactions
                .Include(x => x.Category)
                .OrderByDescending(y => y.Date)
                .Take(5)
                .ToListAsync();


            return View();
        }
    }

    public class SplineChartData
    {
        public string Day;
        public decimal Income;
        public decimal Expense;
    }
}
