using System;
using System.Collections.Generic;

namespace ArtiX.Api.Dtos.Auth;

public class LoginResponseDto
{
    public bool Success { get; set; }

    public string? Error { get; set; }

    public string? AccessToken { get; set; }

    public DateTime? ExpiresAtUtc { get; set; }

    public AuthUserDto? User { get; set; }

    public List<AuthCompanyDto> Companies { get; set; } = new();

    public List<AuthBranchDto> Branches { get; set; } = new();

    public List<AuthSalesChannelDto> SalesChannels { get; set; } = new();
}
