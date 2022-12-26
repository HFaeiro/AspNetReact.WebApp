using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace ASP.Back.Controllers
{
    public class FallbackController : Controller
    {
        public FallbackController()
        {

        }

        public IActionResult Index()
        {
            return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(),
                "wwwroot", "index.html"),
                MediaTypeNames.Text.Html);
        }

    }
}
