/*
 * SRP: Denne klasse har ansvar for at gemme og hente film fra en JSON-fil.
 * Den implementerer IMovieRepository og håndterer al persistens.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using TheMovies.Core.Models;

namespace TheMovies.Core.Repositories
{
    public class FileMovieRepository : IMovieRepository
    {
        // Stien til JSON-filen (gemmes i samme mappe som programmet)
        private readonly string _filePath = "movies.json";

        // Listen over film som vi arbejder med i hukommelsen
        private List<Movie> _movies;

        // Constructor: Indlæser film fra filen ved opstart
        public FileMovieRepository()
        {
            _movies = LoadFromFile();
        }

        // Henter alle film fra datakilden
        public IEnumerable<Movie> GetAll()
        {
            return _movies ?? new List<Movie>(); // Hvis listen er null, returneres en tom liste
        }

        // Gemmer en ny film i datakilden (både in-memory og til fil)
        public void SaveMovie(Movie movie)
        {
            // Stopper med en fejl hvis movie er null, så vi ikke fortsætter med tomme data
            if (movie == null)
                throw new ArgumentNullException(nameof(movie));
            _movies.Add(movie);

            // Gemmer ændringerne til fil (persistens)
            try
            {
                SaveToFile();
            }
            catch
            {
                _movies.Remove(movie); // Rul tilbage hvis filen ikke kunne skrives
                throw;
            }
        }

        // Tjekker om en film allerede er registreret (undgår dubletter). Selve duplikat-reglen
        // ligger på Movie.IsDuplicateOf() (domænelogik) - her itererer vi blot filmene og
        // spørger hver af dem, samme mønster som FileScreeningRepository.HasOverlap().
        public bool IsMovieRegistered(Movie movie)
        {
            if (movie == null)
                return false;
            return _movies.Any(m => m.IsDuplicateOf(movie));
        }

        // Opdaterer en eksisterende film (bruges i UC2 til at sætte instruktør/premieredato).
        // Filmen antages allerede at være den samme reference som findes i _movies
        // (den kommer fra GetAll() via ViewModel'ens dropdown), så vi behøver blot
        // at bekræfte, at den findes, og derefter persistere ændringerne.
        public void UpdateMovie(Movie movie)
        {
            if (movie == null)
                throw new ArgumentNullException(nameof(movie));

            bool found = false;
            foreach (var existing in _movies)
            {
                if (existing == movie)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                throw new InvalidOperationException("Filmen findes ikke i repository og kan derfor ikke opdateres.");

            SaveToFile();
        }

        // Privat hjælpemetode: skriver den aktuelle in-memory liste til fil.
        // Kaldes internt af SaveMovie og UpdateMovie - er ikke en del af IMovieRepository,
        // da ViewModels ikke skal kunne kalde ren fil-persistens uden om domænehandlingerne.
        private void SaveToFile()
        {
            var options = new JsonSerializerOptions { WriteIndented = true }; // Gør JSON læselig
            string json = JsonSerializer.Serialize(_movies, options);

            try
            {
                File.WriteAllText(_filePath, json); // Skriver til fil
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Hvis filen er i brug eller skrivebeskyttet, smider vi en brugervenlig fejl
                throw new InvalidOperationException(
                    $"Kunne ikke gemme film til filen '{_filePath}'. Tjek at filen ikke er i brug, og at der er skriveadgang.",
                    ex);
            }
        }

        // Privat metode: Indlæser film fra JSON-filen
        private List<Movie> LoadFromFile()
        {
            if (!File.Exists(_filePath))
                return new List<Movie>(); // Første gang filen ikke findes -> tom liste

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
                // Deserialiser JSON til en liste af Movie-objekter
                return JsonSerializer.Deserialize<List<Movie>>(json) ?? new List<Movie>();
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
