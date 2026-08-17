namespace Application.Exceptions;

public class PermissionAlreadyExistsException : Exception
{
    public PermissionAlreadyExistsException() : base("A permission with this name already exists.") { }
}
