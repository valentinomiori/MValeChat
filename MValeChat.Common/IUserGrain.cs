using System.Collections.Immutable;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;

namespace MValeChat.Common;

public interface IUserGrain : IGrainWithGuidKey
{
    Task JoinRoom(Guid roomId);
    Task LeaveRoom(Guid roomId);
    Task<ImmutableList<Guid>> GetRooms();
    Task<ImmutableList<ChatMessage>> GetMessages();
    Task<StreamId> GetMessageStreamId();
    Task SendMessage(Guid roomId, string text);
}
