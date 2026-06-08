using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameAgentF.Console
{
    public class GameProjectState
    {
        public string UserRequest { get; set; } = string.Empty;
        public string ProductRequirements { get; set; } = string.Empty;
        public string ArchitecturePlan { get; set; } = string.Empty;
        public string GeneratedPhaserCode { get; set; } = string.Empty;
        public bool IsApproved { get; set; } = false;
        public string ReviewFeedback { get; set; } = string.Empty;
    }
}
