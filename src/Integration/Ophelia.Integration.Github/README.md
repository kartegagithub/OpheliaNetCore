# Ophelia Integration GitHub

A service module for interacting with the GitHub API, facilitating repository management and issue tracking directly from your application.

## 📂 Core Services

### [GitHubService.cs](./GitHubService.cs)
Provides methods to interact with GitHub resources using personal access tokens or OAuth.
- **Key Methods**: `GetRepositories`, `CreateIssue`, `GetPullRequests`, `SearchCode`.

## 📁 Subdirectories
- **[Model](./Model)**: DTOs representing GitHub entities like `Repository`, `Issue`, and `User`.

## 🚀 Usage

```csharp
var github = new GitHubService(accessToken);
var repos = await github.GetRepositories("kartegagithub");
```
