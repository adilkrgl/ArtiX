namespace ArtiX.WebAdmin.Controllers;

public class SettingsController : ModuleController
{
    public IActionResult Index()
    {
        return ModuleView("Settings", "Administration settings for the ArtiX experience.");
    }
}
