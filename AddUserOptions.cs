using System;
using System.Collections.Generic;
using System.Text;

using CommandLine;

namespace DotNetFoundation.AddTeamMembers
{
    [Verb("add-user", HelpText = "Adds users to the specified GitHub team")]
    public class AddUserOption
    {
        /// <summary>
        /// Gets or sets the CSV file name path.
        /// </summary>
        [Option('c', "input-csv-file", HelpText = "The location of the CSV file to read for user names.", Required = true)]
        public string InputCsvFileName { get; set; }

        /// <summary>
        /// Gets or sets the CSV file name path.
        /// </summary>
        [Option('o', "output-csv-file", HelpText = "The location of the CSV file to write for user names of users we couldn't add.", Required = true)]
        public string OutputCsvFileName { get; set; }

        /// <summary>
        /// Gets or sets the team id.
        /// </summary>
        [Option('t', "team-id", Required = true, HelpText = "The team id. This is found in the URL of the team. For example https://github.com/myorg/teams/communications -- communications would be the team ID.")]
        public string TeamId { get; set;  }

        /// <summary>
        /// Gets the role of the user.
        /// </summary>
        [Option('r', "role", HelpText = "Gets the role of the user. Either member or maintainer. Defaults to member.", Default = Role.Member)]
        public Role Role { get; set; } = Role.Member;

        [Option('p', "personal-token", Required = true, HelpText = "The personal ID generated for this process. Go to https://github.com/settings/tokens to create. admin:org is required.")]
        public string PersonalIdToken { get; set; }

        [Option("org-name", Required = true, HelpText = "The name of the organization on GitHub. Eg dotnet-foundation.")]
        public string OrganizationName { get; set; }
    }
}
