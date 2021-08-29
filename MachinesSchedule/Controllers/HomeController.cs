using MachinesSchedule.Models.DataAccessLayer;
using Microsoft.AspNetCore.Mvc;


namespace MachinesSchedule.Controllers
{
    public class HomeController : Controller
    {
        ApplicationContext _context;

        public HomeController(ApplicationContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
