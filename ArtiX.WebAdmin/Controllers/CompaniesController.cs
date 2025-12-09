namespace ArtiX.WebAdmin.Controllers;

public class CompaniesController : ModuleController
{
    public IActionResult Index()
    {
        return ModuleView("Companies", "Manage company profiles sourced from ArtiX.Api.");
    }
}
