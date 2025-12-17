using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Infrastructure.Security
{
	public static class PasswordHasher
	{
		public static string GenerateSalt()
		{
			var bytes = RandomNumberGenerator.GetBytes(16);
			return Convert.ToBase64String(bytes);
		}

		public static string Hash(string password, string salt)
		{
			return BCrypt.Net.BCrypt.HashPassword(password + salt);
		}

		public static bool Verify(string password, string salt, string hash)
		{
			return BCrypt.Net.BCrypt.Verify(password + salt, hash);
		}
	}

}
