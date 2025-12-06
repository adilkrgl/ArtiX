using System;

namespace ArtiX.Application;

public class ManufacturerDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ProductNameAtManufacturer { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? ContactPerson { get; set; }
}

public class CreateManufacturerRequest
{
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ProductNameAtManufacturer { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? ContactPerson { get; set; }
}

public class UpdateManufacturerRequest
{
    public Guid? BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ProductNameAtManufacturer { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? ContactPerson { get; set; }
}
