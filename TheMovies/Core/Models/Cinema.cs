/*
 * SRP: Denne klasse repræsenterer en biograf. Biografer er fast, foruddefineret
 * data (jf. UC2's noter) og oprettes ikke via en selvstændig use case.
 */

using System.Collections.Generic;

namespace TheMovies.Core.Models
{
    public class Cinema
    {
        // Tom constructor til JSON-deserialisering
        public Cinema() { }

        public Cinema(string name)
        {
            Name = name;
            Halls = new List<Hall>();
        }

        public string Name { get; set; }
        public List<Hall> Halls { get; set; } = new List<Hall>();

        // Tilføjer en sal til biografen (bruges kun ved opsætning af faste data)
        public void AddHall(Hall hall)
        {
            Halls.Add(hall);
        }
    }
}
