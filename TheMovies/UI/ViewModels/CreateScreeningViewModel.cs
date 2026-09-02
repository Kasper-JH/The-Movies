/*
 * SRP: Denne ViewModel står for præsentationslogikken til oprettelse af en
 * forestilling (UC2). Den modtager IMovieRepository, IScreeningRepository samt
 * den faste liste af biografer via constructor injection (Dependency Injection).
 */
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TheMovies.Core.Models;
using TheMovies.Core.Repositories;
using TheMovies.UI.Commands;

namespace TheMovies.UI.ViewModels
{
    public class CreateScreeningViewModel : INotifyPropertyChanged
    {
        // Repositories og fast data (injected via constructor)
        private readonly IMovieRepository _movieRepository;
        private readonly IScreeningRepository _screeningRepository;
        private readonly IReadOnlyList<Cinema> _cinemas;

        // Backing fields
        private Movie _selectedMovie;
        private Cinema _selectedCinema;
        private Hall _selectedHall;
        private string _director = string.Empty;
        private DateTime? _premiereDate;
        private DateTime _screeningDate = DateTime.Today;
        private string _screeningTime = string.Empty;
        private string _statusMessage = string.Empty;
        private string _calculatedEndTime = string.Empty;

        private readonly RelayCommand _createScreeningCommand;

        public CreateScreeningViewModel(
            IMovieRepository movieRepository,
            IScreeningRepository screeningRepository,
            IReadOnlyList<Cinema> cinemas)
        {
            _movieRepository = movieRepository ?? throw new ArgumentNullException(nameof(movieRepository));
            _screeningRepository = screeningRepository ?? throw new ArgumentNullException(nameof(screeningRepository));
            _cinemas = cinemas ?? throw new ArgumentNullException(nameof(cinemas));

            _createScreeningCommand = new RelayCommand(_ => CreateScreening(), _ => CanCreateScreening());

            AvailableMovies = new ObservableCollection<Movie>();
            AvailableHalls = new ObservableCollection<Hall>();

            LoadMovies();
        }

        // Kollektioner til UI-binding
        public ObservableCollection<Movie> AvailableMovies { get; }
        public IReadOnlyList<Cinema> AvailableCinemas => _cinemas;
        public ObservableCollection<Hall> AvailableHalls { get; }

        // --- Properties med notifikation ---

        public Movie SelectedMovie
        {
            get => _selectedMovie;
            set
            {
                if (_selectedMovie != value)
                {
                    _selectedMovie = value;
                    OnPropertyChanged();
                    // Forudfyld instruktør og premieredato, hvis filmen allerede har dem
                    // Null-coalescing operator (??): Enten har SelectedMovie en Director som assignes, ellers sæt til tom streng
                    Director = _selectedMovie?.Director ?? string.Empty;
                    // Sætter premieredatoen fra den valgte film – eller null hvis ingen film er valgt.
                    PremiereDate = _selectedMovie?.PremiereDate;
                    UpdateCalculatedEndTime(); // Opdater sluttidspunkt
                    _createScreeningCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public Cinema SelectedCinema
        {
            get => _selectedCinema;
            set
            {
                if (_selectedCinema != value)
                {
                    _selectedCinema = value;
                    OnPropertyChanged();
                    RefreshAvailableHalls();
                    _createScreeningCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public Hall SelectedHall
        {
            get => _selectedHall;
            set
            {
                if (_selectedHall != value)
                {
                    _selectedHall = value;
                    OnPropertyChanged();
                    _createScreeningCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string Director
        {
            get => _director;
            set
            {
                if (_director != value)
                {
                    _director = value;
                    OnPropertyChanged();
                    _createScreeningCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public DateTime? PremiereDate
        {
            get => _premiereDate;
            set
            {
                if (_premiereDate != value)
                {
                    _premiereDate = value;
                    OnPropertyChanged();
                    _createScreeningCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public DateTime ScreeningDate
        {
            get => _screeningDate;
            set
            {
                if (_screeningDate != value)
                {
                    _screeningDate = value;
                    OnPropertyChanged();
                    UpdateCalculatedEndTime();
                    _createScreeningCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string ScreeningTime
        {
            get => _screeningTime;
            set
            {
                if (_screeningTime != value)
                {
                    _screeningTime = value;
                    OnPropertyChanged();
                    UpdateCalculatedEndTime();
                    _createScreeningCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        // Viser det beregnede sluttidspunkt – jf. UC2 trin 7
        public string CalculatedEndTime
        {
            get => _calculatedEndTime;
            private set
            {
                if (_calculatedEndTime != value)
                {
                    _calculatedEndTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand CreateScreeningCommand => _createScreeningCommand;

        // --- Hjælpemetoder ---

        /// <summary>
        /// Forsøger at parse klokkeslættet med streng validering.
        /// Tillader kun formaterne "H:mm" og "HH:mm".
        /// Minutter valideres implicit via formatstrengen "mm" (0-59).
        /// Timer tjekkes eksplicit (0-23), da TryParseExact ikke begrænser timer.
        /// </summary>
        /// <param name="time">Parset TimeSpan ved succes</param>
        /// <param name="errorMessage">Fejlbesked ved fiasko</param>
        /// <returns>True hvis gyldigt, ellers false</returns>
        private bool TryParseScreeningTime(out TimeSpan time, out string errorMessage)
        {
            errorMessage = string.Empty;
            time = TimeSpan.Zero;

            if (string.IsNullOrWhiteSpace(ScreeningTime))
            {
                errorMessage = "Klokkeslættet må ikke være tomt.";
                return false;
            }

            // Tillad både "H:mm" (f.eks. 9:30) og "HH:mm" (f.eks. 14:00)
            // Brug CultureInfo.InvariantCulture for at undgå problemer med komma vs. punktum
            if (!TimeSpan.TryParseExact(ScreeningTime.Trim(), new[] { "h\\:mm", "hh\\:mm" },
                                        CultureInfo.InvariantCulture, out time))
            {
                errorMessage = "Ugyldigt format – brug TT:MM (f.eks. 9:30 eller 14:00).";
                return false;
            }

            // Timer er IKKE implicit begrænset af TryParseExact – vi tjekker dem selv
            if (time.Hours < 0 || time.Hours > 23)
            {
                errorMessage = "Timer skal være mellem 0 og 23.";
                return false;
            }

            // Minutter er implicit valideret via formatstrengen "mm", så intet yderligere tjek nødvendigt.

            return true;
        }

        // Opdaterer den viste beregning af sluttidspunkt (UC2 trin 7: varighed + 30 min)
        private void UpdateCalculatedEndTime()
        {
            // Hvis klokkeslættet er ugyldigt, viser vi en vejledende besked i stedet for en fejl
            if (SelectedMovie == null || string.IsNullOrWhiteSpace(ScreeningTime) || !TryParseScreeningTime(out var time, out _))
            {
                CalculatedEndTime = "Udfyld film og tid (TT:MM) for at se beregning";
                return;
            }

            var start = ScreeningDate.Date + time;
            // Genbruger Screening.AdsAndCleaningMinutes (15 min reklamer + 15 min rengøring, UC2 trin 7)
            // i stedet for at duplikere tallet 30 her (Single Source of Truth).
            var end = start.AddMinutes(SelectedMovie.Duration + Screening.AdsAndCleaningMinutes);
            CalculatedEndTime = end.ToString("dd/MM/yyyy HH:mm");
        }

        // Validering: knappen er kun aktiv når alle felter er udfyldt korrekt
        private bool CanCreateScreening()
        {
            if (SelectedMovie == null || SelectedCinema == null || SelectedHall == null)
                return false;
            if (string.IsNullOrWhiteSpace(Director) || PremiereDate == null)
                return false;
            // Tjek at klokkeslættet er gyldigt – ellers er knappen deaktiveret
            return TryParseScreeningTime(out _, out _);
        }

        // Hovedmetode: opretter en ny forestilling (UC2)
        private void CreateScreening()
        {
            // Håndtering af ugyldigt klokkeslæt – giver en venlig fejlbesked
            if (!TryParseScreeningTime(out var time, out var parseError))
            {
                StatusMessage = $"FEJL: {parseError}";
                return;
            }

            var startTime = ScreeningDate.Date + time;

            // Filmen må ikke vises før premieredatoen
            // Note: Scenarierne specificerer intet om at pre-screenings kunne være en ting, f.eks. 
            // en forestilling oprettet i systemet før premieredatoen kun for ansatte eller andet
            // i denne stil. Derfor dette tjek, men skulle måske ændres hvis pre-screenings blev
            // efterspurgt af kunden? 
            if (PremiereDate.HasValue && startTime.Date < PremiereDate.Value.Date)
            {
                StatusMessage = $"FEJL: Spilletidspunktet ({startTime:dd/MM/yyyy}) er før premieredatoen ({PremiereDate.Value:dd/MM/yyyy}). Vælg en senere dato.";
                return;
            }

            // Screening-konstruktoren beregner selv sluttidspunktet (UC2 trin 7) og validerer
            // desuden at den valgte sal hører til den valgte biograf. Sidstnævnte bør i praksis
            // aldrig fejle her, da RefreshAvailableHalls() kun viser sale fra den valgte biograf -
            // men fanges alligevel defensivt, så en fremtidig fejl i den logik giver en pæn
            // besked i stedet for en ufanget exception.
            Screening tentativeScreening;
            try
            {
                tentativeScreening = new Screening(SelectedMovie, SelectedCinema, SelectedHall, startTime);
            }
            catch (ArgumentException ex)
            {
                StatusMessage = $"FEJL: {ex.Message}";
                return;
            }

            // UC2 undtagelsesflow 6a: tjek for overlap, før forestillingen oprettes
            if (_screeningRepository.HasOverlap(SelectedCinema, SelectedHall, tentativeScreening.StartTime, tentativeScreening.EndTime))
            {
                StatusMessage = "FEJL: Tidspunktet er optaget i den valgte sal – vælg et nyt tidspunkt.";
                return;
            }

            // Film-objektet får yderligere data mht. instruktør og premieredato (UC2 trin 4-5)
            SelectedMovie.Director = Director;
            SelectedMovie.PremiereDate = PremiereDate;

            try
            {
                _movieRepository.UpdateMovie(SelectedMovie);
                _screeningRepository.SaveScreening(tentativeScreening);
            }
            catch (Exception ex)
            {
                StatusMessage = $"FEJL: Forestillingen kunne ikke gemmes: {ex.Message}";
                return;
            }

            // Succes: opdater status (UC2 trin 8)
            StatusMessage = $"Forestillingen for '{SelectedMovie.Title}' er nu oprettet!";
        }

        // Opdaterer listen af sale, når der vælges en ny biograf
        private void RefreshAvailableHalls()
        {
            AvailableHalls.Clear();
            if (SelectedCinema == null) return;
            foreach (var hall in SelectedCinema.Halls)
                AvailableHalls.Add(hall);
        }

        // Indlæser registrerede film ved opstart
        private void LoadMovies()
        {
            AvailableMovies.Clear();
            try
            {
                // Prøv at hente alle film fra repository'et. Hvis der opstår en fejl
                // (f.eks. filen er korrupt), fanges den og vi viser en venlig fejlbesked.
                var movies = _movieRepository?.GetAll();
                if (movies != null)
                    foreach (var movie in movies)
                        AvailableMovies.Add(movie);

                StatusMessage = AvailableMovies.Count > 0
                    ? $"Indlæste {AvailableMovies.Count} film fra fil"
                    : "Ingen film fundet – registrer en film først!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Fejl ved indlæsning: {ex.Message}";
            }
        }

        // INotifyPropertyChanged implementering
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}