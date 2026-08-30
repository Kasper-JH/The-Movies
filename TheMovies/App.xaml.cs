/*
 * SRP: Denne klasse står for opstart af applikationen (composition root).
 * Her oprettes repositories og ViewModels, og de sættes sammen via constructor injection.
 *
 * OBS: Der er endnu ikke lavet navigation mellem UC1 og UC2's vinduer - begge
 * vinduer åbnes blot samtidigt her, indtil I beslutter jer for en navigationsløsning.
 */

using System;
using System.Windows;
using TheMovies.Core.Repositories;
using TheMovies.Core.Seed;
using TheMovies.UI.ViewModels;
using TheMovies.UI.Views;

namespace TheMovies
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            IMovieRepository movieRepository;
            IScreeningRepository screeningRepository;

            // Opret repositories (konkrete implementeringer).
            // Kan kaste InvalidOperationException hvis movies.json/screenings.json er korrupt.
            try
            {
                movieRepository = new FileMovieRepository();
                screeningRepository = new FileScreeningRepository();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Kunne ikke starte: {ex.Message}", "Fejl ved opstart",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            // Faste biografer/sale (statisk seed data, jf. UC2's noter)
            var cinemas = CinemaSeed.GetAll();

            // UC1 - Registrer film
            var registerMovieViewModel = new RegisterMovieViewModel(movieRepository);
            var mainWindow = new MainWindow
            {
                DataContext = registerMovieViewModel
            };
            mainWindow.Show();

            // UC2 - Opret forestilling
            var createScreeningViewModel = new CreateScreeningViewModel(movieRepository, screeningRepository, cinemas);
            var createScreeningWindow = new CreateScreeningView
            {
                DataContext = createScreeningViewModel
            };
            createScreeningWindow.Show();
        }
    }
}
