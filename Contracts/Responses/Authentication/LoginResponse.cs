using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Responses.Authentication
{
    public record LoginResponse
    {
        public bool IsChallenge { get; set; }
        public string? ChallengeId { get; set; }

        public string? ChallengeTextImage { get; set; }
        public string? ChallengeIconsImage { get; set; }
        public string? ChallengeTargetIconImage { get; set; }

        public string? Token { get; set; }
        public string? Locale { get; set; }

    }

}
