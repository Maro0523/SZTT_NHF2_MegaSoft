using System;
using System.Collections.Generic;
using ShelterManagement.App.Models;
using ShelterManagement.App.Services;

namespace ShelterManagement.App.UI
{
    public class AdminMenu
    {
        private readonly AdminService _adminService;

        public AdminMenu(ShelterData data)
        {
            _adminService = new AdminService(data);
        }

        public void Run()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("--- ADMINISZTRÁTORI MENÜ ---");
                Console.WriteLine("1. Szektorok és kennelek kezelése");
                Console.WriteLine("2. Menhely működési statisztikák");
                Console.WriteLine("3. Legaktívabb önkéntesek listája");
                Console.WriteLine("0. Vissza a főmenübe");
                Console.Write("\nVálassz menüpontot: ");

                string? choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        RunSectorAndKennelMenu();
                        break;
                    case "2":
                        ShowAnimalStatistics();
                        break;
                    case "3":
                        ShowMostActiveVolunteers();
                        break;
                    case "0":
                        return;
                    default:
                        ShowError("Érvénytelen választás!");
                        break;
                }
            }
        }

        private void RunSectorAndKennelMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("--- SZEKTOROK ÉS KENNELEK KEZELÉSE ---");
                Console.WriteLine("1. Szektorok és kennelek listázása");
                Console.WriteLine("2. Új szektor létrehozása");
                Console.WriteLine("3. Szektor nevének módosítása");
                Console.WriteLine("4. Szektor inaktiválása / aktiválása");
                Console.WriteLine("5. Új kennel létrehozása");
                Console.WriteLine("6. Kennel módosítása");
                Console.WriteLine("7. Kennel inaktiválása / aktiválása");
                Console.WriteLine("0. Vissza");
                Console.Write("\nVálassz: ");

                string? choice = Console.ReadLine();
                try
                {
                    if (choice == "1")
                    {
                        ListSectorsAndKennels(true);
                    }
                    else if (choice == "2")
                    {
                        Console.Write("Új szektor neve: ");
                        string name = Console.ReadLine() ?? "";
                        Sector s = _adminService.CreateSector(name);
                        ShowSuccess($"Szektor létrehozva: [{s.Id}] {s.Name}");
                    }
                    else if (choice == "3")
                    {
                        ListSectorsAndKennels(false);
                        Console.Write("\nMódosítandó szektor ID: ");
                        int id = int.Parse(Console.ReadLine() ?? "");
                        Console.Write("Új név: ");
                        string newName = Console.ReadLine() ?? "";
                        _adminService.UpdateSector(id, newName);
                        ShowSuccess("Szektor sikeresen módosítva!");
                    }
                    else if (choice == "4")
                    {
                        ListSectorsAndKennels(false);
                        Console.Write("\nSzektor ID: ");
                        int id = int.Parse(Console.ReadLine() ?? "");
                        Console.Write("Művelet (1 = Inaktiválás, 2 = Aktiválás): ");
                        string? op = Console.ReadLine();

                        if (op == "1") _adminService.InactivateSector(id);
                        else if (op == "2") _adminService.ActivateSector(id);
                        else throw new ArgumentException("Érvénytelen művelet!");

                        ShowSuccess("Szektor státusza frissítve!");
                    }
                    else if (choice == "5")
                    {
                        ListSectorsAndKennels(false);
                        Console.Write("\nSzektor ID: ");
                        int sectorId = int.Parse(Console.ReadLine() ?? "");
                        Console.Write("Állattípus (kutya / macska): ");
                        string type = Console.ReadLine() ?? "";
                        Console.Write("Befogadóképesség: ");
                        int capacity = int.Parse(Console.ReadLine() ?? "");

                        Kennel k = _adminService.CreateKennel(sectorId, type, capacity);
                        ShowSuccess($"Kennel létrehozva: [{k.Id}] ({k.AnimalType}, kapacitás: {k.Capacity})");
                    }
                    else if (choice == "6")
                    {
                        ListSectorsAndKennels(false);
                        Console.Write("\nMódosítandó kennel ID: ");
                        int kennelId = int.Parse(Console.ReadLine() ?? "");
                        Console.Write("Új állattípus (kutya / macska): ");
                        string newType = Console.ReadLine() ?? "";
                        Console.Write("Új befogadóképesség: ");
                        int newCap = int.Parse(Console.ReadLine() ?? "");

                        _adminService.UpdateKennel(kennelId, newType, newCap);
                        ShowSuccess("Kennel sikeresen módosítva!");
                    }
                    else if (choice == "7")
                    {
                        ListSectorsAndKennels(false);
                        Console.Write("\nKennel ID: ");
                        int kennelId = int.Parse(Console.ReadLine() ?? "");
                        Console.Write("Művelet (1 = Inaktiválás, 2 = Aktiválás): ");
                        string? op = Console.ReadLine();

                        if (op == "1") _adminService.InactivateKennel(kennelId);
                        else if (op == "2") _adminService.ActivateKennel(kennelId);
                        else throw new ArgumentException("Érvénytelen művelet!");

                        ShowSuccess("Kennel státusza frissítve!");
                    }
                    else if (choice == "0")
                    {
                        return;
                    }
                    else
                    {
                        ShowError("Érvénytelen választás!");
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Hiba: {ex.Message}");
                }
            }
        }

        private void ListSectorsAndKennels(bool pause)
        {
            Console.WriteLine("\n--- SZEKTOROK ÉS KENNELEK ---");
            List<Sector> sectors = _adminService.GetAllSectors();
            List<Kennel> kennels = _adminService.GetAllKennels();

            foreach (var s in sectors)
            {
                string status = s.Active ? "Aktív" : "Inaktív";
                Console.WriteLine($"Szektor [{s.Id}] {s.Name} ({status})");

                bool hasKennel = false;
                foreach (var k in kennels)
                {
                    if (k.SectorId == s.Id)
                    {
                        hasKennel = true;
                        string kStatus = k.Active ? "Aktív" : "Inaktív";
                        Console.WriteLine($"   -> Kennel [{k.Id}] Típus: {k.AnimalType}, Kapacitás: {k.Capacity}, Státusz: {kStatus}");
                    }
                }

                if (!hasKennel)
                {
                    Console.WriteLine("   (Nincs kennel ebben a szektorban)");
                }
            }

            if (pause)
            {
                Pause();
            }
        }

        private void ShowAnimalStatistics()
        {
            Console.Clear();
            Console.WriteLine("--- MENHELY MŰKÖDÉSI STATISZTIKÁK ---");
            try
            {
                Console.Write("Kezdő dátum (ÉÉÉÉ-HH-NN): ");
                if (!DateOnly.TryParse(Console.ReadLine(), out DateOnly start))
                {
                    throw new FormatException("Érvénytelen kezdő dátum formátum!");
                }

                Console.Write("Végdátum (ÉÉÉÉ-HH-NN): ");
                if (!DateOnly.TryParse(Console.ReadLine(), out DateOnly end))
                {
                    throw new FormatException("Érvénytelen végdátum formátum!");
                }

                AnimalStatistics stats = _adminService.GetAnimalStatistics(start, end);

                Console.WriteLine($"\nIdőszak: {start:yyyy-MM-dd} - {end:yyyy-MM-dd}");
                Console.WriteLine("\nBekerült állatok száma fajonként:");
                foreach (var item in stats.IntakeBySpecies)
                {
                    Console.WriteLine($"  - {item.Key}: {item.Value} db");
                }

                Console.WriteLine("\nSikeresen örökbe fogadott állatok száma fajonként:");
                foreach (var item in stats.AdoptedBySpecies)
                {
                    Console.WriteLine($"  - {item.Key}: {item.Value} db");
                }

                Pause();
            }
            catch (Exception ex)
            {
                ShowError($"Hiba: {ex.Message}");
            }
        }

        private void ShowMostActiveVolunteers()
        {
            Console.Clear();
            Console.WriteLine("--- LEGAKTÍVABB ÖNKÉNTESEK ---");
            List<VolunteerActivity> topVolunteers = _adminService.GetMostActiveVolunteers();

            if (topVolunteers.Count == 0)
            {
                Console.WriteLine("Jelenleg nincsenek önkéntesek a rendszerben.");
            }
            else
            {
                int rank = 1;
                foreach (var item in topVolunteers)
                {
                    Console.WriteLine($"{rank}. [{item.Volunteer.Id}] {item.Volunteer.Name} - Teljesített feladatok: {item.CompletedTasksCount} db");
                    rank++;
                }
            }

            Pause();
        }

        private static void ShowError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n{msg}");
            Console.ResetColor();
            Pause();
        }

        private static void ShowSuccess(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n{msg}");
            Console.ResetColor();
            Pause();
        }

        private static void Pause()
        {
            Console.WriteLine("\nEnter a folytatáshoz...");
            Console.ReadLine();
        }
    }
}