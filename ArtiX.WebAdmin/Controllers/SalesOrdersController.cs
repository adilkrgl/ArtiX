namespace ArtiX.WebAdmin.Controllers;

public class SalesOrdersController : ModuleController
{
    public IActionResult Index()
    {
        return ModuleView("Sales Orders", "Sales order entry and tracking from ArtiX.Api.");
    }
}
