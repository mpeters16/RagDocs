using RagDocs.Application.Abstractions;

namespace RagDocs.UnitTests.Application.Fakes;

internal sealed class FakeCurrentUser : ICurrentUser
{
    public string UserId { get; set; } = "user-a";
}