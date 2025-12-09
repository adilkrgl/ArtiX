namespace ArtiX.WebAdmin.Controllers;

public class SalesRepresentativesController : ModuleController
{
    public IActionResult Index()
    {
        return ModuleView("Sales Representatives", "Oversee sales representative assignments and coverage.");
    }
}
