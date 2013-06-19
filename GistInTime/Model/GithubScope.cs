
namespace GistInTime.Model
{
    public static class GithubScope
    {
        /// <summary>
        /// Read/write access to profile info only. Note: this scope includes user:email and user:follow.
        /// </summary>
        public const string User = "user";
        /// <summary>
        /// Read access to a user’s email addresses.
        /// </summary>
        public const string UserEmail = "user:email";
        /// <summary>
        /// Access to follow or unfollow other users.
        /// </summary>
        public const string UserFollow = "user:follow";
        /// <summary>
        /// Read/write access to public repos and organizations.
        /// </summary>
        public const string PublicRepo = "public_repo";
        /// <summary>
        /// Read/write access to public and private repos and organizations.
        /// </summary>
        public const string Repo = "repo";
        /// <summary>
        /// Read/write access to public and private repository commit statuses. This scope is only 
        /// necessary to grant other users or services access to private repository commit statuses 
        /// without granting access to the code. The repo and public_repo scopes already include 
        /// access to commit status for private and public repositories respectively.
        /// </summary>
        public const string RepoStatus = "repo:status";
        /// <summary>
        /// Delete access to adminable repositories.
        /// </summary>
        public const string DeleteRepo = "delete_repo";
        /// <summary>
        /// Read access to a user’s notifications. repo is accepted too.
        /// </summary>
        public const string Notifications = "notifications";
        /// <summary>
        /// Write access to gists.
        /// </summary>
        public const string Gist = "gist";
    }
}
