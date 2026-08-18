namespace Application.Exceptions;

public class SystemRoleCannotBeModifiedException : Exception
{
    public SystemRoleCannotBeModifiedException() : base("System roles cannot be modified.") { }

    public SystemRoleCannotBeModifiedException(string message) : base(message) { }
}
