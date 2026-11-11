using SchoolAdmission.Api.Common;
using SchoolAdmission.Api.Contracts.Requests;
using SchoolAdmission.Api.Services;

namespace SchoolAdmission.Api.Endpoints;

public static class ClassEndpoints
{
    public static void MapClassEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/classes")
            .WithTags("Classes");

        group.MapPost("", async (CreateClassRequest request, IClassService classService, CancellationToken ct) =>
        {
            var created = await classService.CreateClassAsync(request, ct);
            return ApiResults.Created($"/classes/{created.Id}", created);
        })
        .WithName("CreateClass")
        .WithSummary("Create a new admission class")
        .Produces(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("", async (IClassService classService, CancellationToken ct) =>
        {
            var classes = await classService.GetAllClassesAsync(ct);
            return ApiResults.Ok(classes);
        })
        .WithName("GetAllClasses")
        .WithSummary("List all classes with seat availability")
        .Produces(StatusCodes.Status200OK);

        group.MapGet("/{classId}", async (string classId, IClassService classService, CancellationToken ct) =>
        {
            var classDetail = await classService.GetClassByIdAsync(classId, ct);
            return ApiResults.Ok(classDetail);
        })
        .WithName("GetClassById")
        .WithSummary("Get class details and seat availability by ID")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
