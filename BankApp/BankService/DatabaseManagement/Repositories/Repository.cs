using Common.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace BankService.DatabaseManagement.Repositories
{
	public class Repository<TEntity> : IRepository<TEntity>
		where TEntity : IdentifiedObject
	{
		protected readonly DbContext dbContext;

		public Repository(DbContext dbContext)
		{
			this.dbContext = dbContext;
		}

		public void AddEntity(TEntity entity)
		{
			if (dbContext.Database.Connection.State == System.Data.ConnectionState.Closed)
			{
				return;
			}

			dbContext.Set<TEntity>().Add(entity);
			dbContext.SaveChanges();
		}

		public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
		{
			if (dbContext.Database.Connection.State == System.Data.ConnectionState.Closed)
			{
				return new List<TEntity>(0);
			}

			return dbContext.Set<TEntity>().Where(predicate).ToList();
		}

		public TEntity Get(long id)
		{
			if (dbContext.Database.Connection.State == System.Data.ConnectionState.Closed)
			{
				return null;
			}

			return dbContext.Set<TEntity>().FirstOrDefault(x => x.ID == id);
		}

		public List<TEntity> ReadAllEntities()
		{
			if (dbContext.Database.Connection.State == System.Data.ConnectionState.Closed)
			{
				return new List<TEntity>(0);
			}

			return dbContext.Set<TEntity>().ToList();
		}

		public void RemoveEntity(long entityId)
		{
			if (dbContext.Database.Connection.State == System.Data.ConnectionState.Closed)
			{
				return;
			}

			dbContext.Set<TEntity>().Remove(Get(entityId));
			dbContext.SaveChanges();
		}

		public void Update(TEntity entity)
		{
			if (dbContext.Database.Connection.State == System.Data.ConnectionState.Closed)
			{
				return;
			}

			dbContext.Set<TEntity>().Attach(entity);
			dbContext.Entry(entity).State = EntityState.Modified;
			dbContext.SaveChanges();
		}
	}
}
