/*
 * SRP: Dette interface definerer de metoder, som et repository til film skal have.
 * Det gør det muligt at skifte mellem forskellige datakilder (f.eks. JSON, database) uden at ændre ViewModel.
 */

using System.Collections.Generic;
using TheMovies.Core.Models;

namespace TheMovies.Core.Repositories
{
    public interface IMovieRepository
    {
        // Henter alle film fra datakilden
        IEnumerable<Movie> GetAll();

        // Gemmer en ny film i datakilden (både in-memory og til fil)
        void SaveMovie(Movie movie);

        // Tjekker om en film allerede er registreret (undgår dubletter)
        bool IsMovieRegistered(Movie movie);

        // Gemmer alle ændringer til den fysiske fil (persistens)
        void SaveToFile();
    }
}