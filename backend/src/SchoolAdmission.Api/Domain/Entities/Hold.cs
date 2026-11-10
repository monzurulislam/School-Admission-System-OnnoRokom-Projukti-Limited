using SchoolAdmission.Api.Domain.Enums;

namespace SchoolAdmission.Api.Domain.Entities;

public class Hold
{
    public string Id { get; set; } = $"h_{Guid.NewGuid():N}";
    public string ClassId { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string Status { get; set; } = HoldStatus.Held;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }

    public Class Class { get; set; } = null!;

    public bool IsActive(DateTime nowUtc) =>
        Status == HoldStatus.Held && ExpiresAt > nowUtc;
}
