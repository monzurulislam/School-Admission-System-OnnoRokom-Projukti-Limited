using System.Text.Json.Serialization;

namespace SchoolAdmission.Api.Contracts.Responses;

public class ClassResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("totalSeats")]
    public int TotalSeats { get; set; }
}

public class ClassDetailResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("totalSeats")]
    public int TotalSeats { get; set; }

    [JsonPropertyName("heldSeats")]
    public int HeldSeats { get; set; }

    [JsonPropertyName("confirmedSeats")]
    public int ConfirmedSeats { get; set; }

    [JsonPropertyName("availableSeats")]
    public int AvailableSeats { get; set; }
}

public class HoldResponse
{
    [JsonPropertyName("holdId")]
    public string HoldId { get; set; } = string.Empty;

    [JsonPropertyName("classId")]
    public string ClassId { get; set; } = string.Empty;

    [JsonPropertyName("studentId")]
    public string StudentId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("expiresAt")]
    public string ExpiresAt { get; set; } = string.Empty;
}

public class HoldDetailResponse
{
    [JsonPropertyName("holdId")]
    public string HoldId { get; set; } = string.Empty;

    [JsonPropertyName("classId")]
    public string ClassId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("expiresAt")]
    public string ExpiresAt { get; set; } = string.Empty;
}

public class ConfirmHoldResponse
{
    [JsonPropertyName("holdId")]
    public string HoldId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public class StudentHoldResponse
{
    [JsonPropertyName("holdId")]
    public string HoldId { get; set; } = string.Empty;

    [JsonPropertyName("classId")]
    public string ClassId { get; set; } = string.Empty;

    [JsonPropertyName("className")]
    public string ClassName { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("expiresAt")]
    public string ExpiresAt { get; set; } = string.Empty;
}

