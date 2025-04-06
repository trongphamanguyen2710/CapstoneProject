namespace RoleBaseAuthorizationLibrary;

public enum Role
{
    Admin = 100,
    Moderator = 200,
    Driver = 300,
    Supervisor = 400,
    Guest = 500,
}

public enum AuthorizationType
{
    Include,
    Exclude,
}
