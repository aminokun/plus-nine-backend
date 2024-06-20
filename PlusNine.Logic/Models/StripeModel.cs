using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlusNine.Logic.Models
{
    public class StripeModel
    {
        public required string SecretKey { get; set; }

        public required string PublishableKey { get; set; }
        public required string WebhookSecret { get; set; }

    }
}
