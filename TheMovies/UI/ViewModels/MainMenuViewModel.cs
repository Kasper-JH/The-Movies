/*
 * SRP: Denne ViewModel styrer præsentationslogikken for startmenuen.
 * Den håndterer preconditionen for UC2 (dvs. at der findes mindst én film)
 * og åbner de relevante vinduer via kommandoer.
 * 
 */
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TheMovies.Core.Repositories;
using TheMovies.Core.Seed;
using TheMovies.UI.Commands;
using TheMovies.UI.Views;

namespace TheMovies.UI.ViewModels
{
    public class MainMenuViewModel : INotifyPropertyChanged
    {
        private readonly IMovieRepository _movieRepository;
        private readonly IScreeningRepository _screeningRepository;
        private string _statusMessage;

        public MainMenuViewModel(IMovieRepository movieRepository, IScreeningRepository screeningRepository)
        {
            _movieRepository = movieRepository ?? throw new ArgumentNullException(nameof(movieRepository));
            _screeningRepository = screeningRepository ?? throw new ArgumentNullException(nameof(screeningRepository));

            // Opret kommandoer
            RegisterMovieCommand = new RelayCommand(_ => OpenRegisterView());
            // Opretter kommandoen til "Opret forestilling"-knappen: udfører OpenCreateScreeningView() når der
            // trykkes, og deaktiverer knappen hvis HasMovies() er false.
            CreateScreeningCommand = new RelayCommand(_ => OpenCreateScreeningView(), _ => HasMovies());

            // Opdater statusbesked til View ved opstart
            UpdateStatusMessage();
        }

        public ICommand RegisterMovieCommand { get; }
        public ICommand CreateScreeningCommand { get; }

        public string StatusMessage
        {
            get => _statusMessage;
            private set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        // Tjekker preconditionen for UC2: Der skal findes mindst én registreret film
        private bool HasMovies()
        {
            try
            {
                // Prøv at hente alle film fra repository'et.
                // Hvis der opstår en fejl (f.eks. filen er korrupt), fanges den
                // og vi returnerer false.
                return _movieRepository.GetAll()?.Any() == true;
            }
            catch
            {
                return false; // Ved fejl antager vi, at der ikke er film
            }
        }

        // Opdaterer statusbeskeden (bruges efter UC1 er lukket)
        private void UpdateStatusMessage()
        {
            StatusMessage = HasMovies()
                ? "Der er registreret film – du kan oprette forestillinger."
                : "Ingen film registreret – registrer en film først!";
        }

        // Åbner vinduet til registrering af film (UC1)
        private void OpenRegisterView()
        {
            var view = new RegisterMovieView();
            view.DataContext = new RegisterMovieViewModel(_movieRepository);
            view.ShowDialog();

            // Når vinduet lukkes, opdateres status – der kan nu være kommet en film
            UpdateStatusMessage();
            // Fortæl UI'et at "Opret forestilling"-knappens tilstand skal opdateres
            ((RelayCommand)CreateScreeningCommand).RaiseCanExecuteChanged();
        }

        // Åbner vinduet til oprettelse af forestilling (UC2)
        private void OpenCreateScreeningView()
        {
            if (!HasMovies())
            {
                // Dette burde aldrig ske, da knappen er deaktiveret, men for sikkerheds skyld:
                StatusMessage = "FEJL: Der er ingen film at oprette forestilling for.";
                return;
            }

            // Sæt seed data, se CinemaSeed.cs
            var cinemas = CinemaSeed.GetAll();
            var view = new CreateScreeningView();
            view.DataContext = new CreateScreeningViewModel(_movieRepository, _screeningRepository, cinemas);
            view.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}