using MattBaileyDesignsAPI.Controllers;
using MbaileyDesignsPersistence;

namespace MattBaileyDesignsAPI.DbRepository.Interfaces
{
    public interface IDBRepository<TEntity> where TEntity : class
    {
        PostgresDataContext GetContext();
        Task WriteToDb(TEntity item);
        Task<TEntity> GetSingleFromDb(int id);
        Task<List<TEntity>> GetAllFromDb();
        Task EditOnDb(TEntity Item);
        Task DeleteOnDb(TEntity item, bool useHardDelete);
        Task<List<OutboundDTO>> GetAllFromDBFromSearchQuery(string storeProcName, Dictionary<string, object> queryItems);
    }
}
