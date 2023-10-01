namespace MyCourse.Models.Exceptions.Infrastructure;
public class SendException : Exception
{
    public SendException() : base("Constraint violated")
    {
    }
}
