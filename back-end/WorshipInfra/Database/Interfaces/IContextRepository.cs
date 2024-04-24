using System.Data;

namespace WorshipInfra.Database.Interfaces
{
    public interface IContextRepository
    {
        IDbTransaction Transaction { get; }
        IDbConnection Connection { get; }

        void BeginTransaction();
        void CommitTransaction();
        void RollbackTransaction();
    }
}
