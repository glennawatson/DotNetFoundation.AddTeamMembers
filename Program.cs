using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CommandLine;

using CsvHelper;

using Polly;

using Refit;

namespace DotNetFoundation.AddTeamMembers
{
    /// <summary>
    /// Our main application entry point.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The number of times to retry for each user name attempt.
        /// </summary>
        private const int RetryCount = 5;

        /// <summary>
        /// A jitterer to add to the HTTP to avoid hitting the server always at the same point. Avoids a whole heap of services hitting the service all at once.
        /// </summary>
        private static readonly Random _jitterer = new Random();

        /// <summary>
        /// The http statuses we can retry on.
        /// </summary>
        private static readonly HttpStatusCode[] _httpStatusCodesWorthRetrying =
            {
                HttpStatusCode.RequestTimeout, // 408
                HttpStatusCode.InternalServerError, // 500
                HttpStatusCode.BadGateway, // 502
                HttpStatusCode.ServiceUnavailable, // 503
                HttpStatusCode.GatewayTimeout // 504
            };

        /// <summary>
        /// Gets the API instance. This will call the GitHub service for us.
        /// </summary>
        private static readonly IGitHubTeams _team = RestService.For<IGitHubTeams>("https://api.github.com");

        /// <summary>
        /// Our main entry point into the application.
        /// </summary>
        /// <param name="args">The arguments from the command line.</param>
        /// <returns>The result, 0 for no errors, 1 for errors.</returns>
        public static async Task<int> Main(string[] args)
        {
            try
            {
                // Parse our settings, and then run the add logic.
                return await Parser.Default.ParseArguments<AddUserOption>(args)
                    .MapResult(
                        RunAddAndReturnExitCode,
                        errs => Task.FromResult(1));
            }
            catch (ApiException ex)
            {
                Console.WriteLine($"Problem getting organisation details. We got a response back\r\n {ex.StatusCode}: {ex.Content}\r\n{ex.Uri}");
                return 1;
            }
        }

        private static async Task<int> RunAddAndReturnExitCode(AddUserOption opts)
        {
            if (File.Exists(opts.OutputCsvFileName))
            {
                File.Delete(opts.OutputCsvFileName);
            }

            Console.WriteLine("Starting to process input file: " + opts.InputCsvFileName);

            // Get the organisation details. We are interested in the Id which GitHub stores internally.
            var orgDetails = await _team.GetTeam(opts.OrganizationName, opts.TeamId, "token " + opts.PersonalIdToken);

            // Read in all the entries inside the CSV file (simple file with just a entry with header UserName and then the entries)
            // and then process adding each user name listed.
            using (var reader = new StreamReader(opts.InputCsvFileName))
            using (var csv = new CsvReader(reader))
            {
                var records = csv.GetRecords<CsvFileEntry>();

                // Limit to maximum of 5 requests at a time.
                var bulkHead = Policy.BulkheadAsync<(bool added, string userName)>(5, int.MaxValue);

                // Process the requests for the entries inside the CSV file.
                var tasks = records.Select(entry =>
                    bulkHead.ExecuteAsync(
                        async () =>
                            {
                                try
                                {
                                    // Do a retry policy of checking for retryable http status codes.
                                    // Retry for maximum of 5 times if it matches this policy
                                    // with exponetial backoff.
                                    var result = await Policy.Handle<ApiException>(exception => _httpStatusCodesWorthRetrying.Contains(exception.StatusCode))
                                        .WaitAndRetryAsync(
                                            RetryCount,
                                            retryAttempt => TimeSpan.FromSeconds(Math.Pow(0.9, retryAttempt)) + TimeSpan.FromMilliseconds(_jitterer.Next(0, 100)))
                                        .ExecuteAsync(
                                                     async () =>
                                                         {
                                                             await _team.AddUser(orgDetails.Id, entry.UserName, opts.Role, "token " + opts.PersonalIdToken);
                                                             return (true, entry.UserName);
                                                         });
                                    return result;
                                }
                                catch (ApiException ex)
                                {
                                    Console.WriteLine($"Problem adding user {entry.UserName}. We got a response back {ex.StatusCode}: {ex.Content}");
                                    return (false, entry.UserName);
                                }
                            }));

                // Get all the results after we are done.
                var results = await Task.WhenAll(tasks);

                // Get any failed users.
                var failedUsers = results.Where(x => !x.added).Select(x => new CsvFileEntry() { UserName = x.userName }).ToList();

                // If we have no failed users great, just tell the user that and return.
                if (failedUsers.Count == 0)
                {
                    Console.WriteLine("All users have been successfully added.");
                    return 0;
                }

                // If we got failed users, output it for them to the CSV file, so they can retry later for those users.
                Console.WriteLine("Writing users which we failed to add to: " + opts.OutputCsvFileName);
                using (var writer = new StreamWriter(opts.OutputCsvFileName))
                using (var outCsv = new CsvWriter(writer))
                {
                    outCsv.WriteRecords(failedUsers);
                }

                return 1;
            }
        }
    }
}
