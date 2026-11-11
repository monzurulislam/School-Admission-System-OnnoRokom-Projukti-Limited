using SchoolAdmission.Api.Contracts.Requests;
using SchoolAdmission.Api.Contracts.Responses;

namespace SchoolAdmission.Api.Services;

public interface IClassService
{
    Task<ClassResponse> CreateClassAsync(CreateClassRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ClassDetailResponse>> GetAllClassesAsync(CancellationToken ct = default);
    Task<ClassDetailResponse> GetClassByIdAsync(string classId, CancellationToken ct = default);
}

