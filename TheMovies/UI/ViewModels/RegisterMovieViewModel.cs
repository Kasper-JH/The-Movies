/*
 * Single Responsibility Principle (SRP):
 * Denne klasse har én primær opgave: at håndtere PRÆSENTATIONSLOGIKKEN for film-registrering og visning.
 * Den fungerer som mellemmand mellem View (WPF) og Model (Movie) + Repository.
 * 
 * ViewModel'en står for:
 * - At eksponere data til UI'et via properties (med INotifyPropertyChanged)
 * - At håndtere brugerens interaktioner via ICommand (Register, Next, Previous)
 * - At indeholde forretningsregler (f.eks. duplikat-tjek og validering)
 * - At kalde Repository til persistens
 * - At håndtere navigation gennem filmene (Next/Previous med indeks)
 * 
 * INotifyPropertyChanged er KUN implementeret her (i ViewModel) - ikke i Movie.
 * Dette sikrer en ren adskillelse: UI-notifikation hører til i præsentationslaget.
 * 
 * Ved opstart indlæses alle film fra repository (LoadMovies).
 * Navigation foregår ved at ændre _currentIndex og opdatere UI-properties.
 * Ved registrering opdateres listen og der navigeres til den nye film.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TheMovies.Core.Models;
using TheMovies.Core.Repositories;
using TheMovies.UI.Commands;

namespace TheMovies.UI.ViewModels
{
    public class RegisterMovieViewModel : INotifyPropertyChanged
    {
        // Repository til persistens (dependency injection via interfacet)
        private readonly IMovieRepository _repository;

        // Liste over alle film (bruges til navigation) - ALDRIG null!
        private List<Movie> _allMovies = new List<Movie>();

        // Aktuel indeks i listen (bruges til navigation)
        private int _currentIndex;

        // Backing fields til UI-binding
        private string _title = string.Empty;
        private int _duration;
        private string _genre = string.Empty;
        private string _statusMessage = string.Empty;

        // Constructor - initialiserer repository og indlæser data
        public RegisterMovieViewModel()
        {
            try
            {
                _repository = new FileMovieRepository();

                // PERSISTENS: Indlæs alle film ved opstart
                LoadMovies();

                // Opretter kommandoer til UI-handlinger
                RegisterMovieCommand = new RelayCommand(_ => RegisterMovie(), _ => CanRegister());
                NextMovieCommand = new RelayCommand(_ => NextMovie(), _ => HasNext());
                PreviousMovieCommand = new RelayCommand(_ => PreviousMovie(), _ => HasPrevious());
            }
            catch (Exception ex)
            {
                // Hvis noget går galt, sæt en fejlbesked og sørg for at UI'et ikke crasher
                _allMovies = new List<Movie>();
                StatusMessage = $"FEJL ved opstart: {ex.Message}";

                // Opretter dummy-kommandoer så UI'et ikke crasher
                RegisterMovieCommand = new RelayCommand(_ => { }, _ => false);
                NextMovieCommand = new RelayCommand(_ => { }, _ => false);
                PreviousMovieCommand = new RelayCommand(_ => { }, _ => false);
            }
        }

        // ========== UI-BINDING PROPERTIES ==========

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                    // Opdater "Register"-knappens aktivitet
                    ((RelayCommand)RegisterMovieCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public int Duration
        {
            get => _duration;
            set
            {
                if (_duration != value)
                {
                    _duration = value;
                    OnPropertyChanged();
                    ((RelayCommand)RegisterMovieCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string Genre
        {
            get => _genre;
            set
            {
                if (_genre != value)
                {
                    _genre = value;
                    OnPropertyChanged();
                    ((RelayCommand)RegisterMovieCommand)?.RaiseCanExecuteChanged();
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

        // ========== KOMMANDOER ==========

        public ICommand RegisterMovieCommand { get; private set; }
        public ICommand NextMovieCommand { get; private set; }
        public ICommand PreviousMovieCommand { get; private set; }

        // ========== VALIDERINGSLOGIK ==========

        // Bestemmer om "Register"-knappen er aktiv
        // Alle tre felter skal være udfyldt og Duration > 0
        private bool CanRegister()
        {
            return !string.IsNullOrWhiteSpace(Title) &&
                   Duration > 0 &&
                   !string.IsNullOrWhiteSpace(Genre);
        }

        // ========== FORRETNINGSLOGIK ==========

        // Hovedmetode: Registrerer en ny film
        // Kaldes når brugeren trykker på "Register Movie"-knappen
        private void RegisterMovie()
        {
            // Opretter en ny Movie
            var movie = new Movie
            {
                Title = Title,
                Duration = Duration,
                Genre = Genre
            };

            // *** EXCEPTION FLOW 4a: Tjek om filmen allerede findes i systemet ***
            // Tjekker om der allerede findes en film med præcis samme Title, Duration og Genre
            if (_repository.Exists(movie))
            {
                StatusMessage = "FEJL: Filmen findes allerede!";
                return;
            }

            // Gemmer filmen via repository (tilføj til liste + gem til JSON)
            _repository.Add(movie);
            _repository.SaveChanges();

            // Opdater den interne liste og sæt index til den nye film
            _allMovies = _repository.GetAll().ToList();
            _currentIndex = _allMovies.Count - 1;
            UpdateCurrentMovie();

            // Bekræft overfor brugeren
            StatusMessage = $"Filmen '{Title}' er nu registreret!";

            // Rydder felterne så brugeren kan indtaste næste film
            Title = string.Empty;
            Duration = 0;
            Genre = string.Empty;
        }

        // ========== NAVIGATIONSLOGIK ==========

        // Gå til næste film i listen
        private void NextMovie()
        {
            if (HasNext())
            {
                _currentIndex++;
                UpdateCurrentMovie();
                StatusMessage = $"Viser film {_currentIndex + 1} af {_allMovies.Count}";
            }
        }

        // Gå til forrige film i listen
        private void PreviousMovie()
        {
            if (HasPrevious())
            {
                _currentIndex--;
                UpdateCurrentMovie();
                StatusMessage = $"Viser film {_currentIndex + 1} af {_allMovies.Count}";
            }
        }

        // Tjek om der er en næste film (styrer "Next"-knappens aktivitet)
        private bool HasNext()
        {
            return _allMovies != null && _currentIndex < _allMovies.Count - 1;
        }

        // Tjek om der er en forrige film (styrer "Previous"-knappens aktivitet)
        private bool HasPrevious()
        {
            return _allMovies != null && _currentIndex > 0;
        }

        // ========== PERSISTENS VED OPSTART ==========

        // Henter alle film fra repository (bruges ved opstart)
        private void LoadMovies()
        {
            try
            {
                // Sikrer at vi aldrig får null fra GetAll()
                var movies = _repository?.GetAll();
                _allMovies = movies?.ToList() ?? new List<Movie>();
            }
            catch (Exception ex)
            {
                _allMovies = new List<Movie>();
                StatusMessage = $"Fejl ved indlæsning: {ex.Message}";
                return;
            }

            if (_allMovies.Any())
            {
                _currentIndex = 0;
                UpdateCurrentMovie();
                StatusMessage = $"Indlæste {_allMovies.Count} film fra fil";
            }
            else
            {
                // Ryd felterne hvis der ikke er nogen film
                Title = string.Empty;
                Duration = 0;
                Genre = string.Empty;
                StatusMessage = "Ingen film fundet - registrer en ny!";
            }

            // Opdater navigation-knapperne
            ((RelayCommand)NextMovieCommand)?.RaiseCanExecuteChanged();
            ((RelayCommand)PreviousMovieCommand)?.RaiseCanExecuteChanged();
        }

        // ========== UI OPDATERING ==========

        // Opdater UI'et med den aktuelle film (baseret på _currentIndex)
        private void UpdateCurrentMovie()
        {
            if (_allMovies != null && _allMovies.Any() && _currentIndex >= 0 && _currentIndex < _allMovies.Count)
            {
                var movie = _allMovies[_currentIndex];
                Title = movie.Title;
                Duration = movie.Duration;
                Genre = movie.Genre;
            }
            else
            {
                Title = string.Empty;
                Duration = 0;
                Genre = string.Empty;
            }

            // Opdater navigation-knapperne (aktiver/deaktiver)
            ((RelayCommand)NextMovieCommand)?.RaiseCanExecuteChanged();
            ((RelayCommand)PreviousMovieCommand)?.RaiseCanExecuteChanged();
        }

        // ========== INotifyPropertyChanged IMPLEMENTERING ==========

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}   // <-- HUSK LUKKENDE PARENTES! 