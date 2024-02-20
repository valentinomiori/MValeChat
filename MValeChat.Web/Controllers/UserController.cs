using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace MValeChat.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    IGrainFactory GrainFactory { get; }

    public UserController(IGrainFactory grainFactory)
    {
        GrainFactory = grainFactory;
    }

    private Common.IUserGrain GetUserGrain(Guid id)
    {
        return this.GrainFactory.GetGrain<Common.IUserGrain>(id);
    }

    [HttpGet("{id:guid:required}")]
    public async Task<Models.UserDto> Get([FromRoute] Guid id)
    {
        var user = this.GetUserGrain(id);
        return new Models.UserDto()
        {
            Id = user.GetPrimaryKey(),
            Rooms = (await user.GetRooms()).ToList(),
            Messages = (await user.GetMessages()).ToList()
        };
    }

    [HttpPost("{id:guid:required}/[action]")]
    public async Task JoinRoom([FromRoute] Guid id, [FromQuery, Required] Guid? roomId)
    {
        var user = this.GetUserGrain(id);
        await user.JoinRoom(roomId!.Value);
    }

    [HttpPost("{id:guid:required}/[action]")]
    public async Task LeaveRoom([FromRoute] Guid id, [FromQuery, Required] Guid? roomId)
    {
        var user = this.GetUserGrain(id);
        await user.LeaveRoom(roomId!.Value);
    }

    [HttpPost("{id:guid:required}/[action]")]
    public async Task SendMessage([FromRoute] Guid id, [FromQuery, Required] Guid? roomId, [FromBody] string text)
    {
        var user = this.GetUserGrain(id);
        await user.SendMessage(roomId!.Value, text);
    }
}
