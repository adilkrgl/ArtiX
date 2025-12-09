namespace ArtiX.WebAdmin.Controllers;

public class ProductsController : ModuleController
{
    public IActionResult Index()
    {
        return ModuleView("Products", "Catalogue of products synchronised with ArtiX.Api.");
    }
}
