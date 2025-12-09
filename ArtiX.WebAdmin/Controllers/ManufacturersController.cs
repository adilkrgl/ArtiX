namespace ArtiX.WebAdmin.Controllers;

public class ManufacturersController : ModuleController
{
    public IActionResult Index()
    {
        return ModuleView("Manufacturers", "Manage manufacturer references.");
    }
}
