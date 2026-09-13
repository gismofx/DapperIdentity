using CPE.DapperIdentity.Stores;
using DapperRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using IdentityRole = CPE.DapperIdentity.Core.Models.CustomIdentityRole;
using IdentityUser = CPE.DapperIdentity.Core.Models.CustomIdentityUser;
using IdentityUserClaim = CPE.DapperIdentity.Core.Models.CustomIdentityUserClaim;

namespace CPE.DapperIdentity.Services;

/// <summary>
/// Registration for the Dapper-backed Identity stores.
/// </summary>
/// <remarks>
/// Moved here from the DapperIdentity ("vanilla") project along with the stores themselves. The
/// namespace took the CPE. prefix in Session 33 (D-051) along with every other namespace in the
/// library; it is CPE.DapperIdentity.Services rather than CPE.DapperIdentity.Core.Services because
/// the move preserved its shape, which is a wrinkle worth knowing when hunting the using. Roughly
/// 215 lines of commented-out cookie and Identity-UI registration helpers were dropped in the same
/// move: they had been dead for the whole life of the JWT line, and they documented an API the
/// README still advertised but the code no longer offered.
/// </remarks>
public static class DapperIdentityServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Dapper repositories and the Identity store implementations they back.
    /// </summary>
    /// <remarks>
    /// TryAdd rather than Add, so a host that has already supplied its own store keeps it. The
    /// caller is expected to have registered a database connection instantiator for the
    /// repositories before calling this.
    /// </remarks>
    public static IServiceCollection TryAddDapperIdentityDatabaseStores(this IServiceCollection services)
    {
        services.AddTransientRepository<IdentityUser>();
        services.AddTransientRepository<IdentityRole>();
        services.AddTransientRepository<IdentityUserClaim>();

        services.TryAddTransient<IUserStore<IdentityUser>, UserStore>();
        services.TryAddTransient<IRoleStore<IdentityRole>, RoleStore>();

        // UserStore answers both contracts: Identity resolves claims through a separate interface,
        // but it is the same Dapper-backed implementation behind it.
        services.TryAddTransient<IUserClaimStore<IdentityUser>, UserStore>();

        return services;
    }
}
