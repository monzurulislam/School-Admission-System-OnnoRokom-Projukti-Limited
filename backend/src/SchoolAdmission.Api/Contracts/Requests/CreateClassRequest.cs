using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SchoolAdmission.Api.Contracts.Requests;

public class CreateClassRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "totalSeats must be at least 1")]
    [JsonPropertyName("totalSeats")]
    public int TotalSeats { get; set; }
}

