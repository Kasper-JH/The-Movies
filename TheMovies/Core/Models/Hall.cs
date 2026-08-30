/*
 * SRP: Denne klasse repræsenterer en sal i en biograf.
 */

namespace TheMovies.Core.Models
{
    public class Hall
    {
        // Tom constructor til JSON-deserialisering
        public Hall() { }

        public Hall(int hallNumber)
        {
            HallNumber = hallNumber;
        }

        public int HallNumber { get; set; }
    }
}
