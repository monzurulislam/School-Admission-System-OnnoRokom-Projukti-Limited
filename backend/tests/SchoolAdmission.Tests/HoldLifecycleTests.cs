using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SchoolAdmission.Api.Contracts.Requests;
using SchoolAdmission.Api.Contracts.Responses;
using SchoolAdmission.Api.Domain.Enums;

namespace SchoolAdmission.Tests;

public class HoldLifecycleTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HoldLifecycleTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Complete_Hold_Lifecycle_Should_Work_Correctly()
    {
        // 1. Create a Class
        var classRequest = new CreateClassRequest
        {
            Name = "Grade 6 - Morning",
            TotalSeats = 10
        };

        var createClassRes = await _client.PostAsJsonAsync("/classes", classRequest);
        createClassRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdClass = await createClassRes.Content.ReadFromJsonAsync<ClassResponse>();
        createdClass.Should().NotBeNull();
        createdClass!.Id.Should().NotBeNullOrEmpty();
        createdClass.TotalSeats.Should().Be(10);

        // 2. Query Class details
        var getClassRes = await _client.GetAsync($"/classes/{createdClass.Id}");
        getClassRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var classDetail = await getClassRes.Content.ReadFromJsonAsync<ClassDetailResponse>();
        classDetail.Should().NotBeNull();
        classDetail!.HeldSeats.Should().Be(0);
        classDetail.ConfirmedSeats.Should().Be(0);
        classDetail.AvailableSeats.Should().Be(10);

        // 3. Create Hold for a student
        var studentId = "20081234567890123";
        using var holdReq = new HttpRequestMessage(HttpMethod.Post, $"/classes/{createdClass.Id}/holds");
        holdReq.Headers.Add("X-Student-Id", studentId);

        var holdRes = await _client.SendAsync(holdReq);
        holdRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var hold = await holdRes.Content.ReadFromJsonAsync<HoldResponse>();
        hold.Should().NotBeNull();
        hold!.HoldId.Should().NotBeNullOrEmpty();
        hold.ClassId.Should().Be(createdClass.Id);
        hold.StudentId.Should().Be(studentId);
        hold.Status.Should().Be(HoldStatus.Held);

        // Verify seat availability updated
        var afterHoldClassRes = await _client.GetAsync($"/classes/{createdClass.Id}");
        var afterHoldClass = await afterHoldClassRes.Content.ReadFromJsonAsync<ClassDetailResponse>();
        afterHoldClass!.HeldSeats.Should().Be(1);
        afterHoldClass.AvailableSeats.Should().Be(9);

        // 4. Duplicate Hold by same student should be rejected with 409 Conflict
        using var dupHoldReq = new HttpRequestMessage(HttpMethod.Post, $"/classes/{createdClass.Id}/holds");
        dupHoldReq.Headers.Add("X-Student-Id", studentId);
        var dupHoldRes = await _client.SendAsync(dupHoldReq);
        dupHoldRes.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // 5. Confirm Hold
        using var confirmReq = new HttpRequestMessage(HttpMethod.Post, $"/holds/{hold.HoldId}/confirm");
        confirmReq.Headers.Add("X-Student-Id", studentId);
        var confirmRes = await _client.SendAsync(confirmReq);
        confirmRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var confirmData = await confirmRes.Content.ReadFromJsonAsync<ConfirmHoldResponse>();
        confirmData!.Status.Should().Be(HoldStatus.Confirmed);

        // Idempotent Confirm
        using var repeatConfirmReq = new HttpRequestMessage(HttpMethod.Post, $"/holds/{hold.HoldId}/confirm");
        repeatConfirmReq.Headers.Add("X-Student-Id", studentId);
        var repeatConfirmRes = await _client.SendAsync(repeatConfirmReq);
        repeatConfirmRes.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify seat availability now reflects confirmed
        var afterConfirmClassRes = await _client.GetAsync($"/classes/{createdClass.Id}");
        var afterConfirmClass = await afterConfirmClassRes.Content.ReadFromJsonAsync<ClassDetailResponse>();
        afterConfirmClass!.HeldSeats.Should().Be(0);
        afterConfirmClass.ConfirmedSeats.Should().Be(1);
        afterConfirmClass.AvailableSeats.Should().Be(9);

        // 6. Check /me/holds
        using var myHoldsReq = new HttpRequestMessage(HttpMethod.Get, "/me/holds");
        myHoldsReq.Headers.Add("X-Student-Id", studentId);
        var myHoldsRes = await _client.SendAsync(myHoldsReq);
        myHoldsRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var myHolds = await myHoldsRes.Content.ReadFromJsonAsync<List<StudentHoldResponse>>();
        myHolds.Should().NotBeNull();
        myHolds!.Should().ContainSingle(h => h.HoldId == hold.HoldId && h.Status == HoldStatus.Confirmed);
    }
}
