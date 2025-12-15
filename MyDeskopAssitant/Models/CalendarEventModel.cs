using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDeskopAssitant.Models
{
    public class CalendarEventModel
    {
        public string Title { get; set; }    
        public DateTime Date { get; set; }    
        public bool IsPast => Date.Date < DateTime.Now.Date;
    }
}
