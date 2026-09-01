/*
 * SRP: Denne statiske klasse indeholder den faste, foruddefinerede liste af biografer
 * og deres sale, jf. UC2's note om at biograferne (Hjerm, Videbæk, Thorsminde, Ræhr)
 * antages som fast data og ikke oprettes via en selvstændig use case.
 *
 */

using System.Collections.Generic;
using TheMovies.Core.Models;

namespace TheMovies.Core.Seed
{
    public static class CinemaSeed
    {
        public static IReadOnlyList<Cinema> GetAll()
        {
            // Faste biografer i systemet.
            var cinemas = new List<Cinema>();
            string[] cinemaNames = { "Hjerm", "Videbæk", "Thorsminde", "Ræhr" };

            foreach (var name in cinemaNames)
            {
                //Antallet af sale i hver biograf er fastlagt til
                //6 her, men kunne ændres efter behov/udbygges til dynamisk behov senere 
                //(sikkert lettere at opdatere gennem db, men scope creep for nuværende).
                var cinema = new Cinema(name);
                cinema.AddHall(new Hall(1));
                cinema.AddHall(new Hall(2));
                cinema.AddHall(new Hall(3));
                cinema.AddHall(new Hall(4));
                cinema.AddHall(new Hall(5));
                cinema.AddHall(new Hall(6));
                cinemas.Add(cinema);
            }

            return cinemas;
        }
    }
}
