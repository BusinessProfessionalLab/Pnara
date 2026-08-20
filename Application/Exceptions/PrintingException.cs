namespace Application.Exceptions;

public class PrintingException(string message, Exception? innerException = null)
    : Exception(message, innerException);
