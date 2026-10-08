using System.Diagnostics;
using FSPBook.Pages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FSPBook.Tests.Pages;

public class ErrorModelTests
{
    private static ErrorModel CreateModel(string traceIdentifier) => new(NullLogger<ErrorModel>.Instance)
    {
        PageContext = new PageContext { HttpContext = new DefaultHttpContext { TraceIdentifier = traceIdentifier } }
    };

    [Fact]
    public void OnGet_Uses_TraceIdentifier_When_No_Current_Activity()
    {
        Activity.Current = null;
        var model = CreateModel("trace-123");

        model.OnGet();

        Assert.Equal("trace-123", model.RequestId);
        Assert.True(model.ShowRequestId);
    }

    [Fact]
    public void OnGet_Prefers_Current_Activity_Id()
    {
        using var activity = new Activity("test").Start();
        var model = CreateModel("trace-123");

        model.OnGet();

        Assert.Equal(activity.Id, model.RequestId);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("abc", true)]
    public void ShowRequestId_Reflects_RequestId(string? requestId, bool expected)
    {
        var model = new ErrorModel(NullLogger<ErrorModel>.Instance) { RequestId = requestId! };

        Assert.Equal(expected, model.ShowRequestId);
    }
}
