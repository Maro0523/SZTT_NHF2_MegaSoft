using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterManagement.App.Models
{
    public class Animal
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty;
        public string Breed { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string IntakeDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? KennelId { get; set; }
        public string? AdoptionDate { get; set; }
    }
}
