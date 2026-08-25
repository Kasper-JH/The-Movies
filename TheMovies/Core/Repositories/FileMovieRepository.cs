/*
 * Single Responsibility Principle (SRP):
 * Denne klasse har kun én opgave: at håndtere persistens af film til en JSON-fil.
 * Den implementerer IMovieRepository og står for:
 * - At læse film fra en JSON-fil ved opstart (LoadFromFile)
 * - At skrive film til JSON-filen ved gem (SaveChanges)
 * - CRUD-operationer i hukommelsen (List<Movie>)
 * 
 * Klassen har INGEN forretningslogik (f.eks. validering) - det overlades til ViewModel.
 * 
 * JSON-filen oprettes i projektets output-mappe (bin/Debug/net...).
 * Hvis filen ikke findes ved opstart, startes med en tom liste.
 * Ved fejl i JSON-parsing (korrupt fil) startes også med en tom liste.
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
        // Stien til JSON-filen (relativ til output-mappen)
        private readonly string _filePath = "movies.json";

        // In-memory liste over film (arbejdshukommelse)
        private List<Movie> _movies;

        // Constructor: Indlæser film fra fil ved opstart (PERSISTENS)
        public FileMovieRepository()
        {
            _movies = LoadFromFile();
        }

        // READ: Returnerer alle film (bruges ved opstart og navigation)
        public IEnumerable<Movie> GetAll()
        {
            // Returner _movies hvis den ikke er null, men ellers så returner en ny tom liste
            return _movies ?? new List<Movie>();
        }

        // CREATE: Tilføjer en ny film til listen
        public void Add(Movie movie)
        {
            if (movie == null)
                throw new ArgumentNullException(nameof(movie));

            // Exception Flow 4a: Tjek om filmen allerede findes
            if (Exists(movie))
                throw new InvalidOperationException($"Filmen '{movie.Title}' findes allerede!");

            _movies.Add(movie);
        }

        // VALIDATION: Tjekker om en film med præcis samme Title, Duration og Genre allerede findes
        // Bruges til at forhindre dubletter (Exception Flow 4a)
        public bool Exists(Movie movie)
        {
            if (movie == null)
                return false;

            return _movies.Any(m =>
                string.Equals(m.Title, movie.Title, StringComparison.OrdinalIgnoreCase) &&
                m.Duration == movie.Duration &&
                string.Equals(m.Genre, movie.Genre, StringComparison.OrdinalIgnoreCase));
        }

        // PERSIST: Gemmer alle film til JSON-filen
        public void SaveChanges()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_movies, options);
            File.WriteAllText(_filePath, json);
        }

        // Privat hjælpemetode: Indlæser film fra JSON-fil
        // Kaldes i constructoren ved programopstart
        private List<Movie> LoadFromFile()
        {
            // Hvis filen ikke findes, returner en tom liste
            if (!File.Exists(_filePath))
                return new List<Movie>();

            try
            {
                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<Movie>>(json) ?? new List<Movie>();
            }
            catch
            {
                // Hvis filen er korrupt eller tom, start med en tom liste
                return new List<Movie>();
            }
        }
    }
}