using Microsoft.AspNetCore.Mvc;

namespace StudentGradeManagement.MVC.Controllers
{
    public class StudentController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Student")
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Token = HttpContext.Session.GetString("Token");
            ViewBag.FullName = HttpContext.Session.GetString("FullName");
            ViewBag.Email = HttpContext.Session.GetString("Email");

            return View();
        }
    }
}
