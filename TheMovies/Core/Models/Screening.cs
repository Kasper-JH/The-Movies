/*
 * SRP: Denne klasse repræsenterer en forestilling - en konkret visning af en
 * film i en given biograf, sal og på et givent tidspunkt.
 *
 * Sporbarhed: EndTime er en afledt attribut jf. UC2-domænemodellens note
 * ("/slutTidspunkt er en afledt attribut, beregnet jf. trin 7 i UC2"),
 * og beregningen implementerer præcis UC2 trin 7:
 * "Systemet beregner forestillingens sluttidspunkt ud fra filmens varighed
 * samt 15 minutters reklamer og 15 minutters rengøring."
 */

using System;

namespace TheMovies.Core.Models
{
    public class Screening
    {
        // 15 min reklamer + 15 min rengøring, jf. UC2 trin 7
        private const int AdsAndCleaningMinutes = 30;

        // Tom constructor til JSON-deserialisering (når filen læses)
        public Screening() { }

        public Screening(Movie movie, Cinema cinema, Hall hall, DateTime startTime)
        {
            Movie = movie ?? throw new ArgumentNullException(nameof(movie));
            Cinema = cinema ?? throw new ArgumentNullException(nameof(cinema));
            Hall = hall ?? throw new ArgumentNullException(nameof(hall));
            StartTime = startTime;

            // UC2 trin 7: sluttidspunkt = starttidspunkt + filmens varighed + reklamer/rengøring
            EndTime = StartTime.AddMinutes(Movie.Duration + AdsAndCleaningMinutes);
        }

        public Movie Movie { get; set; }
        public Cinema Cinema { get; set; }
        public Hall Hall { get; set; }
        public DateTime StartTime { get; set; }

        // Afledt attribut - beregnes i constructoren, sættes aldrig direkte udefra i normal brug.
        // Har en public setter udelukkende for at JSON-deserialisering kan genskabe objektet fra fil.
        public DateTime EndTime { get; set; }
    }
}
