namespace ArtiX.WebAdmin.Controllers;

public class AttributesController : ModuleController
{
    public IActionResult Index()
    {
        return ModuleView("Attributes", "Define product attributes and options.");
    }
}
