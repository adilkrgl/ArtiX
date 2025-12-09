namespace ArtiX.WebAdmin.Controllers;

public class CustomersController : ModuleController
{
    public IActionResult Index()
    {
        return ModuleView("Customers", "Customer records from ArtiX.Api will be listed here.");
    }
}
