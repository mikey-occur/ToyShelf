using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
		private readonly ToyShelfDbContext _context;
		private Hashtable _repositories;
		private IDbContextTransaction? _currentTransaction;
		public UnitOfWork(ToyShelfDbContext context, IDbContextTransaction? currentTransaction = null)
		{
			_context = context;
			_repositories = new Hashtable();
			_currentTransaction = currentTransaction;
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

		//public async Task<int> SaveChangesAsync()
		//{
		//	return await _context.SaveChangesAsync();
		//}

		public void Dispose()
		{
			_context.Dispose();
		}

		public async Task<int> SaveChangesAsync()
		{
			try
			{
				return await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException )
			{
				// Log lỗi tại đây hoặc quăng ra một Custom Exception của riêng bạn
				throw new Exception("Dữ liệu đã bị thay đổi bởi người khác, vui lòng thử lại.");
			}
		}

		public async Task BeginTransactionAsync()
		{
			if (_currentTransaction != null)
			{
				return;
			}

			_currentTransaction = await _context.Database.BeginTransactionAsync();
		}

		public async Task CommitTransactionAsync()
		{
			try
			{
				
				if (_currentTransaction != null)
				{
					await _currentTransaction.CommitAsync();
				}
			}
			finally
			{
				if (_currentTransaction != null)
				{
					await _currentTransaction.DisposeAsync();
					_currentTransaction = null;
				}
			}
		}

		public async Task RollbackTransactionAsync()
		{
			try
			{
				if (_currentTransaction != null)
				{
					await _currentTransaction.RollbackAsync();
				}
			}
			finally
			{
				if (_currentTransaction != null)
				{
					await _currentTransaction.DisposeAsync();
					_currentTransaction = null;
				}
			}
		}
	}
}
