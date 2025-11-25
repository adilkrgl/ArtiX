using System;
using System.Collections.Generic;

namespace ArtiX.Domain.Auth;

public class AuthResult
{
    public bool Success { get; set; }

    public string? Error { get; set; }

    public string? AccessToken { get; set; }

    public DateTime? ExpiresAtUtc { get; set; }

    public Guid? UserId { get; set; }

    public string? UserName { get; set; }

    public string? Email { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public List<string> Roles { get; set; } = new();

    public List<AuthCompanyInfo> Companies { get; set; } = new();

    public List<AuthBranchInfo> Branches { get; set; } = new();

    public List<AuthSalesChannelInfo> SalesChannels { get; set; } = new();
}

public class AuthCompanyInfo
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;
}

public class AuthBranchInfo
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;
}

public class AuthSalesChannelInfo
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public int ChannelType { get; set; }

    public bool IsOnline { get; set; }
}
