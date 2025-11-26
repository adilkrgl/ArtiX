using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using ArtiX.Domain.Auth;
using Microsoft.AspNetCore.Http;

namespace ArtiX.Api.Auth;

public class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public Guid? UserId
    {
        get
        {
            var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier) ?? User?.FindFirst("sub");
            if (idClaim == null)
            {
                return null;
            }

            return Guid.TryParse(idClaim.Value, out var id) ? id : null;
        }
    }

    public string? UserName => User?.Identity?.Name ?? User?.FindFirst("unique_name")?.Value;

    public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value ?? User?.FindFirst("email")?.Value;

    public string? FirstName => User?.FindFirst(ClaimTypes.GivenName)?.Value;

    public string? LastName => User?.FindFirst(ClaimTypes.Surname)?.Value;

    public IReadOnlyList<string> Roles => User?.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList() ?? new List<string>();

    public bool IsInRole(string role) => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
}
