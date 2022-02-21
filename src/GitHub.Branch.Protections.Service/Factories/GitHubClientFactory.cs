using Octokit;

namespace GitHub.Branch.Protections.Service.Factories
{
    public static class GitHubClientFactory
    {
        public static IGitHubClient CreateGitHubClient(string token)
        {
            var tokenAuth = new Credentials(token);

            var client = new GitHubClient(new ProductHeaderValue("github-branch-protections"))
            {
                Credentials = tokenAuth
            };

            return client;
        }
    }
}
