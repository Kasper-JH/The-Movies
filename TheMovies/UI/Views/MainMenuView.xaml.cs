/*
 * SRP: Code-behind til startmenuen. DataContext sættes i App.xaml.cs.
 *
 * MVVM-separation: MainMenuViewModel kender ikke til konkrete View-typer og kalder aldrig
 * ShowDialog() selv - den rejser i stedet events (RegisterMovieRequested/CreateScreeningRequested)
 * med den færdige child-ViewModel som payload. Det er kun her i code-behind, at vi opretter det
 * rigtige Window, sætter DataContext og viser det. Det er den eneste "logik" i denne klasse.
 */
using System.Windows;
using TheMovies.UI.ViewModels;

namespace TheMovies.UI.Views
{
    public partial class MainMenuView : Window
    {
        public MainMenuView()
        {
            InitializeComponent();

            // DataContext sættes udefra (i App.xaml.cs) efter konstruktøren er kørt,
            // så vi abonnerer på ViewModel'ens events i Loaded, hvor DataContext med
            // sikkerhed er sat.
            Loaded += MainMenuView_Loaded;
        }

        private void MainMenuView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainMenuViewModel viewModel)
            {
                viewModel.RegisterMovieRequested += OnRegisterMovieRequested;
                viewModel.CreateScreeningRequested += OnCreateScreeningRequested;
            }
        }

        // Opretter og viser registreringsvinduet (UC1) når ViewModel'en beder om det.
        private void OnRegisterMovieRequested(object? sender, RegisterMovieViewModel viewModel)
        {
            var view = new RegisterMovieView { DataContext = viewModel };
            view.ShowDialog();
        }

        // Opretter og viser vinduet til oprettelse af forestilling (UC2) når ViewModel'en beder om det.
        private void OnCreateScreeningRequested(object? sender, CreateScreeningViewModel viewModel)
        {
            var view = new CreateScreeningView { DataContext = viewModel };
            view.ShowDialog();
        }
    }
}
