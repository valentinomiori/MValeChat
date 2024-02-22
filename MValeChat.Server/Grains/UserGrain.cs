using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using Orleans.Streams;

namespace MValeChat.Server.Grains;

class UserGrain : Grain, Common.IUserGrain, IInternalUserGrain
{
    private List<Guid> RoomIds { get; set; } = null!;

    private List<Common.ChatMessage> Messages { get; set; } = null!;

    private IAsyncStream<Common.ChatMessage> MessageStream { get; set; } = null!;

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.RoomIds = new List<Guid>();
        this.Messages = new List<Common.ChatMessage>();
        this.MessageStream = this.GetStreamProvider("chat").GetStream<Common.ChatMessage>(StreamId.Create("Chat.User", this.GetPrimaryKey()));

        return base.OnActivateAsync(cancellationToken);
    }

    [ReadOnly]
    public Task<ImmutableList<Guid>> GetRooms() => Task.FromResult(this.RoomIds.ToImmutableList());

    [ReadOnly]
    public Task<ImmutableList<Common.ChatMessage>> GetMessages() => Task.FromResult(this.Messages.ToImmutableList());

    [ReadOnly]
    public Task<StreamId> GetMessageStreamId() => Task.FromResult(this.MessageStream.StreamId);

    [ReadOnly]
    public async Task JoinRoom(Guid roomId)
    {
        if (this.RoomIds.Contains(roomId))
            throw new InvalidOperationException($"User {this.GetPrimaryKey()} is already in room {roomId}.");

        using var scope = RequestContext.AllowCallChainReentrancy();
        var room = this.GrainFactory.GetGrain<Common.IRoomGrain>(roomId);
        await room.JoinUser(this.GetPrimaryKey());
    }

    [ReadOnly]
    public async Task LeaveRoom(Guid roomId)
    {
        if (!this.RoomIds.Contains(roomId))
            throw new InvalidOperationException($"User {this.GetPrimaryKey()} is not in room {roomId}.");

        using var scope = RequestContext.AllowCallChainReentrancy();
        var room = this.GrainFactory.GetGrain<Common.IRoomGrain>(roomId);
        await room.LeaveUser(this.GetPrimaryKey());
    }

    [ReadOnly]
    public async Task SendMessage(Guid roomId, string text)
    {
        if (!this.RoomIds.Contains(roomId))
            throw new InvalidOperationException($"User {this.GetPrimaryKey()} is not in room {roomId}.");

        using var scope = RequestContext.AllowCallChainReentrancy();
        var room = this.GrainFactory.GetGrain<Common.IRoomGrain>(roomId);
        await room.SendMessage(this.GetPrimaryKey(), text);
    }

    public Task JoinRoom(Common.IRoomGrain room)
    {
        this.RoomIds.Add(room.GetPrimaryKey());
        return Task.CompletedTask;
    }

    public Task LeaveRoom(Common.IRoomGrain room)
    {
        this.RoomIds.Remove(room.GetPrimaryKey());
        return Task.CompletedTask;
    }

    public async Task SendMessage(Common.IRoomGrain room, Common.ChatMessage message)
    {
        this.Messages.Add(message);
        await this.MessageStream.OnNextAsync(this.Messages.Last());
    }
}
