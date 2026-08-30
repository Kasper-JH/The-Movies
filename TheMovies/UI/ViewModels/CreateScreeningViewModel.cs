/*
 * SRP: Denne ViewModel står for præsentationslogikken til oprettelse af en
 * forestilling (UC2). Den modtager IMovieRepository, IScreeningRepository samt
 * den faste liste af biografer via constructor injection (Dependency Injection).
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        // Backing fields til properties
        private Movie _selectedMovie;
        private Cinema _selectedCinema;
        private Hall _selectedHall;
        private string _director = string.Empty;
        private DateTime? _premiereDate;
        private DateTime _screeningDate = DateTime.Today;
        private string _screeningTime = string.Empty;
        private string _statusMessage = string.Empty;

        // Kommando til oprettelse (gemmes som RelayCommand så vi kan kalde RaiseCanExecuteChanged)
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

        // Kollektioner til UI-binding (dropdowns)
        public ObservableCollection<Movie> AvailableMovies { get; }
        public IReadOnlyList<Cinema> AvailableCinemas => _cinemas;
        public ObservableCollection<Hall> AvailableHalls { get; }

        public Movie SelectedMovie
        {
            get => _selectedMovie;
            set
            {
                if (_selectedMovie != value)
                {
                    _selectedMovie = value;
                    OnPropertyChanged();

                    // Forudfylder instruktør/premieredato, hvis filmen allerede har
                    // fået dem sat ved en tidligere forestilling
                    Director = _selectedMovie?.Director ?? string.Empty;
                    PremiereDate = _selectedMovie?.PremiereDate;

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
                    _createScreeningCommand.RaiseCanExecuteChanged();
                }
            }
        }

        // Klokkeslæt indtastes som tekst i formatet "TT:MM", f.eks. "20:30"
        public string ScreeningTime
        {
            get => _screeningTime;
            set
            {
                if (_screeningTime != value)
                {
                    _screeningTime = value;
                    OnPropertyChanged();
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

        // Eksponerer kommandoen til UI (binding)
        public ICommand CreateScreeningCommand => _createScreeningCommand;

        // Validering: knappen er kun aktiv når alle felter er udfyldt korrekt
        private bool CanCreateScreening()
        {
            if (SelectedMovie == null || SelectedCinema == null || SelectedHall == null)
                return false;

            if (string.IsNullOrWhiteSpace(Director) || PremiereDate == null)
                return false;

            return TimeSpan.TryParse(ScreeningTime, out _);
        }

        // Hovedmetode: opretter en ny forestilling (UC2 trin 3-8)
        private void CreateScreening()
        {
            if (!TimeSpan.TryParse(ScreeningTime, out var time))
            {
                StatusMessage = "FEJL: Klokkeslættet er ikke gyldigt (brug formatet TT:MM).";
                return;
            }

            var startTime = ScreeningDate.Date + time;

            // Screening-konstruktoren beregner selv sluttidspunktet (UC2 trin 7)
            var tentativeScreening = new Screening(SelectedMovie, SelectedCinema, SelectedHall, startTime);

            // UC2 undtagelsesflow 5a: tjek for overlap, før forestillingen oprettes
            if (_screeningRepository.HasOverlap(SelectedCinema, SelectedHall, tentativeScreening.StartTime, tentativeScreening.EndTime))
            {
                StatusMessage = "FEJL: Tidspunktet er optaget i den valgte sal - vælg et nyt tidspunkt.";
                return;
            }

            // Filmen beriges med instruktør og premieredato (UC2 trin 3-4)
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

            if (SelectedCinema == null)
                return;

            foreach (var hall in SelectedCinema.Halls)
            {
                AvailableHalls.Add(hall);
            }
        }

        // Indlæser registrerede film ved opstart
        private void LoadMovies()
        {
            AvailableMovies.Clear();

            try
            {
                var movies = _movieRepository?.GetAll();
                if (movies != null)
                {
                    foreach (var movie in movies)
                    {
                        AvailableMovies.Add(movie);
                    }
                }

                StatusMessage = AvailableMovies.Count > 0
                    ? $"Indlæste {AvailableMovies.Count} film fra fil"
                    : "Ingen film fundet - registrer en film først (UC1)!";
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
