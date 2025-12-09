namespace ArtiX.WebAdmin.Controllers;

public class BranchesController : ModuleController
{
    public IActionResult Index()
    {
        return ModuleView("Branches", "Maintain branch details and locations.");
    }
}
