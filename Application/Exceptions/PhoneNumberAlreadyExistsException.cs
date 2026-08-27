namespace Application.Exceptions;

public class PhoneNumberAlreadyExistsException : Exception
{
    public PhoneNumberAlreadyExistsException() : base("Phone number already exists.") { }
}
