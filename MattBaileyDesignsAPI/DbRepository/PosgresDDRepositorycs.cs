using MattBaileyDesignsAPI.DbRepository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MattBaileyDesignsAPI.DbRepository
{
    public class PosgresDBRepository<T> : IDBRepository<T> where T : class
    {
        private readonly DbContext _db;
        private readonly DbSet<T> _currentTable;

        public PosgresDBRepository(DbContext db) {
            _db = db;
            _currentTable = _db.Set<T>();
        }

        public async Task DeleteOnDb(T item, bool useHardDelete)
        {
            try
            {
                if (useHardDelete)
                {
                    _currentTable.Attach(item);
                    _db.Entry(item).State = EntityState.Modified;
                }
                else
                {
                    _currentTable.Remove(item);
                }

                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error Could Not Delete Data : " + ex);
            }
        }

        public async Task EditOnDb(T item)
        {
            try
            {
                _currentTable.Attach(item);
                _db.Entry(item).State = EntityState.Modified;
                await _db.SaveChangesAsync();
            }
            catch (Exception ex) 
            {
                throw new Exception("Error Could Not Edit Data : " + ex);
            }
        }

        public async Task<List<T>> GetAllFromDb()
        {
            try
            {
                return await _currentTable.ToListAsync();
            }
            catch(Exception ex)
            {
                throw new Exception("Error Could Not Get All Data : " + ex);
            }
        }

        public async Task<T> GetSingleFromDb(int id)
        {
            try
            {
                var foundItem = await _currentTable.FindAsync(id);

                bool itemExists = foundItem != null;

                if (itemExists)
                {
                    return foundItem;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            catch(Exception ex)
            {
                throw new Exception("Error Could Not Get Single Data Object : " + ex);
            }
        }

        public async Task WriteToDb(T item)
        {
            try
            {
                await _currentTable.AddAsync(item); ;
                await _db.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                throw new Exception("Error Could Not Add New Object : " + ex);
            }
        }
    }
}
