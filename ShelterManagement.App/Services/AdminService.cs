using System;
using System.Collections.Generic;
using ShelterManagement.App.Models;

namespace ShelterManagement.App.Services
{
    public class AdminService
    {
        private readonly ShelterData _data;

        public AdminService(ShelterData data)
        {
            _data = data;
        }

        // --- Szektorok kezelese ---

        public List<Sector> GetAllSectors()
        {
            return _data.Sectors;
        }

        public Sector CreateSector(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("A szektor neve nem lehet üres!");
            }

            int maxId = 0;
            foreach (var s in _data.Sectors)
            {
                if (s.Id > maxId)
                {
                    maxId = s.Id;
                }
            }

            Sector newSector = new Sector();
            newSector.Id = maxId + 1;
            newSector.Name = name.Trim();
            newSector.Active = true;

            _data.Sectors.Add(newSector);
            return newSector;
        }

        public void UpdateSector(int sectorId, string newName)
        {
            Sector? sector = FindSectorById(sectorId);
            if (sector == null)
            {
                throw new KeyNotFoundException($"Nem található szektor ezzel az azonosítóval: {sectorId}");
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                throw new ArgumentException("A szektor neve nem lehet üres!");
            }

            sector.Name = newName.Trim();
        }

        public void InactivateSector(int sectorId)
        {
            Sector? sector = FindSectorById(sectorId);
            if (sector == null)
            {
                throw new KeyNotFoundException($"Nem található szektor ezzel az azonosítóval: {sectorId}");
            }

            if (!sector.Active)
            {
                throw new InvalidOperationException("A szektor már inaktív.");
            }

            // Szektor csak akkor inaktivalhato, ha nincs benne aktiv kennel
            foreach (var kennel in _data.Kennels)
            {
                if (kennel.SectorId == sectorId && kennel.Active)
                {
                    throw new InvalidOperationException("A szektor nem inaktiválható, mert még van benne aktív kennel!");
                }
            }

            sector.Active = false;
        }

        public void ActivateSector(int sectorId)
        {
            Sector? sector = FindSectorById(sectorId);
            if (sector == null)
            {
                throw new KeyNotFoundException($"Nem található szektor ezzel az azonosítóval: {sectorId}");
            }

            sector.Active = true;
        }

        // --- Kennelek kezelese ---

        public List<Kennel> GetAllKennels()
        {
            return _data.Kennels;
        }

        public Kennel CreateKennel(int sectorId, string animalType, int capacity)
        {
            Sector? sector = FindSectorById(sectorId);
            if (sector == null)
            {
                throw new KeyNotFoundException($"Nem létező szektor azonosító: {sectorId}");
            }

            if (!sector.Active)
            {
                throw new InvalidOperationException("Inaktív szektorba nem hozható létre új kennel!");
            }

            // Egy szektorban max 10 kennel lehet
            int countInSector = 0;
            foreach (var k in _data.Kennels)
            {
                if (k.SectorId == sectorId)
                {
                    countInSector++;
                }
            }

            if (countInSector >= 10)
            {
                throw new InvalidOperationException("Egy szektorban legfeljebb 10 kennel lehet!");
            }

            string type = animalType.Trim().ToLower();
            if (type != "kutya" && type != "macska")
            {
                throw new ArgumentException("A kennel típusa csak 'kutya' vagy 'macska' lehet!");
            }

            if (capacity <= 0)
            {
                throw new ArgumentException("A befogadóképességnek 0-nál nagyobbnak kell lennie!");
            }

            int maxId = 100;
            foreach (var k in _data.Kennels)
            {
                if (k.Id > maxId)
                {
                    maxId = k.Id;
                }
            }

            Kennel newKennel = new Kennel();
            newKennel.Id = maxId + 1;
            newKennel.SectorId = sectorId;
            newKennel.AnimalType = type;
            newKennel.Capacity = capacity;
            newKennel.Active = true;

            _data.Kennels.Add(newKennel);
            return newKennel;
        }

        public void UpdateKennel(int kennelId, string newAnimalType, int newCapacity)
        {
            Kennel? kennel = FindKennelById(kennelId);
            if (kennel == null)
            {
                throw new KeyNotFoundException($"Nem található kennel ezzel az azonosítóval: {kennelId}");
            }

            string type = newAnimalType.Trim().ToLower();
            if (type != "kutya" && type != "macska")
            {
                throw new ArgumentException("A kennel típusa csak 'kutya' vagy 'macska' lehet!");
            }

            if (newCapacity <= 0)
            {
                throw new ArgumentException("A befogadóképességnek 0-nál nagyobbnak kell lennie!");
            }

            int animalsCount = CountAnimalsInKennel(kennelId);
            if (newCapacity < animalsCount)
            {
                throw new InvalidOperationException($"A kapacitás nem lehet kisebb a jelenleg benne lévő állatok számánál ({animalsCount} db)!");
            }

            if (animalsCount > 0 && kennel.AnimalType.ToLower() != type)
            {
                throw new InvalidOperationException("A kennel állattípusa nem módosítható, amíg állatok vannak benne!");
            }

            kennel.AnimalType = type;
            kennel.Capacity = newCapacity;
        }

        public void InactivateKennel(int kennelId)
        {
            Kennel? kennel = FindKennelById(kennelId);
            if (kennel == null)
            {
                throw new KeyNotFoundException($"Nem található kennel ezzel az azonosítóval: {kennelId}");
            }

            if (!kennel.Active)
            {
                throw new InvalidOperationException("A kennel már inaktív.");
            }

            if (CountAnimalsInKennel(kennelId) > 0)
            {
                throw new InvalidOperationException("A kennel nem inaktiválható, mert jelenleg állatok vannak benne!");
            }

            kennel.Active = false;
        }

        public void ActivateKennel(int kennelId)
        {
            Kennel? kennel = FindKennelById(kennelId);
            if (kennel == null)
            {
                throw new KeyNotFoundException($"Nem található kennel ezzel az azonosítóval: {kennelId}");
            }

            Sector? sector = FindSectorById(kennel.SectorId);
            if (sector == null || !sector.Active)
            {
                throw new InvalidOperationException("A kennel nem aktiválható, mert a szektora inaktív!");
            }

            kennel.Active = true;
        }

        // --- Segedmetodusok ---

        private Sector? FindSectorById(int id)
        {
            foreach (var s in _data.Sectors)
            {
                if (s.Id == id)
                {
                    return s;
                }
            }
            return null;
        }

        private Kennel? FindKennelById(int id)
        {
            foreach (var k in _data.Kennels)
            {
                if (k.Id == id)
                {
                    return k;
                }
            }
            return null;
        }

        private int CountAnimalsInKennel(int kennelId)
        {
            int count = 0;
            foreach (var a in _data.Animals)
            {
                if (a.KennelId == kennelId)
                {
                    count++;
                }
            }
            return count;
        }
    }
}