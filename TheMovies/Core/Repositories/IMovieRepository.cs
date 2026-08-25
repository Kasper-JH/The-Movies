/*
 * Single Responsibility Principle (SRP):
 * Dette interface definerer kontrakten for datalagring af film.
 * Det adskiller FORRETNINGSLOGIKKEN (ViewModel) fra PERSISTENSLOGIKKEN (Repository).
 * ViewModel'en kender KUN dette interface - ikke den konkrete implementering.
 * 
 * CRUD-operationer:
 * - Create: Add(Movie movie) - tilføjer en ny film
 * - Read:   GetAll() - henter alle film (bruges ved opstart og navigation)
 * - Update: Ikke nødvendig i dette scope (film ændres ikke efter registrering)
 * - Delete: Ikke nødvendig i dette scope (film slettes ikke)
 * 
 * Exists bruges til validering før Create (undgår dubletter - Exception Flow 4a).
 * SaveChanges gemmer alle ændringer til den fysiske fil (JSON-persistens).
 * 
 * Bemærk: GetByTitle er fjernet, da navigation sker via indeks i en liste,
 * og der ikke er behov for søgefunktionalitet i dette scope.
 */

using System.Collections.Generic;
using TheMovies.Core.Models;

namespace TheMovies.Core.Repositories
{
    public interface IMovieRepository
    {
        // READ: Henter alle film fra datakilden (bruges ved opstart og til navigation)
        IEnumerable<Movie> GetAll();

        // CREATE: Tilføjer en ny film til datakilden (bruges ved registrering)
        void Add(Movie movie);

        // VALIDATION: Tjekker om en film med præcis samme Title, Duration og Genre allerede findes
        // Bruges i Exception Flow 4a: Hvis filmen allerede findes, informeres brugeren
        bool Exists(Movie movie);

        // PERSIST: Gemmer ændringer til den fysiske fil (f.eks. JSON)
        // Kaldes efter Add for at sikre data er gemt permanent
        void SaveChanges();
    }
}