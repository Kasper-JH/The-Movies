/*
 * SRP: Denne klasse står for opstart af applikationen (composition root).
 * Her oprettes repository og ViewModel, og de sættes sammen via constructor injection.
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

            // Opret repository (konkret implementering).
            // Kan kaste InvalidOperationException hvis movies.json er korrupt.
            IMovieRepository repository;
            try
            {
                repository = new FileMovieRepository();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Kunne ikke starte: {ex.Message}", "Fejl ved opstart",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            // Injicer repository i ViewModel
            var viewModel = new RegisterMovieViewModel(repository);

            // Opret vindue og sæt DataContext
            var mainWindow = new MainWindow
            {
                DataContext = viewModel
            };

            mainWindow.Show();
        }
    }
}