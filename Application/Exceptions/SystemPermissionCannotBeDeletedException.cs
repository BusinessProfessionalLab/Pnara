namespace Application.Exceptions;

public class SystemPermissionCannotBeDeletedException : Exception
{
    public SystemPermissionCannotBeDeletedException() : base("System permissions cannot be deleted.") { }
}
