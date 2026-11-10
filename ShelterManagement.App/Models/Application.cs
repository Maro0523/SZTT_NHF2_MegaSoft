using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterManagement.App.Models
{
    public class Application
    {
        public int Id { get; set; }
        public int AnimalId { get; set; }
        public int AdopterId { get; set; }
        public string ApplicationDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
