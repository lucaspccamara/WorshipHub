namespace WorshipDomain.Core.Interfaces
{
    public interface IGenericRepository<TKey, TEntity>
    {
        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();

        TEntity Get(TKey key);
        IEnumerable<TEntity> GetList(object? whereConditions);
        IEnumerable<TEntity> GetList(string conditions, object? parameters = null);
        IEnumerable<TEntity> GetList();
        TKey Insert(TEntity entity);
        void Update(TEntity entity);
        int Delete(TKey key);
    }
}
