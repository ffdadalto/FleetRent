namespace FleetRent.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainError Error { get; }

    public DomainException(DomainError error)
        : base(error?.Message ?? throw new ArgumentNullException(nameof(error)))
    {
        Error = error;
    }

    public DomainException(DomainError error, Exception innerException)
        : base(error?.Message ?? throw new ArgumentNullException(nameof(error)), innerException)
    {
        Error = error;
    }

    public static DomainException From(DomainError error, Exception innerException = null) =>
        innerException is null ? new DomainException(error) : new DomainException(error, innerException);
}
