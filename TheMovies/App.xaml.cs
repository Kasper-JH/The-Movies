/*
 * SRP: Denne klasse står for opstart af applikationen (composition root).
 * Her oprettes repositories (så vi kan arbejde med vores data) og 
 * startmenuen (MainMenu) med tilhørende ViewModel.
 * 
 */
using System;
using System.Windows;
using TheMovies.Core.Repositories;
using TheMovies.UI.ViewModels;
using TheMovies.UI.Views;

namespace TheMovies
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Opret repositories til persistens (JSON-filer)
                var movieRepository = new FileMovieRepository();
                var screeningRepository = new FileScreeningRepository();

                // Opret startmenu med tilhørende ViewModel
                var mainMenuView = new MainMenuView();
                mainMenuView.DataContext = new MainMenuViewModel(movieRepository, screeningRepository);
                mainMenuView.Show();
            }
            catch (InvalidOperationException ex)
            {
                // Hvis filerne er korrupte eller utilgængelige, vises en fejl og programmet lukker
                MessageBox.Show($"Kunne ikke starte: {ex.Message}", "Fejl ved opstart",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}