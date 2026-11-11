using SchoolAdmission.Api.Contracts.Responses;

namespace SchoolAdmission.Api.Services;

public interface IHoldService
{
    Task<HoldResponse> CreateHoldAsync(string classId, string studentId, CancellationToken ct = default);
    Task<HoldDetailResponse> GetHoldByIdAsync(string holdId, CancellationToken ct = default);
    Task<ConfirmHoldResponse> ConfirmHoldAsync(string holdId, string studentId, CancellationToken ct = default);
    Task CancelHoldAsync(string holdId, string studentId, CancellationToken ct = default);
    Task<IReadOnlyList<StudentHoldResponse>> GetHoldsForStudentAsync(string studentId, CancellationToken ct = default);
}

