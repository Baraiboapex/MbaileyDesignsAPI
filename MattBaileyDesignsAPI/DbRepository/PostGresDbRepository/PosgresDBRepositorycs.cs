using MattBaileyDesignsAPI.Controllers;
using MattBaileyDesignsAPI.DbRepository.Interfaces;
using MBaileyDesignsDomain;
using MbaileyDesignsPersistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Npgsql;
using System.Text.Json;

namespace MattBaileyDesignsAPI.DbRepository
{
    public class PosgresDBRepository<TEntity> : IDBRepository<TEntity> where TEntity : class
    {
        private readonly PostgresDataContext _db;
        private IConfiguration _config;
        private readonly DbSet<TEntity> _currentTable;

        public PosgresDBRepository(PostgresDataContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
            _currentTable = _db.Set<TEntity>();
        }

        public PostgresDataContext GetContext() {
            return _db;
        }

        public async Task DeleteOnDb(TEntity item, bool useHardDelete)
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

        public async Task EditOnDb(TEntity item)
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

        public async Task<List<TEntity>> GetAllFromDb()
        {
            try
            {
                return await _currentTable.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error Could Not Get All Data : " + ex);
            }
        }

        public async Task<TEntity> GetSingleFromDb(int id)
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
            catch (Exception ex)
            {
                throw new Exception("Error Could Not Get Single Data Object : " + ex);
            }
        }

        public async Task WriteToDb(TEntity item)
        {
            try
            {
                await _currentTable.AddAsync(item);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error Could Not Add New Object : " + ex);
            }
        }

        public async Task<List<OutboundDTO>> GetAllFromDBFromSearchQuery(string storeProcName, Dictionary<string, object> queryItems)
        {
            try
            {
                string jsonString = System.Text.Json.JsonSerializer.Serialize(queryItems);

                if (!string.IsNullOrEmpty(jsonString) && _config != null)
                {
                    string connectionString = _config.GetConnectionString("DefaultConnection");
                    bool connectionStringIsNotNull = !string.IsNullOrEmpty(connectionString);

                    if (connectionStringIsNotNull)
                    {
                        var results = new List<OutboundDTO>();

                        await using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                        {
                            await connection.OpenAsync();

                            await using (NpgsqlCommand command = new NpgsqlCommand($"SELECT * FROM {storeProcName}(@jsonData::jsonb)", connection))
                            {
                                try
                                {
                                    var type = NpgsqlTypes.NpgsqlDbType.Jsonb;

                                    command.Parameters.AddWithValue("@jsonData", type, jsonString ?? (object)DBNull.Value);

                                    await using (var reader = command.ExecuteReader())
                                    {
                                        while (await reader.ReadAsync())
                                        {
                                            var retrievedJson = reader.GetString(0);
                                            var itemDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(retrievedJson);
                                            
                                            if (itemDictionary != null)
                                            {
                                                var newOutboundItem = new OutboundDTO(itemDictionary);
                                                results.Add(newOutboundItem);
                                            }
                                        }

                                        return results;
                                    }
                                }
                                catch(Exception ex)
                                {
                                    throw new Exception("Error: " + ex);
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Error: Connection string not found");
                    }
                }
                else
                {
                    throw new Exception("Query items not provided");
                }
            }
            catch (Exception ex)
            {
                // Consider logging the exception here
                Console.WriteLine(ex);
                throw new Exception("Error: Could Not Get All Data", ex);
            }
        }
    }
}
