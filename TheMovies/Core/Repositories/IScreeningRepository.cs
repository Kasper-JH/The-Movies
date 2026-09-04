/*
 * SRP: Dette interface definerer de metoder, som et repository til forestillinger skal have.
 * Det gør det muligt at skifte mellem forskellige datakilder (f.eks. JSON, database) uden at ændre ViewModel.
 */

using System.Collections.Generic;
using TheMovies.Core.Models;

namespace TheMovies.Core.Repositories
{
    public interface IScreeningRepository
    {
        // Henter alle forestillinger fra datakilden
        IEnumerable<Screening> GetAll();

        // Tjekker om en kandidat-forestilling overlapper en eksisterende forestilling i samme
        // biograf/sal, jf. UC2 undtagelsesflow 6a. Selve overlap-reglen (hvad "overlap" betyder)
        // ligger på Screening.OverlapsWith() - repository'et står kun for at iterere de
        // eksisterende forestillinger og spørge hver af dem.
        bool HasOverlap(Screening candidate);

        // Gemmer en ny forestilling i datakilden (både in-memory og til fil)
        void SaveScreening(Screening screening);
    }
}
