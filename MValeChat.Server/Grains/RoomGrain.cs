using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;

namespace MValeChat.Server.Grains;

class RoomGrain : Grain, Common.IRoomGrain
{
    private List<Guid> UserIds { get; set; } = null!;

    private List<Common.ChatMessage> Messages { get; set; } = null!;

    private IAsyncStream<Common.ChatMessage> MessageStream { get; set; } = null!;

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.UserIds = new List<Guid>();
        this.Messages = new List<Common.ChatMessage>();
        this.MessageStream = this.GetStreamProvider("chat").GetStream<Common.ChatMessage>(StreamId.Create("Chat.Room", this.GetPrimaryKey()));

        return base.OnActivateAsync(cancellationToken);
    }

    public Task<ImmutableList<Guid>> GetUsers() => Task.FromResult(this.UserIds.ToImmutableList());

    public Task<ImmutableList<Common.ChatMessage>> GetMessages() => Task.FromResult(this.Messages.ToImmutableList());

    public Task<StreamId> GetMessageStreamId() => Task.FromResult(this.MessageStream.StreamId);

    public Task JoinUser(Guid userId)
    {
        if (this.UserIds.Contains(userId))
            throw new InvalidOperationException($"User {userId} is already in room {this.GetPrimaryKey()}.");

        this.UserIds.Add(userId);
        var user = this.GrainFactory.GetGrain<IInternalUserGrain>(userId);
        return user.JoinRoom(this);
    }

    public async Task LeaveUser(Guid userId)
    {
        if (!this.UserIds.Contains(userId))
            throw new InvalidOperationException($"User {userId} is not in room {this.GetPrimaryKey()}.");

        this.UserIds.Remove(userId);
        var user = this.GrainFactory.GetGrain<IInternalUserGrain>(userId);
        await user.LeaveRoom(this);
    }

    public async Task SendMessage(Guid userId, string text)
    {
        if (!this.UserIds.Contains(userId))
            throw new InvalidOperationException($"User {userId} is not in room {this.GetPrimaryKey()}.");

        this.Messages.Add(new Common.ChatMessage(this.GetPrimaryKey(), userId, text));
        await this.MessageStream.OnNextAsync(this.Messages.Last());

        foreach (var currentUserId in  this.UserIds)
        {
            var user = this.GrainFactory.GetGrain<IInternalUserGrain>(currentUserId);
            await user.SendMessage(this, this.Messages.Last());
        }
    }
}
