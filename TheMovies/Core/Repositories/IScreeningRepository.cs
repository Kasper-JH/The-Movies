/*
 * SRP: Dette interface definerer de metoder, som et repository til forestillinger skal have.
 * Det gør det muligt at skifte mellem forskellige datakilder (f.eks. JSON, database) uden at ændre ViewModel.
 */

using System;
using System.Collections.Generic;
using TheMovies.Core.Models;

namespace TheMovies.Core.Repositories
{
    public interface IScreeningRepository
    {
        // Henter alle forestillinger fra datakilden
        IEnumerable<Screening> GetAll();

        // Tjekker om et tidsrum overlapper en eksisterende forestilling i samme
        // biograf/sal, jf. UC2 undtagelsesflow 6a
        bool HasOverlap(Cinema cinema, Hall hall, DateTime start, DateTime end);

        // Gemmer en ny forestilling i datakilden (både in-memory og til fil)
        void SaveScreening(Screening screening);

        // Gemmer alle ændringer til den fysiske fil (persistens)
        void SaveToFile();
    }
}
