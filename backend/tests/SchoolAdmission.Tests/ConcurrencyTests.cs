using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SchoolAdmission.Api.Contracts.Requests;
using SchoolAdmission.Api.Contracts.Responses;

namespace SchoolAdmission.Tests;

public class ConcurrencyTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ConcurrencyTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HighConcurrency_50_Students_Compete_For_5_Seats_Should_Have_Zero_Overselling()
    {
        var client = _factory.CreateClient();

        // 1. Create a class with only 5 seats
        const int totalSeats = 5;
        const int concurrentApplicants = 50;

        var classRes = await client.PostAsJsonAsync("/classes", new CreateClassRequest
        {
            Name = "Strict Capacity Class 101",
            TotalSeats = totalSeats
        });
        classRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdClass = await classRes.Content.ReadFromJsonAsync<ClassResponse>();
        createdClass.Should().NotBeNull();
        var classId = createdClass!.Id;

        // 2. Prepare 50 unique 17-digit student IDs
        var studentIds = Enumerable.Range(1, concurrentApplicants)
            .Select(i => $"9999000011112{i:D4}")
            .ToList();

        var successResults = new ConcurrentBag<HttpResponseMessage>();
        var conflictResults = new ConcurrentBag<HttpResponseMessage>();
        var otherResults = new ConcurrentBag<HttpResponseMessage>();

        // 3. Fire all 50 requests concurrently
        var tasks = studentIds.Select(async studentId =>
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, $"/classes/{classId}/holds");
            req.Headers.Add("X-Student-Id", studentId);

            var res = await client.SendAsync(req);

            if (res.StatusCode == HttpStatusCode.Created)
            {
                successResults.Add(res);
            }
            else if (res.StatusCode == HttpStatusCode.Conflict)
            {
                conflictResults.Add(res);
            }
            else
            {
                otherResults.Add(res);
            }
        });

        await Task.WhenAll(tasks);

        // 4. Assert strict invariants
        otherResults.Should().BeEmpty("all requests must be either 201 Created or 409 Conflict");
        successResults.Count.Should().Be(totalSeats, $"exactly {totalSeats} seats must be held");
        conflictResults.Count.Should().Be(concurrentApplicants - totalSeats, $"remaining {concurrentApplicants - totalSeats} must be rejected with 409 Conflict");

        // 5. Verify database class state
        var classDetailRes = await client.GetAsync($"/classes/{classId}");
        classDetailRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await classDetailRes.Content.ReadFromJsonAsync<ClassDetailResponse>();

        detail.Should().NotBeNull();
        detail!.TotalSeats.Should().Be(totalSeats);
        detail.HeldSeats.Should().Be(totalSeats);
        detail.ConfirmedSeats.Should().Be(0);
        detail.AvailableSeats.Should().Be(0);
    }
}

