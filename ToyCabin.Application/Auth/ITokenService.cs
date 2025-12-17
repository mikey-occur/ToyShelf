using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyCabin.Domain.Entities;

namespace ToyCabin.Application.Auth
{
	public interface ITokenService
	{
		string GenerateAccessToken(Account account);
	}

}
