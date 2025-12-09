using System;
namespace ArtiX.Api.Dtos.Products;

public class ManufacturerDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? ProductNameAtManufacturer { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? ContactPerson { get; set; }
}
