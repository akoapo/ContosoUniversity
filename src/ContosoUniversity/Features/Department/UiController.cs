namespace ContosoUniversity.Features.Department
{
    using System.Threading.Tasks;
    using Infrastructure;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    public class UiController : Controller
    {
        private readonly IMediator _mediator;

        public UiController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _mediator.Send(new Index.Query());

            return View(model);
        }

        public async Task<IActionResult> Details(Details.Query query)
        {
            var department = await _mediator.Send(query);

            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Create.Command());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Create.Command model)
        {
            await _mediator.Send(model);

            return this.RedirectToActionJson(c => c.Index());
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Edit.Query query)
        {
            var department = await _mediator.Send(query);

            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Edit.Command model)
        {
            await _mediator.Send(model);

            return this.RedirectToActionJson(c => c.Index());
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Delete.Query query)
        {
            var model = await _mediator.Send(query);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Delete.Command command)
        {
            await _mediator.Send(command);

            return this.RedirectToActionJson(c => c.Index());
        }
    }
}
