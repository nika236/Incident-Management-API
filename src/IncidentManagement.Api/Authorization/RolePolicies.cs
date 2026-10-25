namespace IncidentManagement.Api.Authorization;

public static class RolePolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string ResponderOrAdmin = "ResponderOrAdmin";
    public const string AnyAuthenticated = "AnyAuthenticated";
}
