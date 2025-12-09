namespace ArtiX.WebAdmin.Controllers;

public class ContactsController : ModuleController
{
    public IActionResult Index()
    {
        return ModuleView("Contacts", "Maintain contact people and communication preferences.");
    }
}
