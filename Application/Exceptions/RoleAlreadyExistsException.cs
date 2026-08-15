namespace Application.Exceptions;

public class RoleAlreadyExistsException : Exception
{
    public RoleAlreadyExistsException() : base("A role with this name already exists.")
    {
    }
}
