using Microsoft.AspNetCore.Authorization;
using SafetyOps.Api.Domain;

namespace SafetyOps.Api.Features.Access;

/// <summary>
/// Endpoint policies. Each requires the minimum role on at least one org unit; which records a
/// request may touch is then decided per unit by the services through <see cref="IAccessScope"/>.
/// </summary>
public static class AccessPolicies
{
    /// <summary>Viewer or above somewhere: may call read endpoints.</summary>
    public const string Read = "Access.Read";

    /// <summary>Manager or above somewhere: may call create/update/delete endpoints.</summary>
    public const string Write = "Access.Write";

    /// <summary>Admin somewhere: may manage role assignments.</summary>
    public const string ManageRoles = "Access.ManageRoles";

    public static IServiceCollection AddSafetyOpsAccess(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAccessScope, AccessScope>();
        services.AddScoped<IAuthorizationHandler, OrgRoleHandler>();
        services.AddScoped<IAccessService, AccessService>();

        services.AddAuthorizationBuilder()
            .AddPolicy(Read, p => p.RequireAuthenticatedUser().AddRequirements(new OrgRoleRequirement(Role.Viewer)))
            .AddPolicy(Write, p => p.RequireAuthenticatedUser().AddRequirements(new OrgRoleRequirement(Role.Manager)))
            .AddPolicy(ManageRoles, p => p.RequireAuthenticatedUser().AddRequirements(new OrgRoleRequirement(Role.Admin)));
        return services;
    }
}

/// <summary>Requires at least <see cref="Minimum"/> on an org unit.</summary>
public sealed record OrgRoleRequirement(Role Minimum) : IAuthorizationRequirement;

/// <summary>Authorizing against a specific unit rather than "anywhere".</summary>
public sealed record OrgUnitResource(int OrgUnitId);

/// <summary>
/// Succeeds when the user's effective role meets the requirement: on the given
/// <see cref="OrgUnitResource"/>, or on any unit when evaluated for an endpoint.
/// </summary>
public sealed class OrgRoleHandler(IAccessScope scope) : AuthorizationHandler<OrgRoleRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OrgRoleRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return;

        var access = await scope.GetAsync();
        var allowed = context.Resource is OrgUnitResource unit
            ? access.Can(unit.OrgUnitId, requirement.Minimum)
            : access.HasAnywhere(requirement.Minimum);
        if (allowed)
            context.Succeed(requirement);
    }
}
