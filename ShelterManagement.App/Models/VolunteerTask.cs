using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterManagement.App.Models
{
    public class VolunteerTask
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public int? AnimalId { get; set; }
        public int RequiredVolunteers { get; set; }
        public List<int> AssignedVolunteerIds { get; set; } = new();
    }
}
