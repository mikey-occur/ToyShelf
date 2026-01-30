using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyShelf.Application.Security
{
	public interface IPasswordHasher
	{
		string GenerateSalt();
		string Hash(string password, string salt);
		bool Verify(string password, string salt, string hash);
	}
}
