using WorshipDomain.Core.Interfaces;

namespace WorshipDomain.Core.Entities
{
    public abstract class ServiceBase<TKey, TEntity> : IServiceBase<TKey, TEntity>
    {
        protected readonly IGenericRepository<TKey, TEntity> _repository;

        public ServiceBase(IGenericRepository<TKey, TEntity> tEntityRepository)
        {
            _repository = tEntityRepository;
        }

        public TEntity Get(TKey key)
        {
            return _repository.Get(key);
        }

        public IEnumerable<TEntity> GetList(object? whereConditions)
        {
            return _repository.GetList(whereConditions);
        }

        public IEnumerable<TEntity> GetList(string conditions, object? parameters = null)
        {
            return _repository.GetList(conditions, parameters);
        }

        public IEnumerable<TEntity> GetList()
        {
            return _repository.GetList();
        }

        public TKey Insert(TEntity entity)
        {
            return _repository.Insert(entity);
        }

        public void Update(TEntity entity)
        {
            try
            {
                _repository.BeginTransaction();
                _repository.Update(entity);
                _repository.CommitTransaction();
            }
            catch
            {
                _repository.RollbackTransaction();
                throw;
            }
        }

        public int Delete(TKey key)
        {
            return _repository.Delete(key);
        }
    }
}
