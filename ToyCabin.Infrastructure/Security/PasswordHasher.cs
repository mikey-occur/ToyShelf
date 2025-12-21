using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Application.Security;

namespace ToyCabin.Infrastructure.Security
{
	public class PasswordHasher : IPasswordHasher
	{
		public string GenerateSalt()
		{
			var bytes = RandomNumberGenerator.GetBytes(16);
			return Convert.ToBase64String(bytes);
		}

		public string Hash(string password, string salt)
		{
			return BCrypt.Net.BCrypt.HashPassword(password + salt);
		}

		public bool Verify(string password, string salt, string hash)
		{
			return BCrypt.Net.BCrypt.Verify(password + salt, hash);
		}
	}
}
