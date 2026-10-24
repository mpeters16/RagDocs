using RagDocs.Application.Abstractions;

namespace RagDocs.Api.Common;

// Temporary: everyone is "dev-user" until authentication 
internal sealed class CurrentUser : ICurrentUser
{
    public string UserId => "dev-user";
}