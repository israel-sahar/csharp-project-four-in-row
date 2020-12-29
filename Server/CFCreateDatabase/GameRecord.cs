using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFCreateDatabase
{
    public class GameRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GameRecordId { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Game Game { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public User User { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public DateTime RecordTime { get; set; }
        public int Column { get; set; }
    }
}
