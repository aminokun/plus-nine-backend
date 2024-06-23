using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlusNine.Entities.Dtos.Responses
{
    public class FriendShipResponse
    {
        public Guid FriendId {  get; set; }
        public required string Username {  get; set; }
    }
}
