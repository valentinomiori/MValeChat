using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MValeChat.Web.Models
{
    public class UserDto
    {
        public Guid Id { get; set; }

        [Required]
        public List<Guid> Rooms { get; set; }

        [Required]
        public List<Common.ChatMessage> Messages { get; set; }
    }
}
