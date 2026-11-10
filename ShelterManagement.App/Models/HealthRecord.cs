using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterManagement.App.Models
{
    public class HealthRecord
    {
        public int Id { get; set; }
        public int AnimalId { get; set; }
        public string Date { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string PerformedBy { get; set; } = string.Empty;
    }
}
