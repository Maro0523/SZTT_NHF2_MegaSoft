using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterManagement.App.Models
{
    public class ShelterData
    {
        public List<Sector> Sectors { get; set; } = new();
        public List<Kennel> Kennels { get; set; } = new();
        public List<Caretaker> Caretakers { get; set; } = new();
        public List<Animal> Animals { get; set; } = new();
        public List<HealthRecord> HealthRecords { get; set; } = new();
        public List<Adopter> Adopters { get; set; } = new();
        public List<Application> Applications { get; set; } = new();
        public List<Volunteer> Volunteers { get; set; } = new();
        public List<VolunteerTask> Tasks { get; set; } = new();
    }
}
