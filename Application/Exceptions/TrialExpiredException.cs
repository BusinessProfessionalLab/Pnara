namespace Application.Exceptions;

public class TrialExpiredException : Exception
{
    public TrialExpiredException() : base("مدت زمان استفاده شما به پایان رسیده است. لطفاً برای تمدید مجوز با پشتیبانی تماس بگیرید.")
    {
    }
}

