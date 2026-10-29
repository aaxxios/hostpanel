using Innovayse.Hostpanel.Application.Common.Interfaces;

namespace Innovayse.Hostpanel.Integration.Tests.Auth;

/// <summary>
/// The services the setup token seeder tests read from a scope.
/// </summary>
/// <param name="Context">The data context.</param>
/// <param name="Store">The setup token store the seeder issues through.</param>
/// <param name="Roles">The subject role store.</param>
public sealed record SeederServices(IHostpanelDbContext Context, ISetupTokenStore Store, ISubjectRoleStore Roles);
