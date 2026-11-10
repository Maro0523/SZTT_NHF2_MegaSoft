using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using ShelterManagement.App.Models;

namespace ShelterManagement.App.Services;
public class DataLoader
{
    public static ShelterData LoadAndValidate(string filePath)
    {
        //letezik a fajl?
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"A megadott fájl nem található: {filePath}");
        }

        string jsonContent = File.ReadAllText(filePath);

        // JSON szintaxis es deszerializacio leellenorzese
        ShelterData data;
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            data = JsonSerializer.Deserialize<ShelterData>(jsonContent, options)
                   ?? throw new InvalidOperationException("A JSON fájl üres!");
        }
        catch (JsonException ex)
        {
            throw new FormatException($"Érvénytelen formátum: {ex.Message}");
        }

        //validaciok
        ValidateSectorsAndKennels(data);
        ValidateForeignKeys(data);
        ValidateKennelIntegrity(data);
        ValidateApplications(data);

        return data;
    }

    private static void ValidateSectorsAndKennels(ShelterData data)
    {
        //10 kennel / szektor
        var kennelsBySector = data.Kennels.GroupBy(k => k.SectorId);
        foreach (var group in kennelsBySector)
        {
            if (group.Count() > 10)
            {
                throw new InvalidOperationException($"A(z) {group.Key} azonosítójú szektorban több mint 10 kennel található: {group.Count()} db!");
            }
        }

        //inaktiv szektorban nem lehet aktiv kennel
        var inactiveSectorIds = data.Sectors.Where(s => !s.Active).Select(s => s.Id).ToHashSet();
        foreach (var kennel in data.Kennels)
        {
            if (kennel.Active && inactiveSectorIds.Contains(kennel.SectorId))
            {
                throw new InvalidOperationException($"A(z) {kennel.Id} azonosítójú kennel aktív, de a hozzá tartozó {kennel.SectorId} azonosítójú szektor inaktív!");
            }
        }
    }

    private static void ValidateForeignKeys(ShelterData data)
    {
        var sectorIds = data.Sectors.Select(s => s.Id).ToHashSet();
        var kennelIds = data.Kennels.Select(k => k.Id).ToHashSet();
        var animalIds = data.Animals.Select(a => a.Id).ToHashSet();
        var adopterIds = data.Adopters.Select(ad => ad.Id).ToHashSet();

        //kennel -> szektor
        foreach (var kennel in data.Kennels)
        {
            if (!sectorIds.Contains(kennel.SectorId))
            {
                throw new InvalidOperationException($"A(z) {kennel.Id} kennel nem létező szektorra hivatkozik ({kennel.SectorId})!");
            }
        }

        //allat -> kennel (ha van megadva kennelId)
        foreach (var animal in data.Animals)
        {
            if (animal.KennelId.HasValue && !kennelIds.Contains(animal.KennelId.Value))
            {
                throw new InvalidOperationException($"A(z) {animal.Name} nevű állat ({animal.Id}) nem létező kennelre hivatkozik ({animal.KennelId})!");
            }
        }

        //egeszsegugyi bejegyzes -> allat
        foreach (var hr in data.HealthRecords)
        {
            if (!animalIds.Contains(hr.AnimalId))
            {
                throw new InvalidOperationException($"A(z) {hr.Id} azonosítójú egészségügyi bejegyzés nem létező állatra hivatkozik ({hr.AnimalId})!");
            }
        }

        //jelentkezes -> allat es orokbefogado
        foreach (var app in data.Applications)
        {
            if (!animalIds.Contains(app.AnimalId))
            {
                throw new InvalidOperationException($"A(z) {app.Id} jelentkezés nem létező állatra hivatkozik ({app.AnimalId})!");
            }
            if (!adopterIds.Contains(app.AdopterId))
            {
                throw new InvalidOperationException($"A(z) {app.Id} jelentkezés nem létező örökbefogadóra hivatkozik ({app.AdopterId})!");
            }
        }
    }

    private static void ValidateKennelIntegrity(ShelterData data)
    {
        var kennelsDict = data.Kennels.ToDictionary(k => k.Id);

        //csoportositjuk a kennelekben lako allatokat
        var animalsInKennels = data.Animals.Where(a => a.KennelId.HasValue).GroupBy(a => a.KennelId!.Value);

        foreach (var group in animalsInKennels)
        {
            int kennelId = group.Key;
            if (!kennelsDict.TryGetValue(kennelId, out var kennel))
            {
                continue;
            }

            //inaktiv kennelbe nem helyezheto el allat
            if (!kennel.Active)
            {
                throw new InvalidOperationException($"A(z) {kennelId} kennel inaktív, mégis van hozzárendelve állat!");
            }

            //kapacitas tullepes
            if (group.Count() > kennel.Capacity)
            {
                throw new InvalidOperationException($"A(z) {kennelId} kennel kapacitása ({kennel.Capacity}) kisebb, mint a benne lévő állatok száma ({group.Count()})!");
            }

            //fajazonossag ellenorzes: (kennel.AnimalType vs animal.Species)
            foreach (var animal in group)
            {
                if (!string.Equals(animal.Species, kennel.AnimalType, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"Fajeltérés a(z) {kennelId} kennelben: a kennel típusa '{kennel.AnimalType}', de a benne lévő állat faja '{animal.Species}' (Állat: {animal.Name})!");
                }
            }
        }
    }

    private static void ValidateApplications(ShelterData data)
    {
        //1 orokbefogado egy allatra csak 1x jelentkezhet
        var duplicateApplications = data.Applications.GroupBy(app => new { app.AdopterId, app.AnimalId }).Where(g => g.Count() > 1).ToList();

        if (duplicateApplications.Any())
        {
            var firstDup = duplicateApplications.First().Key;
            throw new InvalidOperationException($"Érvénytelen jelentkezés! A(z) {firstDup.AdopterId} azonosítójú jelentkező többször is jelentkezett a(z) {firstDup.AnimalId} azonosítójú állatra!");
        }
    }
}
