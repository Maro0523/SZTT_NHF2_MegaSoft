using System;
using ShelterManagement.App.Models;
using ShelterManagement.App.Services;

namespace ShelterManagement.App;

class Program
{
    private static ShelterData _data = new();

    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        string dataFilePath = "shelter_data.json";

        Console.WriteLine("=== Állatmenhely és Örökbefogadás ===");
        Console.WriteLine("Adatok betöltése folyamatban...");

        try
        {
            _data = DataLoader.LoadAndValidate(dataFilePath);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Az adatok betöltése és ellenőrzése sikeresen megtörtént!\n");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nHIBA AZ ADATOK BETÖLTÉSEKOR: {ex.Message}");
            Console.WriteLine("A program nem tud elindulni, érvénytelen adatok.");
            Console.ResetColor();
            Console.WriteLine("\nNyomj egy gombot a kilépéshez...");
            Console.ReadKey();
            return;
        }

        RunMainMenu();
    }

    private static void RunMainMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("                FŐMENÜ                  ");
            Console.WriteLine("========================================");
            Console.WriteLine("1. Adminisztrátor");
            Console.WriteLine("2. Gondozó");
            Console.WriteLine("3. Örökbefogadó");
            Console.WriteLine("4. Önkéntes");
            Console.WriteLine("0. Kilépés");
            Console.Write("\nVálassz szerepkört: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    RunAdminMenu();
                    break;
                case "2":
                    SelectCaretaker();
                    break;
                case "3":
                    SelectAdopter();
                    break;
                case "4":
                    RunVolunteerMenu();
                    break;
                case "0":
                    Console.WriteLine("\nViszlát!");
                    return;
                default:
                    ShowError("Érvénytelen!");
                    break;
            }
        }
    }

    private static void RunAdminMenu()
    {
        Console.Clear();
        Console.WriteLine("--- ADMINISZTRÁTORI MENÜ ---"); //Ez a 2. tag modulja
        Console.WriteLine("1. Szektorok és kenelek kezelése");
        Console.WriteLine("2. Menhely működési statisztikák");
        Console.WriteLine("3. Legaktívabb önkéntesek listája");
        Console.WriteLine("0. Vissza a főmenübe");
        Pause();
    }

    private static void SelectCaretaker()
    {
        Console.Clear();
        Console.WriteLine("--- GONDOZÓ VÁLASZTÁSA ---");
        foreach (var c in _data.Caretakers)
        {
            Console.WriteLine($"[{c.Id}] {c.Name}");
        }
        Console.Write("\nVálaszd ki az azonosítódat (vagy 0 a visszalépéshez): ");
        if (int.TryParse(Console.ReadLine(), out int id) && id != 0)
        {
            var caretaker = _data.Caretakers.FirstOrDefault(c => c.Id == id);
            if (caretaker != null)
            {
                RunCaretakerMenu(caretaker);
            }
            else
            {
                ShowError("Nincs ilyen azonosítójú gondozó!");
            }
        }
    }

    private static void RunCaretakerMenu(Caretaker caretaker)
    {
        Console.Clear();
        Console.WriteLine($"--- GONDOZÓI FELÜLET: {caretaker.Name} ---"); //Ez a 3. tag modulja
        Console.WriteLine("1. Új állat rögzítése és elhelyezése");
        Console.WriteLine("2. Állat adatainak vagy státuszának módosítása");
        Console.WriteLine("3. Egészségügyi bejegyzés rögzítése");
        Console.WriteLine("4. Örökbefogadási jelentkezések elbírálása");
        Console.WriteLine("5. Próbaidő lezárása");
        Console.WriteLine("6. Önkéntes feladat meghirdetése vagy kezelése");
        Console.WriteLine("0. Vissza a főmenübe");
        Pause();
    }

    private static void SelectAdopter()
    {
        Console.Clear();
        Console.WriteLine("--- ÖRÖKBEFOGADÓ VÁLASZTÁSA ---");
        foreach (var a in _data.Adopters)
        {
            Console.WriteLine($"[{a.Id}] {a.Name} ({a.Email})");
        }
        Console.Write("\nVálaszd ki az azonosítódat (vagy 0 a visszalépéshez): ");
        if (int.TryParse(Console.ReadLine(), out int id) && id != 0)
        {
            var adopter = _data.Adopters.FirstOrDefault(a => a.Id == id);
            if (adopter != null)
            {
                RunAdopterMenu(adopter);
            }
            else
            {
                ShowError("Nincs ilyen azonosító!");
            }
        }
    }

    private static void RunAdopterMenu(Adopter adopter)
    {
        Console.Clear();
        Console.WriteLine($"--- ÖRÖKBEFOGADÓI FELÜLET: {adopter.Name} ---"); //Ez a 3. tag modulja
        Console.WriteLine("1. Állatok megtekintése és összetett keresés (faj, kor, név)");
        Console.WriteLine("2. Örökbefogadási szándék vagy jelentkezés leadása");
        Console.WriteLine("0. Vissza a főmenübe");
        Pause();
    }

    private static void RunVolunteerMenu()
    {
        Console.Clear();
        Console.WriteLine("--- ÖNKÉNTES FELÜLET ---"); //Ez a 4. tag modulja
        Console.WriteLine("1. Elérhető feladatok");
        Console.WriteLine("2. Jelentkezés feladatra");
        Console.WriteLine("0. Vissza a főmenübe");
        Pause();
    }

    private static void ShowError(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
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