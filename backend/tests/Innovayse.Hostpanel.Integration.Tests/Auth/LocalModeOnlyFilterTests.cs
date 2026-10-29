using System.Net.Mime;
using Innovayse.Hostpanel.API.Auth;
using Innovayse.Hostpanel.Infrastructure.Auth;
using Innovayse.Hostpanel.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>
/// <see cref="LocalModeOnlyFilter"/>: in a deployment whose people belong to an external sign-on
/// provider the local sign-in routes answer the standard 404 before anything behind them runs, and in
/// local mode the request passes through untouched.
/// </summary>
/// <param name="factory">The shared API + PostgreSQL host, which supplies the real message resolver.</param>
public sealed class LocalModeOnlyFilterTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    /// <summary>Outside local mode the filter answers the standard 404 body and never runs the rest of the pipeline.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task OutsideLocalMode_AnswersTheStandardNotFoundAsync()
    {
        var (context, next) = ContextFor();

        await new LocalModeOnlyFilter(new ConfigurationAuthModeProvider(isLocalMode: false))
            .OnResourceExecutionAsync(context, next.Delegate);

        next.Called.Should().BeFalse();
        var result = context.Result.Should().BeOfType<ContentResult>().Subject;
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        result.ContentType.Should().Be(MediaTypeNames.Application.ProblemJson);
        result.Content.Should().Contain(SharedErrorCodes.NotFound);
    }

    /// <summary>In local mode the filter lets the request through and answers nothing itself.</summary>
    /// <returns>A task that completes when the assertions have run.</returns>
    [Fact]
    public async Task InLocalMode_LetsTheRequestThroughAsync()
    {
        var (context, next) = ContextFor();

        await new LocalModeOnlyFilter(new ConfigurationAuthModeProvider(isLocalMode: true))
            .OnResourceExecutionAsync(context, next.Delegate);

        next.Called.Should().BeTrue();
        context.Result.Should().BeNull();
    }

    /// <summary>Builds the context a resource filter is given, over the host's real services.</summary>
    /// <returns>The context, and a recorder for the rest of the pipeline.</returns>
    private (ResourceExecutingContext Context, PipelineRecorder Next) ContextFor()
    {
        var http = new DefaultHttpContext { RequestServices = factory.Services.CreateScope().ServiceProvider };
        var action = new ActionContext(http, new RouteData(), new ActionDescriptor());
        var context = new ResourceExecutingContext(
            action, new List<IFilterMetadata>(), new List<IValueProviderFactory>());
        return (context, new PipelineRecorder(action));
    }

    /// <summary>Stands in for the rest of the pipeline and records whether the filter ran it.</summary>
    /// <param name="action">The action context the executed result is built over.</param>
    private sealed class PipelineRecorder(ActionContext action)
    {
        /// <summary>Gets a value indicating whether the filter ran the rest of the pipeline.</summary>
        public bool Called { get; private set; }

        /// <summary>Gets the delegate the filter is handed as the rest of the pipeline.</summary>
        public ResourceExecutionDelegate Delegate => () =>
        {
            Called = true;
            return Task.FromResult(new ResourceExecutedContext(action, new List<IFilterMetadata>()));
        };
    }
}
