using System;
using System.Collections.Generic;
using ShelterManagement.App.Models;
using ShelterManagement.App.Services;

namespace ShelterManagement.App.UI
{
    public class CaretakerMenu
    {
        private readonly Caretaker _currentCaretaker;
        private readonly AnimalService _animalService;
        private readonly AdoptionService _adoptionService;

        public CaretakerMenu(ShelterData data, Caretaker caretaker)
        {
            _currentCaretaker = caretaker;
            _animalService = new AnimalService(data);
            _adoptionService = new AdoptionService(data);
        }

        public void Run()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($" GONDOZÓI FELÜLET: {_currentCaretaker.Name} ---");
                Console.WriteLine("1. Állatok listázása");
                Console.WriteLine("2. Új állat rögzítése és elhelyezése");
                Console.WriteLine("3. Állat adatainak, elhelyezésének vagy státuszának módosítása");
                Console.WriteLine("4. Egészségügyi bejegyzések megtekintése és új rögzítése");
                Console.WriteLine("5. Örökbefogadási jelentkezések elbírálása");
                Console.WriteLine("6. Próbaidő lezárása");
                Console.WriteLine("0. Vissza a főmenübe");
                Console.Write("\nVálassz menüpontot: ");

                string? choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        ListAllAnimals(true);
                        break;
                    case "2":
                        AddAnimalMenu();
                        break;
                    case "3":
                        UpdateAnimalMenu();
                        break;
                    case "4":
                        HealthRecordMenu();
                        break;
                    case "5":
                        EvaluateApplicationsMenu();
                        break;
                    case "6":
                        FinishTrialMenu();
                        break;
                    case "0":
                        return;
                    default:
                        ShowError("Érvénytelen választás!");
                        break;
                }
            }
        }

        private void ListAllAnimals(bool pause)
        {
            Console.WriteLine("\n MENHELYEN NYILVÁNTARTOTT ÁLLATOK");
            List<Animal> animals = _animalService.GetAllAnimals();

            if (animals.Count == 0)
            {
                Console.WriteLine("Nincs állat a rendszerben.");
            }
            else
            {
                foreach (var a in animals)
                {
                    string kennelText = a.KennelId.HasValue ? $"Kennel: {a.KennelId}" : "Nincs kennel";
                    Console.WriteLine($"[{a.Id}] {a.Name} | {a.Species} ({a.Breed}) | {a.Age} év | {a.Gender} | Státusz: {a.Status} | {kennelText}");
                }
            }

            if (pause)
            {
                Pause();
            }
        }

        private void AddAnimalMenu()
        {
            Console.Clear();
            Console.WriteLine("ÚJ ÁLLAT HOZZÁADÁSA");
            try
            {
                Console.Write("Név: ");
                string name = Console.ReadLine() ?? "";

                Console.Write("Faj (kutya / macska): ");
                string species = Console.ReadLine() ?? "";

                Console.Write("Fajta: ");
                string breed = Console.ReadLine() ?? "";

                Console.Write("Becsült életkor (év): ");
                if (!int.TryParse(Console.ReadLine(), out int age))
                {
                    throw new Exception("Érvénytelen életkor!");
                }

                Console.Write("Nem (kan / nőstény): ");
                string gender = Console.ReadLine() ?? "";

                Console.Write($"Bekerülés dátuma (ÉÉÉÉ-HH-NN, üres = ma): ");
                string? dateInput = Console.ReadLine();
                string intakeDate = string.IsNullOrWhiteSpace(dateInput)
                    ? DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd")
                    : dateInput;

                Console.WriteLine("\nVálassz státuszt:");
                Console.WriteLine("1. örökbefogadható");
                Console.WriteLine("2. karanténban/kezelés alatt");
                Console.Write("Választás: ");
                string? stChoice = Console.ReadLine();
                string status = stChoice == "1" ? "örökbefogadható"
                              : stChoice == "2" ? "karanténban/kezelés alatt"
                              : throw new ArgumentException("Érvénytelen státusz választás!");

                PrintAvailableKennels(species);
                Console.Write("\nKennel ID: ");
                if (!int.TryParse(Console.ReadLine(), out int kennelId))
                {
                    throw new Exception("Érvénytelen kennel ID!");
                }

                Animal created = _animalService.AddAnimal(name, species, breed, age, gender, intakeDate, status, kennelId);
                ShowSuccess($"Állat sikeresen rögzítve: [{created.Id}] {created.Name} (Kennel: {created.KennelId})");
            }
            catch (Exception ex)
            {
                ShowError($"Hiba: {ex.Message}");
            }
        }

        private void UpdateAnimalMenu()
        {
            Console.Clear();
            Console.WriteLine("ÁLLAT ADATAINAK ÉS ELHELYEZÉSÉNEK MÓDOSÍTÁSA");
            ListAllAnimals(false);

            try
            {
                Console.Write("\nMódosítandó állat ID: ");
                if (!int.TryParse(Console.ReadLine(), out int animalId))
                {
                    throw new Exception("Érvénytelen állat ID!");
                }

                Animal? animal = _animalService.GetAnimalById(animalId);
                if (animal == null)
                {
                    throw new Exception("Nem található ilyen azonosítójú állat!");
                }

                Console.Write($"Új név (jelenlegi: {animal.Name}, Enter = marad): ");
                string? nameInput = Console.ReadLine();
                string newName = string.IsNullOrWhiteSpace(nameInput) ? animal.Name : nameInput;

                Console.Write($"Új fajta (jelenlegi: {animal.Breed}, Enter = marad): ");
                string? breedInput = Console.ReadLine();
                string newBreed = string.IsNullOrWhiteSpace(breedInput) ? animal.Breed : breedInput;

                Console.Write($"Új életkor (jelenlegi: {animal.Age}, Enter = marad): ");
                string? ageInput = Console.ReadLine();
                int newAge = string.IsNullOrWhiteSpace(ageInput) ? animal.Age : int.Parse(ageInput);

                Console.Write($"Új nem (jelenlegi: {animal.Gender}, Enter = marad): ");
                string? genderInput = Console.ReadLine();
                string newGender = string.IsNullOrWhiteSpace(genderInput) ? animal.Gender : genderInput;

                Console.WriteLine($"\nJelenlegi státusz: {animal.Status}");
                Console.WriteLine("1. örökbefogadható");
                Console.WriteLine("2. karanténban/kezelés alatt");
                Console.Write("Új státusz (Enter = marad): ");
                string? stInput = Console.ReadLine();
                string newStatus = string.IsNullOrWhiteSpace(stInput) ? animal.Status
                                 : stInput == "1" ? "örökbefogadható"
                                 : stInput == "2" ? "karanténban/kezelés alatt"
                                 : throw new Exception("Érvénytelen státusz!");

                int? newKennelId = animal.KennelId;
                if (newStatus == "örökbefogadható" || newStatus == "karanténban/kezelés alatt")
                {
                    PrintAvailableKennels(animal.Species);
                    Console.Write($"\nKennel ID (jelenlegi: {animal.KennelId?.ToString() ?? "nincs"}, Enter = marad): ");
                    string? kInput = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(kInput))
                    {
                        newKennelId = int.Parse(kInput);
                    }
                }

                _animalService.UpdateAnimal(animalId, newName, newBreed, newAge, newGender, newStatus, newKennelId);
                ShowSuccess("Állat adatai sikeresen frissítve!");
            }
            catch (Exception ex)
            {
                ShowError($"Hiba: {ex.Message}");
            }
        }

        private void HealthRecordMenu()
        {
            Console.Clear();
            Console.WriteLine("EGÉSZSÉGÜGYI NAPLÓ");
            ListAllAnimals(false);

            try
            {
                Console.Write("\nÁllat ID: ");
                if (!int.TryParse(Console.ReadLine(), out int animalId))
                {
                    throw new Exception("Érvénytelen állat ID!");
                }

                List<HealthRecord> records = _animalService.GetHealthRecordsForAnimal(animalId);
                Console.WriteLine("\nKorábbi egészségügyi bejegyzések (nem módosíthatók):");
                if (records.Count == 0)
                {
                    Console.WriteLine("  (Nincs még rögzített esemény)");
                }
                else
                {
                    foreach (var r in records)
                    {
                        Console.WriteLine($"  - [{r.Id}] {r.Date} | {r.Type} | Elvégezte: {r.PerformedBy}");
                    }
                }

                Console.Write("\nSzeretnél új bejegyzést rögzíteni? (i / n): ");
                string? ans = Console.ReadLine();
                if (ans?.Trim().ToLower() == "i")
                {
                    Console.Write("Dátum (ÉÉÉÉ-HH-NN, üres = ma): ");
                    string? dateInput = Console.ReadLine();
                    string date = string.IsNullOrWhiteSpace(dateInput)
                        ? DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd")
                        : dateInput;

                    Console.Write("Esemény típusa (pl. oltás, általános vizsgálat): ");
                    string type = Console.ReadLine() ?? "";

                    Console.Write($"Elvégző személy (üres = {_currentCaretaker.Name}): ");
                    string? perfInput = Console.ReadLine();
                    string performedBy = string.IsNullOrWhiteSpace(perfInput) ? _currentCaretaker.Name : perfInput;

                    _animalService.AddHealthRecord(animalId, date, type, performedBy);
                    ShowSuccess("Egészségügyi bejegyzés sikeresen rögzítve!");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Hiba: {ex.Message}");
            }
        }

        private void EvaluateApplicationsMenu()
        {
            Console.Clear();
            Console.WriteLine("FÜGGŐBEN LÉVŐ ÖRÖKBEFOGADÁSI JELENTKEZÉSEK");
            List<Application> pending = _adoptionService.GetPendingApplications();

            if (pending.Count == 0)
            {
                Console.WriteLine("Nincs elbírálásra váró jelentkezés.");
                Pause();
                return;
            }

            foreach (var app in pending)
            {
                Animal? animal = _animalService.GetAnimalById(app.AnimalId);
                Adopter? adopter = _adoptionService.FindAdopterById(app.AdopterId);
                Console.WriteLine($"[{app.Id}] Állat: {animal?.Name} (ID: {app.AnimalId}) | Jelentkező: {adopter?.Name} (ID: {app.AdopterId}) | Dátum: {app.ApplicationDate}");
            }

            try
            {
                Console.Write("\nElbírálandó jelentkezés ID (0 = vissza): ");
                if (!int.TryParse(Console.ReadLine(), out int appId) || appId == 0)
                {
                    return;
                }

                Console.Write("Döntés (1 = Elfogadás / Próbaidő indítása, 2 = Elutasítás): ");
                string? decision = Console.ReadLine();

                if (decision == "1")
                {
                    _adoptionService.EvaluateApplication(appId, true);
                    ShowSuccess("Jelentkezés elfogadva! Az állat próbaidőre került.");
                }
                else if (decision == "2")
                {
                    _adoptionService.EvaluateApplication(appId, false);
                    ShowSuccess("Jelentkezés elutasítva.");
                }
                else
                {
                    throw new Exception("Érvénytelen döntés!");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Hiba: {ex.Message}");
            }
        }

        private void FinishTrialMenu()
        {
            Console.Clear();
            Console.WriteLine("PRÓBAIDŐ LEZÁRÁSA");
            List<Animal> trialAnimals = _adoptionService.GetAnimalsOnTrial();

            if (trialAnimals.Count == 0)
            {
                Console.WriteLine("Jelenleg nincs próbaidőn lévő állat.");
                Pause();
                return;
            }

            foreach (var a in trialAnimals)
            {
                Console.WriteLine($"[{a.Id}] {a.Name} ({a.Species}, {a.Breed})");
            }

            try
            {
                Console.Write("\nÁllat ID (0 = vissza): ");
                if (!int.TryParse(Console.ReadLine(), out int animalId) || animalId == 0)
                {
                    return;
                }

                Animal? selected = _animalService.GetAnimalById(animalId);
                if (selected == null)
                {
                    throw new Exception("Nem található ilyen állat!");
                }

                Console.Write("Próbaidő eredménye (1 = Sikeres örökbefogadás, 2 = Sikertelen örökbefogadás): ");
                string? res = Console.ReadLine();

                if (res == "1")
                {
                    _adoptionService.FinishTrialPeriod(animalId, true, null);
                    ShowSuccess("Próbaidő sikeresen lezárva! Az állat örökbe fogadott státuszba került.");
                }
                else if (res == "2")
                {
                    PrintAvailableKennels(selected.Species);
                    Console.Write("\nMelyik kennelbe kerüljön vissza az állat? Kennel ID: ");
                    if (!int.TryParse(Console.ReadLine(), out int kennelId))
                    {
                        throw new Exception("Érvénytelen kennel ID!");
                    }

                    _adoptionService.FinishTrialPeriod(animalId, false, kennelId);
                    ShowSuccess("Az állat visszakerült a menhelyre és újra örökbefogadható.");
                }
                else
                {
                    throw new Exception("Érvénytelen választás!");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Hiba: {ex.Message}");
            }
        }

        private void PrintAvailableKennels(string species)
        {
            Console.WriteLine($"\nElérhető kennelek ({species}):");
            List<Kennel> kennels = _animalService.GetAvailableKennels(species);
            if (kennels.Count == 0)
            {
                Console.WriteLine("  (Nincs szabad kennel ehhez a fajhoz!)");
                return;
            }

            foreach (var k in kennels)
            {
                int count = _animalService.CountAnimalsInKennel(k.Id, null);
                Console.WriteLine($"  -> Kennel [{k.Id}] (Szektor: {k.SectorId}) | Foglaltság: {count}/{k.Capacity}");
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