using Orleans;

namespace MValeChat.Common;

[GenerateSerializer]
public record ChatMessage(Guid id, Guid RoomId, Guid UserId, string Text)
{
    public ChatMessage(Guid roomId, Guid userId, string text) : this(Guid.NewGuid(), roomId, userId, text)
    {
    }
}
