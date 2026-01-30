using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToyShelf.Domain.IRepositories;
using ToyShelf.Infrastructure.Context;

namespace ToyShelf.Infrastructure.Repositories
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly ToyCabinDbContext _context;
		private Hashtable _repositories;

		public UnitOfWork(ToyCabinDbContext context)
		{
			_context = context;
			_repositories = new Hashtable();
		}

		public IGenericRepository<T> Repository<T>() where T : class
		{
			var type = typeof(T).Name;

			if (!_repositories.ContainsKey(type))
			{
				var repositoryType = typeof(GenericRepository<>);
				var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _context);
				_repositories.Add(type, repositoryInstance);
			}

			return (IGenericRepository<T>)_repositories[type]!;
		}

		public async Task<int> SaveChangesAsync()
		{
			return await _context.SaveChangesAsync();
		}

		public void Dispose()
		{
			_context.Dispose();
		}
	}
}
