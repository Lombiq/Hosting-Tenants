using System;
using System.Security.Cryptography;
using System.Text;

namespace Lombiq.Hosting.Tenants.Admin.Login.Services
{
    public class TenantLoginKeyValidator : ITenantLoginPasswordValidator
    {
        private readonly Lazy<string> _lazyPassword = new(() => GeneratePassword(50));

        public string Password => _lazyPassword.Value;

        public bool ValidatePassword(string password) => string.Equals(Password, password, StringComparison.Ordinal);

        private static string GeneratePassword(int length)
        {
            const string validCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!#";
            var stringBuilder = new StringBuilder();
            using var random = RandomNumberGenerator.Create();
            byte[] uintBuffer = new byte[sizeof(uint)];

            while (length > 0)
            {
                length--;
                random.GetBytes(uintBuffer);
                uint randomNumber = BitConverter.ToUInt32(uintBuffer, 0);
                stringBuilder.Append(validCharacters[(int)(randomNumber % (uint)validCharacters.Length)]);
            }

            return stringBuilder.ToString();
        }
    }
}
