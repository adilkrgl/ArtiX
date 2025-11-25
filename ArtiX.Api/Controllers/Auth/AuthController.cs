using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Linq;
using ArtiX.Api.Dtos.Auth;
using ArtiX.Application.Auth;
using ArtiX.Domain.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtiX.Api.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _authService.LoginAsync(request.UserNameOrEmail, request.Password);

        if (!result.Success)
        {
            return Unauthorized(new LoginResponseDto
            {
                Success = false,
                Error = result.Error ?? "Invalid credentials."
            });
        }

        var response = new LoginResponseDto
        {
            Success = true,
            AccessToken = result.AccessToken,
            ExpiresAtUtc = result.ExpiresAtUtc,
            User = result.UserId.HasValue
                ? new AuthUserDto
                {
                    Id = result.UserId.Value,
                    UserName = result.UserName ?? string.Empty,
                    Email = result.Email,
                    FirstName = result.FirstName,
                    LastName = result.LastName,
                    Roles = result.Roles
                }
                : null,
            Companies = result.Companies.Select(c => new AuthCompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code
            }).ToList(),
            Branches = result.Branches.Select(b => new AuthBranchDto
            {
                Id = b.Id,
                CompanyId = b.CompanyId,
                Name = b.Name,
                Code = b.Code
            }).ToList(),
            SalesChannels = result.SalesChannels.Select(s => new AuthSalesChannelDto
            {
                Id = s.Id,
                CompanyId = s.CompanyId,
                Name = s.Name,
                Code = s.Code,
                ChannelType = s.ChannelType,
                IsOnline = s.IsOnline
            }).ToList()
        };

        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<AuthUserDto> Me()
    {
        var principal = HttpContext.User;

        var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst(JwtRegisteredClaimNames.Sub);
        _ = Guid.TryParse(idClaim?.Value, out var userId);

        var userName = principal.Identity?.Name
                        ?? principal.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value
                        ?? string.Empty;

        var email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        var firstName = principal.FindFirst(ClaimTypes.GivenName)?.Value;
        var lastName = principal.FindFirst(ClaimTypes.Surname)?.Value;

        var roles = principal.FindAll(ClaimTypes.Role)
            .Select(r => r.Value)
            .ToList();

        var dto = new AuthUserDto
        {
            Id = userId,
            UserName = userName,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Roles = roles
        };

        return Ok(dto);
    }
}
