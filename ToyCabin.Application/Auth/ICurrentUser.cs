using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyCabin.Application.Auth
{
	public interface ICurrentUser
	{
		Guid AccountId { get; }
		Guid UserId { get; }
		Guid? PartnerId { get; }
		IReadOnlyList<string> Roles { get; }

		bool IsPartnerAdmin();
	}
}
