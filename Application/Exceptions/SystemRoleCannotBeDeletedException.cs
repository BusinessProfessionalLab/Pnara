namespace Application.Exceptions;

public class SystemRoleCannotBeDeletedException : Exception
{
    public SystemRoleCannotBeDeletedException() : base("System roles cannot be deleted.")
    {
    }
}
