/*
 * SRP: Denne klasse repræsenterer en forestilling, hvilket svarer til en konkret visning af en
 * film i en given biograf, sal og på et givent tidspunkt (UC2).
 * 
 * Den samlede mængde af forestillinger, der oprettes via UC2 for en given 
 * biograf og måned, udgør biograf-programmet for den følgende måned, hvilket også 
 * svarer til det Excel-ark som Jens Peter tidligere udarbejdede manuelt (i scenarie 2)
 * (jf. "Øvrige noter"-sektionen i UC2).
 */

using System;

namespace TheMovies.Core.Models
{
    public class Screening
    {
        // 15 min reklamer + 15 min rengøring, jf. UC2 trin 7. Dette er den eneste kilde 
        // til denne værdi i programmet, så hvis den ændres, skal det kun gøres ét sted, her.
        // Derfor er den erklæret som const, så den er låst efter oprettelsen.
        // Fordi den er den eneste kilde til de 30 minutter, så er den også erklæret public, så den
        // kan tilgås fra andre klasser, der har brug for at kende denne værdi.
        public const int AdsAndCleaningMinutes = 30;

        // Tom constructor til JSON-deserialisering (når filen læses)
        public Screening() { }

        public Screening(Movie movie, Cinema cinema, Hall hall, DateTime startTime)
        {
            Movie = movie ?? throw new ArgumentNullException(nameof(movie));
            Cinema = cinema ?? throw new ArgumentNullException(nameof(cinema));
            Hall = hall ?? throw new ArgumentNullException(nameof(hall));
            // Jf. UC2 domænemodellen, så kan en biograf have mange sale
            // (Biograf "1" -- "*" Sal). Vores nuværende UI forhindrer, at man i
            // praksis kan vælge en sal, der ikke hører til den valgte biograf, men
            // reglen bør også være til stede her i model-laget, så dette ikke dukker op 
            // som en fejl i programmet, hvis man senere udskifter UI'et.
            if (!cinema.Halls.Contains(hall))
            {
                throw new ArgumentException(
                    $"Sal {hall.HallNumber} hører ikke til biografen '{cinema.Name}'.",
                    nameof(hall));
            }

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
