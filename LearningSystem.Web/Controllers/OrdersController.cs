namespace LearningSystem.Web.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    public class OrdersController : Controller
    {
        public IActionResult Index() => this.View();

        public async Task<IActionResult> Details(int id)
            => this.View();
    }
}