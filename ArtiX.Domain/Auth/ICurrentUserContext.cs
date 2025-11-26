using System;
using System.Collections.Generic;

namespace ArtiX.Domain.Auth;

public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    string? FirstName { get; }
    string? LastName { get; }
    IReadOnlyList<string> Roles { get; }

    bool IsInRole(string role);
}
