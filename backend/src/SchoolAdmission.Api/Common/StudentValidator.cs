using SchoolAdmission.Api.Domain.Exceptions;

namespace SchoolAdmission.Api.Common;

public static class StudentValidator
{
    public static string ValidateAndNormalize(string? studentId)
    {
        if (string.IsNullOrWhiteSpace(studentId))
        {
            throw new BadRequestException("Header 'X-Student-Id' is required.");
        }

        var trimmed = studentId.Trim();
        if (trimmed.Length != 17 || !trimmed.All(char.IsDigit))
        {
            throw new BadRequestException("Header 'X-Student-Id' must be a valid 17-digit numeric birth registration number.");
        }

        return trimmed;
    }
}

