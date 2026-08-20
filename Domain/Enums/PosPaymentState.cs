namespace Domain.Enums;

public enum PosPaymentState
{
    None = 0,
    Pending = 1,
    Succeeded = 2,
    Failed = 3,
    Cancelled = 4,
    TimedOut = 5,
    Unknown = 6
}
