using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Refit;

namespace DotNetFoundation.AddTeamMembers
{
    /// <summary>
    /// A interface that describes the API.
    /// </summary>
    public interface IGitHubTeams
    {
        /// <summary>
        /// Adds a user to the team.
        /// </summary>
        /// <param name="teamId">The team ID to add.</param>
        /// <param name="userName">The user name of the user.</param>
        /// <param name="role">The role of the user.</param>
        /// <param name="authorization">The personal access token for the user.</param>
        /// <returns></returns>
        [Put("/teams/{teamId}/memberships/{userName}")]
        [Headers("User-Agent: Custom-DotNetFoundation-Utility")]
        Task AddUser(string teamId, string userName, Role role, [Header("Authorization")] string authorization);

        [Get("/orgs/{organisationName}/teams/{teamName}")]
        [Headers("User-Agent: Custom-DotNetFoundation-Utility")]
        Task<Team> GetTeam(string organisationName, string teamName, [Header("Authorization")] string authorization);
    }
}
