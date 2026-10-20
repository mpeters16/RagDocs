namespace RagDocs.Application.Abstractions;

public interface ICurrentUser
{
    // The id of the user making the current request.
    string UserId { get; }
}