using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace DotNetFoundation.AddTeamMembers
{
    /// <summary>
    /// The role for the user in the team.
    /// </summary>
    public enum Role
    {
        [JsonProperty("member")]
        Member,

        [JsonProperty("maintainer")]
        Maintainer
    }
}
