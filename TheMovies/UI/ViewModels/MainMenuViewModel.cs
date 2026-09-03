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

        // MVVM-separation: ViewModel'en må ikke selv oprette Window-objekter eller kalde
        // ShowDialog() - det er en View-specifik detalje. I stedet bygger ViewModel'en den
        // færdige child-ViewModel (som er præsentationslogik, og derfor fint at kende til
        // herfra) og rejser et event. View'et (MainMenuView.xaml.cs) lytter på eventet og
        // står selv for at oprette det rigtige vindue og vise det.
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

        // Åbner vinduet til registrering af film (UC1).
        // ViewModel'en opretter kun den tilhørende ViewModel og rejser eventet - View'et
        // (code-behind) opretter selve Window'et og kalder ShowDialog(). Da ShowDialog()
        // er blokerende, er koden herefter (UpdateStatusMessage osv.) først med til at
        // køre når View'ets event-handler - og dermed selve dialogen - er færdig, ligesom
        // ved det oprindelige direkte kald.
        private void OpenRegisterView()
        {
            var viewModel = new RegisterMovieViewModel(_movieRepository);
            RegisterMovieRequested?.Invoke(this, viewModel);

            // Når vinduet lukkes, opdateres status – der kan nu være kommet en film
            UpdateStatusMessage();
            // Fortæl UI'et at "Opret forestilling"-knappens tilstand skal opdateres
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

            // Sæt seed data, se CinemaSeed.cs
            var cinemas = CinemaSeed.GetAll();
            var viewModel = new CreateScreeningViewModel(_movieRepository, _screeningRepository, cinemas);
            CreateScreeningRequested?.Invoke(this, viewModel);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}