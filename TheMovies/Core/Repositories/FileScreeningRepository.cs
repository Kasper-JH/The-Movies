/*
 * SRP: Denne klasse har ansvar for at gemme og hente forestillinger fra en JSON-fil.
 * Den implementerer IScreeningRepository og håndterer al persistens.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TheMovies.Core.Models;

namespace TheMovies.Core.Repositories
{
    public class FileScreeningRepository : IScreeningRepository
    {
        // Stien til JSON-filen (gemmes i samme mappe som programmet)
        private readonly string _filePath = "screenings.json";

        // Listen over forestillinger som vi arbejder med i hukommelsen
        private List<Screening> _screenings;

        // Constructor: Indlæser forestillinger fra filen ved opstart
        public FileScreeningRepository()
        {
            _screenings = LoadFromFile();
        }

        // Henter alle forestillinger. Returnerer en tom liste hvis der ikke er nogen.
        public IEnumerable<Screening> GetAll()
        {
            return _screenings ?? new List<Screening>();
        }

        // Tjekker om en kandidat-forestilling overlapper en eksisterende forestilling i samme
        // biograf/sal, jf. UC2 undtagelsesflow 6a. Selve overlap-reglen ligger på
        // Screening.OverlapsWith() (domænelogik) - her itererer vi blot de eksisterende
        // forestillinger og spørger hver af dem via deres egen OverLapsWith(), dvs. vi arbejder her med at 
        // hente (specifikke) forestillinger.
        public bool HasOverlap(Screening candidate)
        {
            if (candidate == null)
                return false;

            foreach (var existing in _screenings)
            {
                if (existing.OverlapsWith(candidate))
                    return true;
            }

            return false;
        }

        // Gemmer en ny forestilling i datakilden (både in-memory og til fil)
        public void SaveScreening(Screening screening)
        {
            // Hvis vi har screenings, så tilføjer vi dem in-memory.
            if (screening == null)
                throw new ArgumentNullException(nameof(screening));

            _screenings.Add(screening);

            // Forsøger at gemme til fil.
            try
            {
                SaveToFile();
            }
            catch
            {
                _screenings.Remove(screening); // Rul tilbage hvis filen ikke kunne skrives
                throw;
            }
        }

        // Privat hjælpemetode: Skriver den nuværende in-memory liste til fil.
        // Kaldes internt af SaveScreening ovenfor
        private void SaveToFile()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_screenings, options);

            // Forsøg at skrive til fil, og kast eventuelle IO-fejl. 
            try
            {
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new InvalidOperationException(
                    $"Kunne ikke gemme forestillinger til filen '{_filePath}'. Tjek at filen ikke er i brug, og at der er skriveadgang.",
                    ex);
            }
        }

        // Privat metode: Indlæser forestillinger fra JSON-filen
        private List<Screening> LoadFromFile()
        {
            // Hvis filen ikke eksisterer, så returneres en ny, tom liste (f.eks. første gang programmet køres).
            if (!File.Exists(_filePath))
                return new List<Screening>();

            string json;
            try
            {
                json = File.ReadAllText(_filePath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                throw new InvalidOperationException(
                    $"Kunne ikke læse filen '{_filePath}'. Tjek at filen ikke er i brug, og at der er læseadgang.",
                    ex);
            }

            try
            {
                return JsonSerializer.Deserialize<List<Screening>>(json) ?? new List<Screening>();
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"Filen '{_filePath}' indeholder ugyldig JSON. Filen kan være korrupt.",
                    ex);
            }
        }
    }
}
