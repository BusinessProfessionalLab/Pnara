namespace Application.Exceptions;

public class RoleInUseException : Exception
{
    public RoleInUseException() : base("Cannot delete a role that is assigned to users.")
    {
    }
}
