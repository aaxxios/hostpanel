using Innovayse.Hostpanel.Shared.Infrastructure;
using Innovayse.Hostpanel.Shared.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;

namespace Innovayse.Hostpanel.API.Extensions;

/// <summary>
/// Wires Wolverine as the process's only bus: handler discovery over the API and Application
/// assemblies, the validation and logging policies every chain runs through, and the two
/// middleware types those policies insert.
/// </summary>
public static class MessagingRegistration
{
    /// <summary>Adds Wolverine to the host.</summary>
    /// <param name="builder">The host builder, needed for both its container and <c>UseWolverine</c>.</param>
    /// <returns><paramref name="builder"/>, for chaining.</returns>
    public static WebApplicationBuilder AddHostpanelWolverine(this WebApplicationBuilder builder)
    {
        // The two policies below insert these into every chain, and Wolverine's generated code
        // resolves them from the container, so they are registered here or every dispatch fails.
        builder.Services.AddTransient<ValidationMiddleware>();
        builder.Services.AddTransient<LoggingMiddleware>();
        builder.Services.AddTransient<DemoGuardMiddleware>();
        builder.Host.UseWolverine(opts =>
        {
            opts.Discovery.IncludeAssembly(typeof(MessagingRegistration).Assembly);
            opts.Discovery.IncludeAssembly(typeof(Innovayse.Hostpanel.Application.Clients.Commands.AcceptInvitation.AcceptInvitationCommand).Assembly);

            // Validation at position 0 of every discovered chain (rules/cqrs.md), so no handler has to
            // remember to ask and no rule can be written in a validator and quietly never run. A
            // message with no validator passes straight through. A policy rather than an opt-in marker:
            // a handler whose author forgot the marker would look identical to one whose rules passed.
            //
            // On failure the middleware throws FluentValidation's ValidationException, answered as a
            // 400 VALIDATION_ERROR with the field errors grouped by property. Not a 500: the request
            // was wrong, not the server.
            opts.Policies.Add<ValidationPolicy>();

            // "Handling {RequestName} for User {UserId}" before and "Handled … successfully" after,
            // on every chain.
            opts.Policies.Add<LoggingPolicy>();

            // Added last on purpose: it inserts at position 0, so the guard added last
            // runs first and a restricted command is refused before it is validated or logged.
            opts.Policies.Add<DemoGuardPolicy>();
        });

        return builder;
    }
}
