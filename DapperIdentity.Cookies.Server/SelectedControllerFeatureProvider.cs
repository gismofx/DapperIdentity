using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace CPE.DapperIdentity.Cookies.Server;

/// <summary>
/// Restricts MVC controller discovery to the controllers a consumer explicitly asked for.
/// </summary>
/// <remarks>
/// <para>
/// Adding an <see cref="AssemblyPart"/> tells MVC where to look; it does not let you say which
/// types count. Without this, one registration call routes <em>every</em> controller in this
/// assembly into the consumer's application. That is not hypothetical here: it is how a
/// half-finished <c>BasicAuthController.Login</c> ended up live in a production app.
/// </para>
/// <para>
/// Feature providers run after default discovery has populated the feature, so the only way to
/// narrow it is to remove. The removal is deliberately scoped to this library's own assembly:
/// a provider that removed anything not on its allow-list would strip the host application's
/// controllers as well, which is a far worse failure than the one it fixes.
/// </para>
/// <para>
/// A duplicate of this type lives in DapperIdentity.Jwt.Server. It is duplicated rather than
/// shared because it must resolve <em>its own</em> assembly, and because the alternative - a
/// shared MVC-aware package - would put an ASP.NET dependency somewhere neither tier wants it.
/// </para>
/// </remarks>
internal sealed class SelectedControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
{
    private static readonly Assembly Library = typeof(SelectedControllerFeatureProvider).Assembly;

    private readonly HashSet<TypeInfo> _allowed;

    /// <param name="allowed">The controller types from this library that should remain routable.</param>
    public SelectedControllerFeatureProvider(params Type[] allowed)
        => _allowed = allowed.Select(t => t.GetTypeInfo()).ToHashSet();

    /// <inheritdoc />
    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
    {
        var ours = feature.Controllers
                          .Where(c => c.Assembly == Library && !_allowed.Contains(c))
                          .ToList();

        foreach (var controller in ours)
        {
            feature.Controllers.Remove(controller);
        }
    }
}
