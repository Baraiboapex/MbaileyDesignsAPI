namespace MattBaileyDesignsAPI.DbRepository.Interfaces
{
    public interface IDBRepository<T> where T : class
    {
        Task WriteToDb(T item);
        Task<T> GetSingleFromDb(int id);
        Task<List<T>> GetAllFromDb();
        Task EditOnDb(T Item);
        Task DeleteOnDb(T item, bool useHardDelete);
    }
}
