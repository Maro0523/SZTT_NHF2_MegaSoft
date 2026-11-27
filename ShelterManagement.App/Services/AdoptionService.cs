using System;
using System.Collections.Generic;
using ShelterManagement.App.Models;

namespace ShelterManagement.App.Services
{
    public class AdoptionService
    {
        private readonly ShelterData _data;
        private readonly AnimalService _animalService;

        public AdoptionService(ShelterData data)
        {
            _data = data;
            _animalService = new AnimalService(data);
        }

        // Orokbefogadoi muveletek 

        public Application SubmitApplication(int animalId, int adopterId)
        {
            Animal? animal = _animalService.GetAnimalById(animalId);
            if (animal == null)
            {
                throw new Exception($"Nem található állat ezzel az azonosítóval: {animalId}");
            }

            if (animal.Status != "örökbefogadható")
            {
                throw new Exception("Erre az állatra jelenleg nem lehet jelentkezni, mert nem örökbefogadható státuszú!");
            }

            Adopter? adopter = FindAdopterById(adopterId);
            if (adopter == null)
            {
                throw new Exception($"Nem található örökbefogadó ezzel az azonosítóval: {adopterId}");
            }

            // Ellenorzes: ugyanarra az allatra csak egyszer jelentkezhet
            foreach (var app in _data.Applications)
            {
                if (app.AnimalId == animalId && app.AdopterId == adopterId)
                {
                    throw new Exception("Erre az állatra már adtál le jelentkezést!");
                }
            }

            int maxId = 0;
            foreach (var app in _data.Applications)
            {
                if (app.Id > maxId)
                {
                    maxId = app.Id;
                }
            }

            Application newApp = new Application();
            newApp.Id = maxId + 1;
            newApp.AnimalId = animalId;
            newApp.AdopterId = adopterId;
            newApp.ApplicationDate = DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd");
            newApp.Status = "elbírálás alatt";

            _data.Applications.Add(newApp);
            return newApp;
        }

        public List<Application> GetApplicationsByAdopter(int adopterId)
        {
            List<Application> result = new List<Application>();
            foreach (var app in _data.Applications)
            {
                if (app.AdopterId == adopterId)
                {
                    result.Add(app);
                }
            }
            return result;
        }

        // Gondozoi muveletek: elbiralas es probaido

        public List<Application> GetPendingApplications()
        {
            List<Application> pending = new List<Application>();
            foreach (var app in _data.Applications)
            {
                if (app.Status == "elbírálás alatt")
                {
                    pending.Add(app);
                }
            }
            return pending;
        }

        public void EvaluateApplication(int applicationId, bool approve)
        {
            Application? app = FindApplicationById(applicationId);
            if (app == null)
            {
                throw new Exception($"Nem található jelentkezés ezzel az azonosítóval: {applicationId}");
            }

            if (app.Status != "elbírálás alatt")
            {
                throw new Exception("Ez a jelentkezés már korábban elbírálásra került!");
            }

            Animal? animal = _animalService.GetAnimalById(app.AnimalId);
            if (animal == null)
            {
                throw new Exception("A jelentkezéshez tartozó állat nem található!");
            }

            if (!approve)
            {
                app.Status = "elutasított";
                return;
            }

            if (animal.Status != "örökbefogadható")
            {
                throw new Exception("Az állat már nincs örökbefogadható státuszban!");
            }

            // Jelentkezes elfogadasa: probaido, kenelbol ki
            app.Status = "elfogadott";
            animal.Status = "próbaidőn";
            animal.KennelId = null;

            // tobbi jelentkezest elutasitja ilyenkor
            foreach (var otherApp in _data.Applications)
            {
                if (otherApp.AnimalId == animal.Id && otherApp.Id != app.Id && otherApp.Status == "elbírálás alatt")
                {
                    otherApp.Status = "elutasított";
                }
            }
        }

        public List<Animal> GetAnimalsOnTrial()
        {
            List<Animal> trialAnimals = new List<Animal>();
            foreach (var a in _data.Animals)
            {
                if (a.Status == "próbaidőn")
                {
                    trialAnimals.Add(a);
                }
            }
            return trialAnimals;
        }

        public void FinishTrialPeriod(int animalId, bool success, int? returnKennelId)
        {
            Animal? animal = _animalService.GetAnimalById(animalId);
            if (animal == null)
            {
                throw new Exception($"Nem található állat ezzel az azonosítóval: {animalId}");
            }

            if (animal.Status != "próbaidőn")
            {
                throw new Exception("Ez az állat jelenleg nincs próbaidőn!");
            }

            if (success)
            {
                // Sikeres probaido: vegleges orokbefogadas
                animal.Status = "örökbe fogadott";
                animal.KennelId = null;
                animal.AdoptionDate = DateOnly.FromDateTime(DateTime.Now).ToString("yyyy-MM-dd");
            }
            else
            {
                // Sikertelen probaido: visszakerul a menhelyre egy kennelbe
                if (!returnKennelId.HasValue)
                {
                    throw new Exception("Sikertelen próbaidő esetén meg kell adni a kennelt, ahová az állat visszakerül!");
                }

                _animalService.ValidateKennelPlacement(returnKennelId.Value, animal.Species, animal.Id);
                animal.KennelId = returnKennelId.Value;
                animal.Status = "örökbefogadható";
                animal.AdoptionDate = null;
            }
        }

        // Segedmethodusok

        public Adopter? FindAdopterById(int id)
        {
            foreach (var a in _data.Adopters)
            {
                if (a.Id == id)
                {
                    return a;
                }
            }
            return null;
        }

        private Application? FindApplicationById(int id)
        {
            foreach (var app in _data.Applications)
            {
                if (app.Id == id)
                {
                    return app;
                }
            }
            return null;
        }
    }
}