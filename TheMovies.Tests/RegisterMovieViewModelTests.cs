using TheMovies.Core.Models;
using TheMovies.UI.ViewModels;

namespace TheMovies.Tests
{
    public class RegisterMovieViewModelTests
    {
        private static RegisterMovieViewModel CreateViewModel(
            FakeMovieRepository repository,
            string title = "Inception",
            int duration = 148,
            string genre = "Sci-Fi")
        {
            return new RegisterMovieViewModel(repository)
            {
                Title = title,
                Duration = duration,
                Genre = genre
            };
        }


        [Fact]
        public void RegisterMovie_GemmerFilmen_NaarFelterErUdfyldt()
        {
            var repository = new FakeMovieRepository();
            var viewModel = CreateViewModel(repository);

            viewModel.RegisterMovieCommand.Execute(null);

            var saved = Assert.Single(repository.Movies);
            Assert.Equal("Inception", saved.Title);
            Assert.Equal(148, saved.Duration);
            Assert.Equal("Sci-Fi", saved.Genre);
        }

        [Fact]
        public void RegisterMovie_RydderFelterne_EfterGemning()
        {
            var repository = new FakeMovieRepository();
            var viewModel = CreateViewModel(repository);

            viewModel.RegisterMovieCommand.Execute(null);

            Assert.Equal(string.Empty, viewModel.Title);
            Assert.Equal(0, viewModel.Duration);
            Assert.Equal(string.Empty, viewModel.Genre);
            Assert.Contains("Inception", viewModel.StatusMessage);
        }


        [Fact]
        public void RegisterMovie_GemmerIkkeDublet_NaarFilmenFindes()
        {
            var repository = new FakeMovieRepository();
            repository.Movies.Add(new Movie("Inception", 148, "Sci-Fi"));
            var viewModel = CreateViewModel(repository);

            viewModel.RegisterMovieCommand.Execute(null);

            Assert.Single(repository.Movies);
            Assert.StartsWith("FEJL", viewModel.StatusMessage);
        }

        [Theory]
        [InlineData("", 148, "Sci-Fi")]      // manglende titel
        [InlineData("   ", 148, "Sci-Fi")]   // kun mellemrum
        [InlineData("Inception", 0, "Sci-Fi")]   // varighed 0
        [InlineData("Inception", -5, "Sci-Fi")]  // negativ varighed
        [InlineData("Inception", 148, "")]       // manglende genre
        public void CanExecute_ErFalse_NaarInputErUgyldigt(string title, int duration, string genre)
        {
            var viewModel = CreateViewModel(new FakeMovieRepository(), title, duration, genre);

            Assert.False(viewModel.RegisterMovieCommand.CanExecute(null));
        }

        [Fact]
        public void CanExecute_ErTrue_NaarAlleFelterErGyldige()
        {
            var viewModel = CreateViewModel(new FakeMovieRepository());

            Assert.True(viewModel.RegisterMovieCommand.CanExecute(null));
        }

        [Fact]
        public void Constructor_KasterArgumentNullException_NaarRepositoryErNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RegisterMovieViewModel(null!));
        }

        [Fact]
        public void Constructor_VisserAntalIndlaesteFilm()
        {
            var repository = new FakeMovieRepository();
            repository.Movies.Add(new Movie("Inception", 148, "Sci-Fi"));
            repository.Movies.Add(new Movie("Dune", 155, "Sci-Fi"));

            var viewModel = new RegisterMovieViewModel(repository);

            Assert.Contains("2", viewModel.StatusMessage);
        }


        [Fact]
        public void Title_RejserPropertyChanged()
        {
            var viewModel = new RegisterMovieViewModel(new FakeMovieRepository());
            var raised = new List<string?>();
            viewModel.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            viewModel.Title = "Dune";

            Assert.Contains(nameof(viewModel.Title), raised);
        }
    }
}
