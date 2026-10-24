namespace ContosoUniversity.Features.Home
{
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    public class UiController : Controller
    {
        private readonly IMediator _mediator;

        public UiController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> About()
        {
            var data = await _mediator.Send(new About.Query());

            return View(data);
        }

        public IActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        /// <summary>
        /// Target of app.UseExceptionHandler("/Home/Error"); renders Features/Shared/Error.cshtml.
        /// Replaces the MVC5 global HandleErrorAttribute registered in FilterConfig.
        /// </summary>
        public IActionResult Error()
        {
            return View("Error");
        }
    }
}
