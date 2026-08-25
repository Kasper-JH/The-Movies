/*
 * Single Responsibility Principle (SRP):
 * Denne klasse repræsenterer en film som data.
 * Den indeholder automatiske properties for filmens titel, varighed og genre.
 * Klassen har INGEN logik, INGEN persistens, INGEN validering og INGEN UI-notifikation.
 * 
 */

namespace TheMovies.Core.Models
{
    public class Movie
    {
        // Parameterløs constructor til JSON-deserialisering. 
        public Movie() { }

        public Movie(string title, int duration, string genre)
        {
            Title = title;
            Duration = duration;
            Genre = genre;
        }

        public string Title { get; set; }
        public int Duration { get; set; }  // Varighed i minutter (int, men kan senere ændres til TimeSpan hvis nødvendigt)
        public string Genre { get; set; }
    }
}