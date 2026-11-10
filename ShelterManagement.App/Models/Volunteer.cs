using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterManagement.App.Models
{
    public class Volunteer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<string> AvailableDays { get; set; } = new();
        public List<string> Skills { get; set; } = new();
    }
}
