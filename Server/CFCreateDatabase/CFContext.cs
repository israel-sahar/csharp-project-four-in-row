using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFCreateDatabase
{
    public class CFContext: DbContext
    {
        public CFContext(string databaseName) : base(databaseName) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<GameRecord> GameRecords { get; set; }

    }
}
