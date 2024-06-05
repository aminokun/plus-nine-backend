using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlusNine.Entities.Dtos.Responses
{
    public class GetObjectiveResponse
    {
        public Guid ObjectiveId { get; set; }
        public Guid UserId { get; set; }
        public string ObjectiveName { get; set; } = string.Empty;
        public int CurrentAmount { get; set; }
        public int AmountToComplete { get; set; }
        public int Progress { get; set; }
        public bool Completed { get; set; }
    }
}
