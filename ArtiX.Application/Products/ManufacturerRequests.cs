using System;
using System.ComponentModel.DataAnnotations;

namespace ArtiX.Application.Products;

public class CreateManufacturerRequest
{
    [Required]
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }

    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(64)]
    public string? Code { get; set; }
    public string? ProductNameAtManufacturer { get; set; }
    public string? Address { get; set; }

    [MaxLength(32)]
    public string? Phone { get; set; }

    [MaxLength(256)]
    public string? Website { get; set; }

    [MaxLength(128)]
    public string? ContactPerson { get; set; }
}

public class UpdateManufacturerRequest
{
    public Guid? BranchId { get; set; }

    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(64)]
    public string? Code { get; set; }
    public string? ProductNameAtManufacturer { get; set; }
    public string? Address { get; set; }

    [MaxLength(32)]
    public string? Phone { get; set; }

    [MaxLength(256)]
    public string? Website { get; set; }

    [MaxLength(128)]
    public string? ContactPerson { get; set; }
}
