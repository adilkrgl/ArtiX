namespace ArtiX.WebAdmin.Controllers;

public class DashboardController : ModuleController
{
    public IActionResult Index()
    {
        return ModuleView("Dashboard", "Overview of key ArtiX metrics will appear here.");
    }
}
