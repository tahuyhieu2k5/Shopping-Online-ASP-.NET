using Microsoft.AspNetCore.Mvc;
using Shopping_Tutorial.Repository;

namespace Shopping_Tutorial.Controllers
{
    public class RealtimeController : Controller
    {

        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
