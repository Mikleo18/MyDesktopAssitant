using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDeskopAssitant.Models
{
    public  class ToDoModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public bool IsCompleted { get; set; }
        public string TaskDate { get; set; } // Tarih (string tutmak daha kolay listelemek için)
        public string TaskTime { get; set; } // Saat
    }
}
