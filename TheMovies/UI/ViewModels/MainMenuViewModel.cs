/*
 * SRP: Denne ViewModel styrer præsentationslogikken for startmenuen.
 * Den håndterer preconditionen for UC2 (dvs. tjekker at der findes mindst én film)
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

namespace TheMovies.UI.ViewModels
{
    public class MainMenuViewModel : INotifyPropertyChanged
    {
        // Her gemmer vi de repositories, som vi får tilsendt via constructor injection.
        // Vi opretter dem ikke selv her via keywordet New. Det gør App.xaml.cs (composition root).
        private readonly IMovieRepository _movieRepository;
        private readonly IScreeningRepository _screeningRepository;
        private string _statusMessage;

        public MainMenuViewModel(IMovieRepository movieRepository, IScreeningRepository screeningRepository)
        {
            // Hvis repository er null, så kastes en ArgumentNullException, hvilket sikrer at vi ikke får en null-reference senere.
            _movieRepository = movieRepository ?? throw new ArgumentNullException(nameof(movieRepository));
            _screeningRepository = screeningRepository ?? throw new ArgumentNullException(nameof(screeningRepository));

            // Opret kommandoer
            RegisterMovieCommand = new RelayCommand(_ => OpenRegisterView());
            // "Opret forestilling"-knappen er kun aktiv, hvis der findes mindst én film (HasMovies() returnerer true).
            CreateScreeningCommand = new RelayCommand(_ => OpenCreateScreeningView(), _ => HasMovies());

            // Opdater statusbesked til View ved opstart
            UpdateStatusMessage();
        }

        // MVVM: ViewModel'en opretter IKKE vinduerne selv, da det er View'ets job.
        // I stedet "råber" ViewModel'en op via events, og så lytter View'et (MainMenuView.xaml.cs)
        // efter og opretter det rigtige vindue.
        public event EventHandler<RegisterMovieViewModel>? RegisterMovieRequested;
        public event EventHandler<CreateScreeningViewModel>? CreateScreeningRequested;

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
                    OnPropertyChanged();// Fortæller UI'et at teksten skal opdateres
                }
            }
        }

        // Tjekker preconditionen for UC2: Der skal findes mindst én registreret film.
        private bool HasMovies()
        {
            try
            {
                // Vi prøver at se om der nogen film at hente fra repository? Hvis ja, returneres true og UC2 kan også køre.
                return _movieRepository.GetAll().Any() == true;
            }
            catch
            {
                return false; // Ved fejl antager vi, at der ikke er film.
            }
        }

        // Opdaterer statusbeskeden (bruges efter UC1 er kørt)
        private void UpdateStatusMessage()
        {
            // Hvis der er registreret film, så informerer vi brugeren om dette, men ellers
            // informerer vi om at der ikke er registreret film endnu og instruerer brugeren
            // i hvad de så skal gøre.
            StatusMessage = HasMovies()
                ? "Der er registreret film – du kan oprette forestillinger."
                : "Ingen film registreret – registrer en film først!";
        }

        // Åbner vinduet til registrering af film (UC1).
        private void OpenRegisterView()
        {
            // Opret ViewModellen til registreringsvinduet og send repository med.
            var viewModel = new RegisterMovieViewModel(_movieRepository);
            // Vi råber op om dette og Viewet (MainMenuView.xaml.cs) fanger dette og viser vinduet.
            RegisterMovieRequested?.Invoke(this, viewModel);

            // Når vinduet lukkes, opdateres status – der kan nu være kommet en film.
            UpdateStatusMessage();
            // Fortæl knappen at den skal tjekke om den kan aktiveres (der er nu måske film).
            ((RelayCommand)CreateScreeningCommand).RaiseCanExecuteChanged();
        }

        // Åbner vinduet til oprettelse af forestilling (UC2). Samme mønster som ovenfor.
        private void OpenCreateScreeningView()
        {
            if (!HasMovies())
            {
                // Dette burde aldrig ske, da knappen er deaktiveret, men for sikkerheds skyld:
                StatusMessage = "FEJL: Der er ingen film at oprette forestilling for.";
                return;
            }

            // Hent de faste biografer og sale (data fra CinemaSeed.cs)
            var cinemas = CinemaSeed.GetAll();
            var viewModel = new CreateScreeningViewModel(_movieRepository, _screeningRepository, cinemas);
            CreateScreeningRequested?.Invoke(this, viewModel);
        }

        // INotifyPropertyChanged - sørger for at UI'et opdateres når properties ændres.
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}