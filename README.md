# Branch Protections Service
This service was built to help an organization ensure that a peer review process is in place on all repositories in their GitHub organization.
It uses GitHub's webhooks at the organization level to evaluate and apply a branch protection policy on the default branch.
This policy is considered the absolute minimum and allows teams to go above and beyond.

## Understanding the Solution
For details on the problem statement and approach, see the [README.md][solution] in the `docs` folder.

To minimize maintenance and keep the implementation as simple as possilbe, two key NuGet packages were used.

- `Octokit.Webhooks.AspNetCore`
- `Octokit`

The `Octokit.Webhooks.AspNetCore` package is provided by GitHub which allows us to launch a web application and have it accept webhooks from GitHub.
Logic can then be written to handle the events in what ever way we deem necessiary.

The `Octokit` is provided by GitHub and gives us a way to programmatically interact with GitHub via C#.

These two packages prevent us from having to write our own API client to interact with GitHub and speed up development and implementation efforts.

## Setup and Configuring
This solution is a .NET 6.0 base project can be run from Visual Studio or Vsiual Studio Code. Simply clone the repo and run a .NET build command as such:
```shell
$> dotnet build .\src\GitHub.Branch.Protections.sln
```

Once built, two .NET secrets need to be set for the application to run locally. The first is `secret` which is a secret key to ensure security for GitHub webhook to fire. The second is `token` which is a GitHub Personal Access Token which has full access to repositories.

```shell
$> dotnet user-secrets set "secret" "some value here" --project ./src/GitHub.Branch.Protections.Service/GitHub.Branch.Protections.Service.csproj
Successfully saved secret = some value here to the secret store.
```

```shell
$> dotnet user-secrets set "token" "gh.token" --project ./src/GitHub.Branch.Protections.Service/GitHub.Branch.Protections.Service.csproj
Successfully saved token = gh.token the secret store.
```

### Running Locally
Using your editor or IDE of choice, simply launch/debug the project as you would with any web based .NET project.
Once running, we need to use a tool like `ngrok` to proxy publicly exposed events to your local host.
For details, see [Creating Webhooks - Exposing localhost to the internet][webhooks] in the GitHub docs.

Once the proxy is up and running, we need to ensure that the webhook is created at the organization.
Simply navigate to the settings of the organzation and select "Webhooks".
Click the "Add webhook" button in the upper right of the screen and fill in as follows:
- Payload URL - This will be the `ngrok` URL with the route `api/github/webhooks`. This route is the default one setup by the `Octokit.Webhooks.AspNetCore` package but can be changed if necessary.
- Content type - This should be set to `application/json`.
- Secret - Any secret you wish to use to securer this application with.
- Which events would you like to trigger this webhook? - Select "Send me everything".
- Active - Checked

Once done, a ping test should fire and besuccessful. If not, check the outout of `ngrok` and the .NET debugger/console for details on the error.

## Next Steps and/or Improvements
- Add a way to allow for a temporary bypass of the rules
- Allow rules to be defined for the organization and for a repo
- Develop unit tests
- Setup GitHub Actions
- Setup production deployment

[solution]: docs/README.md
[webhooks]: https://docs.github.com/en/developers/webhooks-and-events/webhooks/creating-webhooks#exposing-localhost-to-the-internet