using TheMovies.Core.Models;

namespace TheMovies.Tests
{
	public class ScreeningTests
	{
		[Fact]
		public void OnNewScreening_WhenCalculatingRunningTime_IsEndTimeCorrect()
		{
			var movie = new Movie("Inception", 148, "Sci-Fi");
			var cinema = new Cinema("Odense");
			var hall = new Hall(1);
			var start = new DateTime(2026, 1, 1, 20, 0, 0);   // kl. 20:00

			var screening = new Screening(movie, cinema, hall, start);

			// 20:00 + 148 min + 30 min = 22:58
			Assert.Equal(new DateTime(2026, 1, 1, 22, 58, 0), screening.EndTime);
		}
	}
}