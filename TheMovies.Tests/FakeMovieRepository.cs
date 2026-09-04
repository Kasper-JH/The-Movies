using TheMovies.Core.Models;
using TheMovies.Core.Repositories;

namespace TheMovies.Tests
{
    public class FakeMovieRepository : IMovieRepository
    {
        public List<Movie> Movies { get; } = new();

        public int SaveToFileCallCount { get; private set; }

        public IEnumerable<Movie> GetAll() => Movies;

        public void SaveMovie(Movie movie) => Movies.Add(movie);

        public bool IsMovieRegistered(Movie movie) =>
            Movies.Any(m => m.Title == movie.Title &&
                            m.Duration == movie.Duration &&
                            m.Genre == movie.Genre);

        public void UpdateMovie(Movie movie)
        {
            var existing = Movies.FirstOrDefault(m => m.Title == movie.Title);
            if (existing != null)
            {
                existing.Director = movie.Director;
                existing.PremiereDate = movie.PremiereDate;
            }
        }

        public void SaveToFile() => SaveToFileCallCount++;
    }
}
