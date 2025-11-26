using ArtiX.Domain.Auth;
using ArtiX.Domain.Entities.Core;
using ArtiX.Domain.Entities.Identity;
using ArtiX.Domain.Enums;
using ArtiX.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtiX.Api.Controllers.Dev;

[ApiController]
[Route("api/dev/[controller]")]
public class DevSeedController : ControllerBase
{
    private readonly ErpDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public DevSeedController(ErpDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("seed-admin")]
    [AllowAnonymous]
    public async Task<IActionResult> SeedAdmin()
    {
        if (await _context.Users.AnyAsync())
        {
            return BadRequest(new { message = "Users already exist." });
        }

        var now = DateTime.UtcNow;

        var company = new Company
        {
            Id = Guid.NewGuid(),
            Name = "Demo Company",
            Code = "DEMO",
            CreatedAt = now
        };

        var branch = new Branch
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            Company = company,
            Name = "Main Branch",
            Code = "MAIN",
            BranchType = BranchType.Store,
            CreatedAt = now
        };

        var salesChannel = new SalesChannel
        {
            Id = Guid.NewGuid(),
            CompanyId = company.Id,
            Company = company,
            Name = "Demo Web",
            Code = "WEB",
            ChannelType = SalesChannelType.Website,
            IsOnline = true,
            IsActive = true,
            CreatedAt = now
        };

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Admin",
            Description = "System administrator",
            CreatedAt = now
        };

        _passwordHasher.CreatePasswordHash("Admin123!", out var passwordHash, out var passwordSalt);

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "admin",
            Email = "admin@demo.com",
            FirstName = "System",
            LastName = "Admin",
            IsActive = true,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            CompanyId = company.Id,
            BranchId = branch.Id,
            CreatedAt = now
        };

        var userRole = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            RoleId = role.Id,
            Role = role,
            CreatedAt = now
        };

        var userCompany = new UserCompany
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            CompanyId = company.Id,
            Company = company,
            CreatedAt = now
        };

        var userBranch = new UserBranch
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            BranchId = branch.Id,
            Branch = branch,
            CreatedAt = now
        };

        _context.Companies.Add(company);
        _context.Branches.Add(branch);
        _context.SalesChannels.Add(salesChannel);
        _context.Roles.Add(role);
        _context.Users.Add(user);
        _context.UserRoles.Add(userRole);
        _context.UserCompanies.Add(userCompany);
        _context.UserBranches.Add(userBranch);

        await _context.SaveChangesAsync();

        return Ok(new { message = "Admin user created.", userName = "admin", password = "Admin123!" });
    }
}
