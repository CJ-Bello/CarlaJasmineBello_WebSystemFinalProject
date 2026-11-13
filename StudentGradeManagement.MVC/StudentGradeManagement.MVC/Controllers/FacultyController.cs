using Microsoft.AspNetCore.Mvc;

namespace StudentGradeManagement.MVC.Controllers
{
    public class FacultyController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Faculty")
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Token = HttpContext.Session.GetString("Token");
            ViewBag.FullName = HttpContext.Session.GetString("FullName");

            return View();
        }
    }
}
