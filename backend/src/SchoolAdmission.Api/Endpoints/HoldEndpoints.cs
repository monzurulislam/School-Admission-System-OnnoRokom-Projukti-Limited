using Microsoft.AspNetCore.Mvc;
using SchoolAdmission.Api.Common;
using SchoolAdmission.Api.Services;

namespace SchoolAdmission.Api.Endpoints;

public static class HoldEndpoints
{
    public static void MapHoldEndpoints(this IEndpointRouteBuilder app)
    {
        // 1. Hold a seat: POST /classes/{classId}/holds
        app.MapPost("/classes/{classId}/holds", async (
            string classId,
            [FromHeader(Name = "X-Student-Id")] string? studentIdHeader,
            IHoldService holdService,
            CancellationToken ct) =>
        {
            var studentId = StudentValidator.ValidateAndNormalize(studentIdHeader);
            var result = await holdService.CreateHoldAsync(classId, studentId, ct);
            return ApiResults.Created($"/holds/{result.HoldId}", result);
        })
        .WithTags("Holds")
        .WithName("CreateHold")
        .WithSummary("Reserve a temporary seat hold for a student")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        // 2. Get hold status: GET /holds/{holdId}
        app.MapGet("/holds/{holdId}", async (
            string holdId,
            IHoldService holdService,
            CancellationToken ct) =>
        {
            var result = await holdService.GetHoldByIdAsync(holdId, ct);
            return ApiResults.Ok(result);
        })
        .WithTags("Holds")
        .WithName("GetHoldById")
        .WithSummary("Get current status and expiration for a hold")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        // 3. Confirm hold: POST /holds/{holdId}/confirm
        app.MapPost("/holds/{holdId}/confirm", async (
            string holdId,
            [FromHeader(Name = "X-Student-Id")] string? studentIdHeader,
            IHoldService holdService,
            CancellationToken ct) =>
        {
            var studentId = StudentValidator.ValidateAndNormalize(studentIdHeader);
            var result = await holdService.ConfirmHoldAsync(holdId, studentId, ct);
            return ApiResults.Ok(result);
        })
        .WithTags("Holds")
        .WithName("ConfirmHold")
        .WithSummary("Confirm admission and payment for a held seat")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);

        // 4. Cancel hold: DELETE /holds/{holdId}
        app.MapDelete("/holds/{holdId}", async (
            string holdId,
            [FromHeader(Name = "X-Student-Id")] string? studentIdHeader,
            IHoldService holdService,
            CancellationToken ct) =>
        {
            var studentId = StudentValidator.ValidateAndNormalize(studentIdHeader);
            await holdService.CancelHoldAsync(holdId, studentId, ct);
            return ApiResults.NoContent();
        })
        .WithTags("Holds")
        .WithName("CancelHold")
        .WithSummary("Cancel an active hold to release the seat immediately")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);

        // 5. My applications: GET /me/holds
        app.MapGet("/me/holds", async (
            [FromHeader(Name = "X-Student-Id")] string? studentIdHeader,
            IHoldService holdService,
            CancellationToken ct) =>
        {
            var studentId = StudentValidator.ValidateAndNormalize(studentIdHeader);
            var results = await holdService.GetHoldsForStudentAsync(studentId, ct);
            return ApiResults.Ok(results);
        })
        .WithTags("Holds")
        .WithName("GetMyHolds")
        .WithSummary("Get all admission holds for the authenticated student")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
