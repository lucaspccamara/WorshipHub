using Dapper;
using System.Data;
using WorshipDomain.Core.Interfaces;
using WorshipInfra.Database.Interfaces;

namespace WorshipInfra.Database
{
    public abstract class Repository
    {
        private readonly IContextRepository _context;

        protected IDbConnection _dbConnection => _context.Connection;
        protected IDbTransaction _dbTransaction => _context.Transaction;


        protected Repository(IContextRepository context)
        {
            _context = context;
        }

        public void BeginTransaction()
        {
            _context.BeginTransaction();
        }

        public void CommitTransaction()
        {
            _context.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            _context.RollbackTransaction();
        }
    }

    public abstract class GenericRepository<TKey, TEntity> : Repository, IGenericRepository<TKey, TEntity>
    {
        protected GenericRepository(IContextRepository context) : base(context)
        {
        }

        public virtual TEntity Get(TKey key)
        {
            return _dbConnection.Get<TEntity>(key, _dbTransaction);
        }

        public virtual IEnumerable<TEntity> GetList(object? whereConditions)
        {
            return _dbConnection.GetList<TEntity>(whereConditions, _dbTransaction);
        }

        public virtual IEnumerable<TEntity> GetList(string conditions, object? parameters = null)
        {
            return _dbConnection.GetList<TEntity>(conditions, parameters, _dbTransaction);
        }

        public virtual IEnumerable<TEntity> GetList()
        {
            return _dbConnection.GetList<TEntity>();
        }

        public virtual TKey Insert(TEntity entity)
        {
            return _dbConnection.Insert<TKey, TEntity>(entity, _dbTransaction);
        }

        public virtual void Update(TEntity entity)
        {
            _dbConnection.Update(entity, _dbTransaction);
        }

        public virtual int Delete(TKey key)
        {
            return _dbConnection.Delete(key, _dbTransaction);
        }
    }
}
