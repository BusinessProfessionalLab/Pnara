namespace Application.Exceptions;

public class CannotAssignAdminRoleException : Exception
{
    public CannotAssignAdminRoleException() : base("The Admin role cannot be assigned to users.") { }

    public CannotAssignAdminRoleException(string message) : base(message) { }
}
