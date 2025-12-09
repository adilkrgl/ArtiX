using ArtiX.WebAdmin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtiX.WebAdmin.Controllers;

[Authorize]
public abstract class ModuleController : Controller
{
    protected IActionResult ModuleView(string title, string? description = null)
    {
        ViewData["Title"] = title;
        var model = new ModulePageViewModel
        {
            Title = title,
            Description = description
        };

        return View("~/Views/Shared/Placeholder.cshtml", model);
    }
}
