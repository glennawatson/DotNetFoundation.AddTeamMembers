# DotNetFoundation - Add team users by CSV

This is a command line utility for adding users to a organisation team by using a CSV file.

It will read in a CSV input file with just the user names (make sure you have the header "UserName" at the top), and then will add users contained within. It will then output a CSV file with any users it failed to add to try another day.

## GitHub setup

You must add a personal access token by going to https://github.com/settings/tokens - click "New" and select `admin:org`. It is recommended after finishing this process to delete that Personal Access Token since it has quite a lot of security rights. Be aware you will have to store that token away since GitHub will only show it to you once.

## Command Line Arguments

```txt
  -c, --input-csv-file     Required. The location of the CSV file to read for user names.

  -o, --output-csv-file    Required. The location of the CSV file to write for user names of users we couldn't add.

  -t, --team-id            Required. The team id. This is found in the URL of the team. For example
                           https://github.com/myorg/teams/communications -- communications would be the team ID.

  -r, --role               (Default: Member) Gets the role of the user. Either member or maintainer. Defaults to
                           member.

  -p, --personal-token     Required. The personal ID generated for this process. Go to
                           https://github.com/settings/tokens to create. write:org is required.

  --org-name               Required. The name of the organization on GitHub. Eg dotnet-foundation.

  --help                   Display this help screen.

  --version                Display version information.
```

A sample for adding to 'dynamic-data' team on 'ReactiveUI':

```bash
DotNetFoundation.AddTeamMembers.exe -c test.txt -o test-out.txt -t "dynamic-data" -p "personal-access-token" --org-name ReactiveUI
```

## CSV Input/Output File Format

The CSV file would be in the following format:

```csv
UserName
glennawatson
```
