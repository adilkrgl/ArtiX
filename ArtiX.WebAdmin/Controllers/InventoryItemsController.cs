namespace ArtiX.WebAdmin.Controllers;

public class InventoryItemsController : ModuleController
{
    public IActionResult Index()
    {
        return ModuleView("Inventory Items", "Inventory items and stock-keeping details.");
    }
}
