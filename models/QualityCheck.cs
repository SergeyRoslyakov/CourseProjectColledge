using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QualityAppWPF.Models
{
    public class QualityCheck
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductBarcode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public DateTime CheckDate { get; set; } = DateTime.Now;
        public int Rating { get; set; } = 3; // 1-5
        public string Comment { get; set; } = string.Empty;
        public string Inspector { get; set; } = "Оператор";
        public string Location { get; set; } = string.Empty;
    }
}
