using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models;

public record Entity
{
    [Key]
    public required string Id { get; set; }

    public required int TenantId { get; set; }

    public required IDictionary<string, object> Properties { get; set; }
}