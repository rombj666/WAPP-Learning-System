using ILOWLearningSystem.Web.Data;
using ILOWLearningSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ILOWLearningSystem.Web.Controllers
{
    public class StressTrackerController : Controller
    {
        private readonly AppDbContext _db;

        public StressTrackerController(AppDbContext db)
        {
            _db = db;
        }

        // =========================================
        // DASHBOARD / HISTORY
        // =========================================
        public async Task<IActionResult> Index()
        {
            var userIdString = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = Convert.ToInt32(userIdString);

            var records = await _db.StressRecords
                .Where(s => s.UserId == userId)
                .OrderBy(s => s.RecordedAt)
                .ToListAsync();

            double averageStress = 0;

            if (records.Any())
            {
                averageStress = records.Average(r => r.StressLevel);
            }

            string recommendation = "";
            string status = "";

            if (averageStress <= 3)
            {
                status = "Low Stress";
                recommendation = "You are doing well. Keep maintaining a healthy study routine.";
            }
            else if (averageStress <= 6)
            {
                status = "Moderate Stress";
                recommendation = "Consider taking short breaks and balancing your study schedule.";
            }
            else if (averageStress <= 8)
            {
                status = "High Stress";
                recommendation = "Your stress level is increasing. Try reducing workload and resting more.";
            }
            else
            {
                status = "Critical Stress";
                recommendation = "Your stress level is very high. Consider seeking support and prioritizing rest.";
            }

            var viewModel = new StressTrackerViewModel
            {
                Records = records,
                AverageStress = averageStress,
                Recommendation = recommendation,
                StressStatus = status
            };

            return View(viewModel);
        }

        // =========================================
        // CREATE PAGE
        // =========================================
        public IActionResult Create()
        {
            var userIdString = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // =========================================
        // SAVE RECORD
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StressRecord model)
        {
            var userIdString = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int userId = Convert.ToInt32(userIdString);

            model.UserId = userId;
            model.RecordedAt = DateTime.Now;

            _db.StressRecords.Add(model);

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Stress record added successfully.";

            return RedirectToAction("Index");
        }
    }
}