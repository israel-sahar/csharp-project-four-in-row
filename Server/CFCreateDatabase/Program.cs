using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFCreateDatabase
{
    public class Program
    {
        static void Main(string[] args)
        {
            string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog= FourInRowDB;AttachDbFilename=C:\fourinrow\databases\fourinrowDB_Aviv_Sahar.mdf;Integrated Security=True";
            using (var ctx = new CFContext(connectionString)) {
                ctx.Database.Initialize(force: true);
                Console.WriteLine("database created..");
                Console.ReadLine();
            }
        }
    }
}
