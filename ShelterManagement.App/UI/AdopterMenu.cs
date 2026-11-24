using System;
using System.Collections.Generic;
using ShelterManagement.App.Models;
using ShelterManagement.App.Services;

namespace ShelterManagement.App.UI
{
    public class AdopterMenu
    {
        private readonly Adopter _currentAdopter;
        private readonly AnimalService _animalService;
        private readonly AdoptionService _adoptionService;

        public AdopterMenu(ShelterData data, Adopter adopter)
        {
            _currentAdopter = adopter;
            _animalService = new AnimalService(data);
            _adoptionService = new AdoptionService(data);
        }

        public void Run()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"ÖRÖKBEFOGADÓI FELÜLET: {_currentAdopter.Name} ---");
                Console.WriteLine("1. Összes örökbefogadható állat megtekintése");
                Console.WriteLine("2. Állatok keresése szűrőkkel (faj, életkor, nem, név)");
                Console.WriteLine("3. Örökbefogadási jelentkezés leadása");
                Console.WriteLine("4. Saját jelentkezéseim megtekintése");
                Console.WriteLine("0. Vissza a főmenübe");
                Console.Write("\nVálassz menüpontot: ");

                string? choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        ListAdoptableAnimals();
                        break;
                    case "2":
                        SearchAnimalsMenu();
                        break;
                    case "3":
                        SubmitApplicationMenu();
                        break;
                    case "4":
                        ListMyApplications();
                        break;
                    case "0":
                        return;
                    default:
                        ShowError("Érvénytelen választás!");
                        break;
                }
            }
        }

        private void ListAdoptableAnimals()
        {
            Console.Clear();
            Console.WriteLine("ÖRÖKBEFOGADHATÓ ÁLLATOK");
            List<Animal> animals = _animalService.SearchAnimals(null, null, null, null, true);

            PrintAnimalList(animals);
            Pause();
        }

        private void SearchAnimalsMenu()
        {
            Console.Clear();
            Console.WriteLine("ÁLLATOK KERESÉSE (Hagyd üresen, ha nem szűrsz rá)");

            Console.Write("Faj (kutya / macska): ");
            string? species = Console.ReadLine();

            Console.Write("Maximum életkor (év): ");
            string? ageInput = Console.ReadLine();
            int? maxAge = null;
            if (!string.IsNullOrWhiteSpace(ageInput))
            {
                if (int.TryParse(ageInput, out int parsedAge) && parsedAge >= 0)
                {
                    maxAge = parsedAge;
                }
                else
                {
                    ShowError("Érvénytelen életkor formátum!");
                    return;
                }
            }

            Console.Write("Nem (kan / nőstény): ");
            string? gender = Console.ReadLine();

            Console.Write("Név részlet: ");
            string? namePart = Console.ReadLine();

            List<Animal> results = _animalService.SearchAnimals(species, maxAge, gender, namePart, true);

            Console.WriteLine("\nKERESÉSI TALÁLATOK ");
            PrintAnimalList(results);
            Pause();
        }

        private void SubmitApplicationMenu()
        {
            Console.Clear();
            Console.WriteLine("ÖRÖKBEFOGADÁSI JELENTKEZÉS LEADÁSA");
            List<Animal> animals = _animalService.SearchAnimals(null, null, null, null, true);
            PrintAnimalList(animals);

            if (animals.Count == 0)
            {
                Pause();
                return;
            }

            try
            {
                Console.Write("\nVálasztott állat ID: ");
                if (!int.TryParse(Console.ReadLine(), out int animalId))
                {
                    throw new FormatException("Érvénytelen azonosító!");
                }

                Application app = _adoptionService.SubmitApplication(animalId, _currentAdopter.Id);
                ShowSuccess($"Jelentkezés sikeresen rögzítve! (Jelentkezés ID: {app.Id}, Dátum: {app.ApplicationDate})");
            }
            catch (Exception ex)
            {
                ShowError($"Hiba: {ex.Message}");
            }
        }

        private void ListMyApplications()
        {
            Console.Clear();
            Console.WriteLine("SAJÁT JELENTKEZÉSEIM");
            List<Application> myApps = _adoptionService.GetApplicationsByAdopter(_currentAdopter.Id);

            if (myApps.Count == 0)
            {
                Console.WriteLine("Még nincs leadott jelentkezésed.");
            }
            else
            {
                foreach (var app in myApps)
                {
                    Animal? animal = _animalService.GetAnimalById(app.AnimalId);
                    string animalName = animal != null ? animal.Name : "Ismeretlen";
                    Console.WriteLine($"[{app.Id}] Állat: {animalName} (ID: {app.AnimalId}) | Dátum: {app.ApplicationDate} | Státusz: {app.Status}");
                }
            }

            Pause();
        }

        private void PrintAnimalList(List<Animal> animals)
        {
            if (animals.Count == 0)
            {
                Console.WriteLine("Nincs a feltételeknek megfelelő állat.");
                return;
            }

            foreach (var a in animals)
            {
                Console.WriteLine($"[{a.Id}] {a.Name} | Faj: {a.Species} ({a.Breed}) | Kor: {a.Age} év | Nem: {a.Gender} | Státusz: {a.Status}");
            }
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