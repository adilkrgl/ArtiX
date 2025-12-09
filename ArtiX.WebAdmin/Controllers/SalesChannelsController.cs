namespace ArtiX.WebAdmin.Controllers;

public class SalesChannelsController : ModuleController
{
    public IActionResult Index()
    {
        return ModuleView("Sales Channels", "Configure sales channels and routes to market.");
    }
}
