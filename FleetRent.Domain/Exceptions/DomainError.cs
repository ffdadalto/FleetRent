namespace FleetRent.Domain.Exceptions;

public sealed record DomainError(string Message)
{
    public override string ToString() => Message;
}
