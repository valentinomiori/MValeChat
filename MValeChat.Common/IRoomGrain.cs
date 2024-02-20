using System.Collections.Immutable;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;

namespace MValeChat.Common;

public interface IRoomGrain : IGrainWithGuidKey
{
    Task JoinUser(Guid userId);
    Task LeaveUser(Guid userId);
    Task<ImmutableList<Guid>> GetUsers();
    Task<ImmutableList<ChatMessage>> GetMessages();
    Task<StreamId> GetMessageStreamId();
    Task SendMessage(Guid userId, string text);
}
