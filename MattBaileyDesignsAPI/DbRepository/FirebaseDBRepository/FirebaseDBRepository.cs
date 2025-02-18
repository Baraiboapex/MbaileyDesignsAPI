using MattBaileyDesignsAPI.Controllers;
using MattBaileyDesignsAPI.DbRepository.Interfaces;
using MbaileyDesignsPersistence;

namespace MattBaileyDesignsAPI.DbRepository.FirebaseDBRepository
{
    public class FirebaseDBRepository<TEntity> : IDBRepository<TEntity> where TEntity : class
    {
        public Task DeleteOnDb(TEntity item, bool useHardDelete)
        {
            throw new NotImplementedException();
        }

        public Task EditOnDb(TEntity Item)
        {
            throw new NotImplementedException();
        }

        public Task<List<TEntity>> GetAllFromDb()
        {
            throw new NotImplementedException();
        }

        public Task<List<OutboundDTO>> GetAllFromDBFromSearchQuery(string storeProcName, Dictionary<string, object> queryItems)
        {
            throw new NotImplementedException();
        }

        public PostgresDataContext GetContext()
        {
            throw new NotImplementedException();
        }

        public Task<TEntity> GetSingleFromDb(int id)
        {
            throw new NotImplementedException();
        }

        public Task WriteToDb(TEntity item)
        {
            throw new NotImplementedException();
        }
    }
}
