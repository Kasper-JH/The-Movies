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

        // Henter alle forestillinger
        public IEnumerable<Screening> GetAll()
        {
            return _screenings ?? new List<Screening>();
        }

        // Tjekker om et tidsrum overlapper en eksisterende forestilling i samme
        // biograf/sal, jf. UC2 undtagelsesflow 6a.
        public bool HasOverlap(Cinema cinema, Hall hall, DateTime start, DateTime end)
        {
            if (cinema == null || hall == null)
                return false;

            foreach (var existing in _screenings)
            {
                bool sameLocation = existing.Cinema.Name == cinema.Name &&
                                     existing.Hall.HallNumber == hall.HallNumber;

                if (!sameLocation)
                    continue;

                // To tidsrum overlapper, hvis det ene starter, før det andet slutter, og omvendt
                bool overlaps = start < existing.EndTime && existing.StartTime < end;

                if (overlaps)
                    return true;
            }

            return false;
        }

        // Gemmer en ny forestilling i datakilden (både in-memory og til fil)
        public void SaveScreening(Screening screening)
        {
            if (screening == null)
                throw new ArgumentNullException(nameof(screening));

            _screenings.Add(screening);

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

        // Gemmer alle ændringer til den fysiske fil (persistens)
        public void SaveToFile()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_screenings, options);

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
