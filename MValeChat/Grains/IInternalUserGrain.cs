using System.Threading.Tasks;
using Orleans;

namespace MValeChat.Server.Grains;

interface IInternalUserGrain : IGrainWithGuidKey
{
    Task JoinRoom(Common.IRoomGrain room);
    Task LeaveRoom(Common.IRoomGrain room);
    Task SendMessage(Common.IRoomGrain room, Common.ChatMessage message);
}
