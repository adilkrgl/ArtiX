using System.Linq;
using ArtiX.Application.Auth;
using ArtiX.Domain.Auth;
using ArtiX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ArtiX.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly ErpDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        ErpDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IOptions<JwtOptions> jwtOptions)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResult> LoginAsync(string userNameOrEmail, string password)
    {
        if (string.IsNullOrWhiteSpace(userNameOrEmail) || string.IsNullOrWhiteSpace(password))
        {
            return InvalidCredentials();
        }

        var normalized = userNameOrEmail.Trim().ToLowerInvariant();

        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u =>
                u.UserName.ToLower() == normalized ||
                (u.Email != null && u.Email.ToLower() == normalized));

        if (user is null || !user.IsActive)
        {
            return InvalidCredentials();
        }

        var passwordValid = _passwordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt);
        if (!passwordValid)
        {
            return InvalidCredentials();
        }

        var roles = user.UserRoles
            .Select(ur => ur.Role.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .ToList();

        var companies = await _context.UserCompanies
            .Where(uc => uc.UserId == user.Id)
            .Select(uc => new AuthCompanyInfo
            {
                Id = uc.CompanyId,
                Name = uc.Company.Name,
                Code = uc.Company.Code
            })
            .ToListAsync();

        var branches = await _context.UserBranches
            .Where(ub => ub.UserId == user.Id)
            .Select(ub => new AuthBranchInfo
            {
                Id = ub.BranchId,
                CompanyId = ub.Branch.CompanyId,
                Name = ub.Branch.Name,
                Code = ub.Branch.Code
            })
            .ToListAsync();

        List<AuthSalesChannelInfo> salesChannels;
        if (companies.Any())
        {
            var companyIds = companies.Select(c => c.Id).ToList();

            salesChannels = await _context.SalesChannels
                .Where(sc => companyIds.Contains(sc.CompanyId))
                .Select(sc => new AuthSalesChannelInfo
                {
                    Id = sc.Id,
                    CompanyId = sc.CompanyId,
                    Name = sc.Name,
                    Code = sc.Code,
                    ChannelType = (int)sc.ChannelType,
                    IsOnline = sc.IsOnline
                })
                .ToListAsync();
        }
        else
        {
            salesChannels = new List<AuthSalesChannelInfo>();
        }

        var token = _jwtTokenService.GenerateToken(user, roles);

        return new AuthResult
        {
            Success = true,
            AccessToken = token,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes),
            UserId = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles,
            Companies = companies,
            Branches = branches,
            SalesChannels = salesChannels
        };
    }

    private static AuthResult InvalidCredentials()
    {
        return new AuthResult
        {
            Success = false,
            Error = "Invalid credentials."
        };
    }
}
