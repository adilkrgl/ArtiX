namespace ArtiX.WebAdmin.Controllers;

public class StockMovementsController : ModuleController
{
    public IActionResult Index()
    {
        return ModuleView("Stock Movements", "Track stock movement history once API hooks are in place.");
    }
}
