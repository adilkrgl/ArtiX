using System;
using System.Collections.Generic;

namespace ArtiX.Api.Dtos.Auth;

public class AuthUserDto
{
    public Guid Id { get; set; }

    public string UserName { get; set; } = null!;

    public string? Email { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public List<string> Roles { get; set; } = new();
}

public class AuthCompanyDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;
}

public class AuthBranchDto
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;
}

public class AuthSalesChannelDto
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public int ChannelType { get; set; }

    public bool IsOnline { get; set; }
}
