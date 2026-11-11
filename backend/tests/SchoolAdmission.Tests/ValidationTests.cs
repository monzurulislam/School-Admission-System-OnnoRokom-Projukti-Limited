using FluentAssertions;
using SchoolAdmission.Api.Common;
using SchoolAdmission.Api.Domain.Exceptions;

namespace SchoolAdmission.Tests;

public class ValidationTests
{
    [Theory]
    [InlineData("12345678901234567")]
    [InlineData("20001234567890123")]
    [InlineData("  12345678901234567  ")]
    public void StudentValidator_Should_Pass_For_Valid_17_Digits(string input)
    {
        var result = StudentValidator.ValidateAndNormalize(input);
        result.Should().Be(input.Trim());
        result.Length.Should().Be(17);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12345")]
    [InlineData("1234567890123456")] // 16 digits
    [InlineData("123456789012345678")] // 18 digits
    [InlineData("1234567890123456a")] // contains letter
    [InlineData("12345678-01234567")] // contains dash
    public void StudentValidator_Should_Throw_BadRequestException_For_Invalid_Input(string? input)
    {
        var act = () => StudentValidator.ValidateAndNormalize(input);
        act.Should().Throw<BadRequestException>();
    }
}

