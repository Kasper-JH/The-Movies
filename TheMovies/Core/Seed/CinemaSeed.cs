/*
 * SRP: Denne statiske klasse indeholder den faste, foruddefinerede liste af biografer
 * og deres sale, jf. UC2's note om at biograferne (Hjerm, Videbæk, Thorsminde, Ræhr)
 * antages som fast data og ikke oprettes via en selvstændig use case.
 *
 * OBS: Antallet af sale pr. biograf (2) er sat som et eksempel her, da use casen
 * ikke angiver et præcist tal - juster efter behov.
 */

using System.Collections.Generic;
using TheMovies.Core.Models;

namespace TheMovies.Core.Seed
{
    public static class CinemaSeed
    {
        public static IReadOnlyList<Cinema> GetAll()
        {
            var cinemas = new List<Cinema>();
            string[] cinemaNames = { "Hjerm", "Videbæk", "Thorsminde", "Ræhr" };

            foreach (var name in cinemaNames)
            {
                var cinema = new Cinema(name);
                cinema.AddHall(new Hall(1));
                cinema.AddHall(new Hall(2));
                cinemas.Add(cinema);
            }

            return cinemas;
        }
    }
}
