using System.Reflection;
using Innovayse.Hostpanel.API.Authorization;
using Innovayse.Hostpanel.Domain.Clients.Enums;
using Innovayse.Hostpanel.Shared.Authorization;
using Innovayse.Hostpanel.Shared.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Innovayse.Hostpanel.Architecture.Tests;

/// <summary>
/// Every client portal action decides who may call it: either it names the <see cref="ClientPermission"/>
/// it needs, or it says in writing that every user of the account may call it. A route added later
/// cannot reach production without that decision.
/// </summary>
/// <remarks>
/// A "client portal action" is an action behind <see cref="Policies.Client"/>, on the action or on its
/// controller. The permission check itself is covered by the integration tests; this file covers the
/// part a reviewer could miss, that the declaration exists at all.
/// </remarks>
public sealed class ClientPortalPermissionTests
{
    /// <summary>
    /// Flags no portal endpoint exists for yet. Listed here so the day an endpoint appears the test
    /// forces it to declare the flag, and so a flag nobody uses cannot hide among the others.
    /// </summary>
    private static readonly ClientPermission[] ReservedPermissions =
    [
        ClientPermission.ViewAcceptQuotes,
        ClientPermission.ViewManageAffiliate,
    ];

    /// <summary>Every action method of every controller in the API assembly.</summary>
    /// <returns>The controller and action method of each HTTP-bound public method.</returns>
    private static IReadOnlyList<ControllerAction> AllActions()
    {
        return Layers.AuthoredTypesOf(Layers.Api)
            .Where(type => type.IsClass && !type.IsAbstract && typeof(BaseApiController).IsAssignableFrom(type))
            .SelectMany(controller => controller
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => method.GetCustomAttributes<HttpMethodAttribute>(true).Any())
                .Select(method => new ControllerAction(controller, method)))
            .ToList();
    }

    /// <summary>Whether the action sits behind <see cref="Policies.Client"/>, on itself or its controller.</summary>
    /// <param name="controller">The declaring controller.</param>
    /// <param name="action">The action method.</param>
    /// <returns><c>true</c> for a client portal action.</returns>
    private static bool IsClientPortalAction(Type controller, MethodInfo action)
    {
        return action.GetCustomAttributes<AuthorizeAttribute>(true)
            .Concat(controller.GetCustomAttributes<AuthorizeAttribute>(true))
            .Any(authorize => authorize.Policy == Policies.Client);
    }

    /// <summary>The permission declarations that apply to an action, on itself and on its controller.</summary>
    /// <param name="controller">The declaring controller.</param>
    /// <param name="action">The action method.</param>
    /// <returns>The permission-requiring declarations and the every-user markers found.</returns>
    private static PermissionDeclarations DeclarationsOf(
        Type controller, MethodInfo action)
    {
        var required = action.GetCustomAttributes<RequiresClientPermissionAttribute>(true)
            .Concat(controller.GetCustomAttributes<RequiresClientPermissionAttribute>(true))
            .ToList();
        var open = action.GetCustomAttributes<AvailableToEveryClientUserAttribute>(true)
            .Concat(controller.GetCustomAttributes<AvailableToEveryClientUserAttribute>(true))
            .Count();
        return new(required, open);
    }

    /// <summary>The scan finds the portal; a rule over zero actions would pass for the wrong reason.</summary>
    [Fact]
    public void TheScan_FindsTheClientPortal()
    {
        var portal = AllActions().Where(a => IsClientPortalAction(a.Controller, a.Action)).ToList();

        portal.Select(a => a.Controller).Distinct().Should().HaveCountGreaterThanOrEqualTo(8);
        portal.Should().HaveCountGreaterThanOrEqualTo(50);
    }

    /// <summary>Every client portal action declares exactly one of the two decisions.</summary>
    [Fact]
    public void EveryClientPortalAction_DeclaresAPermissionOrIsOpenToEveryUser()
    {
        var offenders = AllActions()
            .Where(a => IsClientPortalAction(a.Controller, a.Action))
            .Where(a =>
            {
                var (required, open) = DeclarationsOf(a.Controller, a.Action);
                return required.Count + open != 1;
            })
            .Select(a => $"{a.Controller.Name}.{a.Action.Name}")
            .ToList();

        offenders.Should().BeEmpty(
            "each client portal action carries [RequiresClientPermission(...)] or [AvailableToEveryClientUser], "
            + "exactly one of them, on the action or its controller");
    }

    /// <summary>A permission declaration on an action that is not a portal action guards nothing and misleads.</summary>
    [Fact]
    public void PermissionDeclarations_AppearOnlyOnClientPortalActions()
    {
        var offenders = AllActions()
            .Where(a => !IsClientPortalAction(a.Controller, a.Action))
            .Where(a =>
            {
                var (required, open) = DeclarationsOf(a.Controller, a.Action);
                return required.Count + open > 0;
            })
            .Select(a => $"{a.Controller.Name}.{a.Action.Name}")
            .ToList();

        offenders.Should().BeEmpty("only an action behind Policies.Client has account permissions to check");
    }

    /// <summary>
    /// Every flag guards at least one portal action, bar the two reserved for endpoints that do not exist
    /// yet, so a permission an administrator can grant is never silently decorative.
    /// </summary>
    [Fact]
    public void EveryDefinedPermission_GuardsAPortalActionOrIsReserved()
    {
        var used = AllActions()
            .Where(a => IsClientPortalAction(a.Controller, a.Action))
            .SelectMany(a => DeclarationsOf(a.Controller, a.Action).Required)
            // An action that asks for full access names every bit at once; it guards no single flag.
            .Where(declaration => declaration.Permission != ClientPermission.All)
            .Aggregate(ClientPermission.None, (all, declaration) => all | declaration.Permission);

        var defined = Enum.GetValues<ClientPermission>()
            .Where(flag => flag != ClientPermission.None && flag != ClientPermission.All);

        var unguarded = defined.Where(flag => (used & flag) != flag).Except(ReservedPermissions).ToList();

        unguarded.Should().BeEmpty("a granted permission must change what the user can do");
        ReservedPermissions.Where(flag => (used & flag) == flag).Should().BeEmpty(
            "a reserved flag that is now used belongs out of ReservedPermissions");
    }

    /// <summary>A declaration naming no flag, or a bit the enum never defined, cannot be constructed.</summary>
    /// <param name="value">A value that is not a grantable permission.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(0x2000)]
    [InlineData(-1)]
    public void ARequiresClientPermission_RefusesAValueThatGrantsNothingOrIsUnknown(int value)
    {
        var construct = () => new RequiresClientPermissionAttribute((ClientPermission)value);

        construct.Should().Throw<ArgumentOutOfRangeException>();
    }
}
