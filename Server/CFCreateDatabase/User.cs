using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace CFCreateDatabase
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string UserName { get; set; }
        public string HashedPassword { get; set; }
        public int GameLoses { get; set; }
        public int GameWins { get; set; }
        public int GameDraws { get; set; }
        public int Score { get; set; }
        public ICollection<Game> Games { get; set; }


    }
}
