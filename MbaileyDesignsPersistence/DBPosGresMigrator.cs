using MbaileyDesignsPersistence.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MbaileyDesignsPersistence
{

    public class DBPosGresMigrator : IDBMigrator
    {
        private readonly DbContext _db;

        public DBPosGresMigrator(DbContext context) {
            _db = context;
        }

        public void MigrateDB()
        {
            throw new NotImplementedException();
        }
    }
}
