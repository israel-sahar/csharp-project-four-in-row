using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFCreateDatabase
{
    public class Game
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GameId { get; set; }
        public DateTime StartTime { get; set; }
        public Nullable<DateTime> EndTime { get; set; }
        public ICollection<GameRecord> GameRecords { get; set; }
        public string WinnerName { get; set; }
        public int WinnerScore { get; set; }
        public string LoserName { get; set; }
        public int LoserScore { get; set; }
        public Nullable<bool> IsDraw { get; set; }
        public ICollection<User> Players { get; set; }
    }
}
