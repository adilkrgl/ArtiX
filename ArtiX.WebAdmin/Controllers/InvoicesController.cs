namespace ArtiX.WebAdmin.Controllers;

public class InvoicesController : ModuleController
{
    public IActionResult Index()
    {
        return ModuleView("Invoices", "Invoice management aligned with ArtiX.Api.");
    }
}
