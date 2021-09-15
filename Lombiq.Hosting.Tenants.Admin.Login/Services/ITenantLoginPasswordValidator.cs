namespace Lombiq.Hosting.Tenants.Admin.Login.Services
{
    /// <summary>
    /// Service for storing a randomly generated password that allows you to log in to tenants.
    /// The service also can decide whether the passwords received as parameters are the same as the stored one.
    /// </summary>
    public interface ITenantLoginPasswordValidator
    {
        /// <summary>
        /// Gets a randomly generated password.
        /// </summary>
        public string Password { get; }

        /// <summary>
        /// Compares <paramref name="password"/> with <see cref="Password"/>.
        /// </summary>
        /// <param name="password">The password to validate.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="password"/> is same as <see cref="Password"/> ,
        /// <see langword="false"/> otherwise.
        /// </returns>
        bool ValidatePassword(string password);
    }
}
