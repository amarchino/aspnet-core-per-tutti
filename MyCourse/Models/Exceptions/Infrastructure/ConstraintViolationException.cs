namespace MyCourse.Models.Exceptions.Infrastructure;
public class ConstraintViolationException : Exception
{
    public ConstraintViolationException(Exception innerException) : base("Constraint violated", innerException)
    {
    }
}
