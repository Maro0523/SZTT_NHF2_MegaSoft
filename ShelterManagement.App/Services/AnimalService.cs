using System;
using System.Collections.Generic;
using ShelterManagement.App.Models;

namespace ShelterManagement.App.Services
{
    public class AnimalService
    {
        private readonly ShelterData _data;

        public AnimalService(ShelterData data)
        {
            _data = data;
        }

        public List<Animal> GetAllAnimals()
        {
            return _data.Animals;
        }

        public Animal? GetAnimalById(int id)
        {
            foreach (var animal in _data.Animals)
            {
                if (animal.Id == id)
                {
                    return animal;
                }
            }
            return null;
        }

        public List<Animal> SearchAnimals(string? species, int? maxAge, string? gender, string? namePart, bool onlyAdoptable)
        {
            List<Animal> result = new List<Animal>();

            foreach (var a in _data.Animals)
            {
                if (onlyAdoptable && a.Status != "örökbefogadható")
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(species) && a.Species.ToLower() != species.Trim().ToLower())
                {
                    continue;
                }

                if (maxAge.HasValue && a.Age > maxAge.Value)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(gender) && a.Gender.ToLower() != gender.Trim().ToLower())
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(namePart) && !a.Name.ToLower().Contains(namePart.Trim().ToLower()))
                {
                    continue;
                }

                result.Add(a);
            }

            return result;
        }

        public Animal AddAnimal(string name, string species, string breed, int age, string gender, string intakeDate, string status, int? kennelId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new Exception("Az állat nevét meg kell adni!");
            }

            string cleanSpecies = species.Trim().ToLower();
            if (cleanSpecies != "kutya" && cleanSpecies != "macska")
            {
                throw new Exception("A faj csak 'kutya' vagy 'macska' lehet!");
            }

            if (string.IsNullOrWhiteSpace(breed))
            {
                throw new Exception("A fajta megadása kötelező!");
            }

            if (age < 0)
            {
                throw new Exception("Az életkor nem lehet negatív!");
            }

            string cleanGender = gender.Trim().ToLower();
            if (cleanGender != "kan" && cleanGender != "nőstény")
            {
                throw new Exception("A nem csak 'kan' vagy 'nőstény' lehet!");
            }

            if (!DateOnly.TryParse(intakeDate, out _))
            {
                throw new Exception("Érvénytelen bekerülési dátum formátum (ÉÉÉÉ-HH-NN)!");
            }

            ValidateStatus(status);

            if (status == "örökbefogadható" || status == "karanténban/kezelés alatt")
            {
                if (!kennelId.HasValue)
                {
                    throw new Exception("Aktív menhelyi státusz esetén kötelező kennelt megadni!");
                }
                ValidateKennelPlacement(kennelId.Value, cleanSpecies, null);
            }
            else
            {
                kennelId = null;
            }

            int maxId = 0;
            foreach (var a in _data.Animals)
            {
                if (a.Id > maxId)
                {
                    maxId = a.Id;
                }
            }

            Animal newAnimal = new Animal();
            newAnimal.Id = maxId + 1;
            newAnimal.Name = name.Trim();
            newAnimal.Species = cleanSpecies;
            newAnimal.Breed = breed.Trim();
            newAnimal.Age = age;
            newAnimal.Gender = cleanGender;
            newAnimal.IntakeDate = intakeDate.Trim();
            newAnimal.Status = status;
            newAnimal.KennelId = kennelId;

            if (status == "örökbe fogadott")
            {
                newAnimal.AdoptionDate = DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd");
            }

            _data.Animals.Add(newAnimal);
            return newAnimal;
        }

        public void UpdateAnimal(int animalId, string newName, string newBreed, int newAge, string newGender, string newStatus, int? newKennelId)
        {
            Animal? animal = GetAnimalById(animalId);
            if (animal == null)
            {
                throw new Exception($"Nem található állat ezzel az azonosítóval: {animalId}");
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new Exception("Az állat neve nem lehet üres!");
            }

            if (string.IsNullOrWhiteSpace(newBreed))
            {
                throw new Exception("A fajta nem lehet üres!");
            }

            if (newAge < 0)
            {
                throw new Exception("Az életkor nem lehet negatív!");
            }

            string cleanGender = newGender.Trim().ToLower();
            if (cleanGender != "kan" && cleanGender != "nőstény")
            {
                throw new Exception("A nem csak 'kan' vagy 'nőstény' lehet!");
            }

            ValidateStatus(newStatus);

            if (newStatus == "örökbefogadható" || newStatus == "karanténban/kezelés alatt")
            {
                if (!newKennelId.HasValue)
                {
                    throw new Exception("Menhelyen lévő állathoz kötelező kennelt megadni!");
                }
                ValidateKennelPlacement(newKennelId.Value, animal.Species, animal.Id);
            }
            else
            {
                newKennelId = null;
            }

            animal.Name = newName.Trim();
            animal.Breed = newBreed.Trim();
            animal.Age = newAge;
            animal.Gender = cleanGender;
            animal.Status = newStatus;
            animal.KennelId = newKennelId;

            if (newStatus == "örökbe fogadott" && string.IsNullOrWhiteSpace(animal.AdoptionDate))
            {
                animal.AdoptionDate = DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd");
            }
        }

        public HealthRecord AddHealthRecord(int animalId, string date, string type, string performedBy)
        {
            Animal? animal = GetAnimalById(animalId);
            if (animal == null)
            {
                throw new Exception($"Nem található állat ezzel az azonosítóval: {animalId}");
            }

            if (!DateOnly.TryParse(date, out _))
            {
                throw new Exception("Érvénytelen dátum formátum (ÉÉÉÉ-HH-NN)!");
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                throw new Exception("Az egészségügyi esemény típusa nem lehet üres!");
            }

            if (string.IsNullOrWhiteSpace(performedBy))
            {
                throw new Exception("Az eseményt elvégző személy neve kötelező!");
            }

            int maxId = 0;
            foreach (var r in _data.HealthRecords)
            {
                if (r.Id > maxId)
                {
                    maxId = r.Id;
                }
            }

            HealthRecord record = new HealthRecord();
            record.Id = maxId + 1;
            record.AnimalId = animalId;
            record.Date = date.Trim();
            record.Type = type.Trim();
            record.PerformedBy = performedBy.Trim();

            _data.HealthRecords.Add(record);
            return record;
        }

        public List<HealthRecord> GetHealthRecordsForAnimal(int animalId)
        {
            Animal? animal = GetAnimalById(animalId);
            if (animal == null)
            {
                throw new Exception($"Nem található állat ezzel az azonosítóval: {animalId}");
            }

            List<HealthRecord> records = new List<HealthRecord>();
            foreach (var r in _data.HealthRecords)
            {
                if (r.AnimalId == animalId)
                {
                    records.Add(r);
                }
            }
            return records;
        }

        public List<Kennel> GetAvailableKennels(string species)
        {
            List<Kennel> available = new List<Kennel>();
            foreach (var k in _data.Kennels)
            {
                if (k.Active && k.AnimalType.ToLower() == species.ToLower())
                {
                    int currentCount = CountAnimalsInKennel(k.Id, null);
                    if (currentCount < k.Capacity)
                    {
                        available.Add(k);
                    }
                }
            }
            return available;
        }

        public int CountAnimalsInKennel(int kennelId, int? excludeAnimalId)
        {
            int count = 0;
            foreach (var a in _data.Animals)
            {
                if (a.KennelId == kennelId && a.Id != excludeAnimalId)
                {
                    count++;
                }
            }
            return count;
        }

        public void ValidateKennelPlacement(int kennelId, string species, int? excludeAnimalId)
        {
            Kennel? kennel = null;
            foreach (var k in _data.Kennels)
            {
                if (k.Id == kennelId)
                {
                    kennel = k;
                    break;
                }
            }

            if (kennel == null)
            {
                throw new Exception($"Nem létező kennel azonosító: {kennelId}");
            }

            if (!kennel.Active)
            {
                throw new Exception("Inaktív kennelbe nem helyezhető el állat!");
            }

            if (kennel.AnimalType.ToLower() != species.ToLower())
            {
                throw new Exception($"A kennel csak '{kennel.AnimalType}' típusú állatok fogadására alkalmas!");
            }

            int occupied = CountAnimalsInKennel(kennelId, excludeAnimalId);
            if (occupied >= kennel.Capacity)
            {
                throw new Exception($"A kennel megtelt ({occupied}/{kennel.Capacity})!");
            }
        }

        private void ValidateStatus(string status)
        {
            if (status != "örökbefogadható" &&
                status != "karanténban/kezelés alatt" &&
                status != "próbaidőn" &&
                status != "örökbe fogadott")
            {
                throw new Exception("Érvénytelen állat státusz!");
            }
        }
    }
}