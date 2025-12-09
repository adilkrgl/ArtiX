namespace ArtiX.WebAdmin.Controllers;

public class WarehousesController : ModuleController
{
    public IActionResult Index()
    {
        return ModuleView("Warehouses", "Warehouse definitions provided by ArtiX.Api.");
    }
}
