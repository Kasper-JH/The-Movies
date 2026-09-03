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

        // Properties for Movie, Cinema, Hall, StartTime, and EndTime som indkapsler (OOP) de relevante data for en forestilling.
        public Movie Movie { get; set; }
        public Cinema Cinema { get; set; }
        public Hall Hall { get; set; }
        public DateTime StartTime { get; set; }

        // EndTime er en 'derived attribute', dvs. vi beregner den i constructoren ovenfor (films varighed+30m). 
        // Har en public setter udelukkende for at JSON-deserialisering kan genskabe objektet fra fil.
        // Dette er potentielt et problem: Hvis man sætter EndTime til en værdi udefra (via public setter), der ikke stemmer overens med
        // StartTime + Movie.Duration + AdsAndCleaningMinutes, men er nødvendig for at kunne læse objektet fra fil.
        public DateTime EndTime { get; set; }

        // Domæneregel (UC2 undtagelsesflow 6a): To forestillinger overlapper, hvis de er i
        // samme biograf og sal, og deres tidsrum skærer hinanden. Dette er domænelogik, der hører
        // til i model-laget, og ikke i persistenslaget (repository), da det er en regel for, hvordan to
        // forestillinger relaterer sig til hinanden. Dog kræver denne sammenligning at vi også henter noget data at 
        // sammenligne med, hvilket er repositoriets ansvar. Derfor er det en kombination af model-lag (logik) og repository-lag (håndtering af data)
        public bool OverlapsWith(Screening other)
        {
            // Hvis vi ikke har en anden forestilling at sammenligne med, så kan de ikke overlappe.
            if (other == null)
                return false;

            // Sammenligner biografnavn og salnummer for at afgøre, om de er i samme biograf og sal.
            bool sameLocation = Cinema.Name == other.Cinema.Name &&
                                 Hall.HallNumber == other.Hall.HallNumber;
            // Hvis de ikke er i samme biograf og sal, kan de ikke overlappe.
            if (!sameLocation)
                return false;

            // To tidsrum overlapper, hvis det ene starter, før det andet slutter, og omvendt.
            return StartTime < other.EndTime && other.StartTime < EndTime;
        }
    }
}
