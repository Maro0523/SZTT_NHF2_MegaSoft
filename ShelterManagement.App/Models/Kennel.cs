using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterManagement.App.Models
{
    public class Kennel
    {
        public int Id { get; set; }
        public int SectorId { get; set; }
        public string AnimalType { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public bool Active { get; set; }
    }
}
