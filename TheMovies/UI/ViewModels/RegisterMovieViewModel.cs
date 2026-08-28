/*
 * SRP: Denne ViewModel står for præsentationslogikken til registrering af film.
 * Den modtager en IMovieRepository via constructor injection (Dependency Injection).
 * Den indeholder properties til UI-binding, kommandoer til registrering, og validering.
 */

using System;
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
        // Repository til at gemme/hente film (injected via constructor)
        private readonly IMovieRepository _repository;

        // Backing fields til properties
        private string _title = string.Empty;
        private int _duration;
        private string _genre = string.Empty;
        private string _statusMessage = string.Empty;

        // Kommando til registrering (gemmes som RelayCommand så vi kan kalde RaiseCanExecuteChanged)
        private readonly RelayCommand _registerMovieCommand;

        // Constructor: modtager repository udefra (dependency injection)
        public RegisterMovieViewModel(IMovieRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));

            // Opretter kommandoen: ved udførsel kaldes RegisterMovie(), og CanRegister() bestemmer om knappen er aktiv
            _registerMovieCommand = new RelayCommand(_ => RegisterMovie(), _ => CanRegister());

            // Ved opstart indlæses film (for at tælle hvor mange der findes)
            LoadMovies();
        }

        // Properties som UI'et binder til
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                    _registerMovieCommand.RaiseCanExecuteChanged(); // Opdater knap-status
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
                    _registerMovieCommand.RaiseCanExecuteChanged();
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
                    _registerMovieCommand.RaiseCanExecuteChanged();
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
        public ICommand RegisterMovieCommand => _registerMovieCommand;

        // Validering: knappen er kun aktiv når alle felter er udfyldt og Duration > 0
        private bool CanRegister()
        {
            return !string.IsNullOrWhiteSpace(Title) &&
                   Duration > 0 &&
                   !string.IsNullOrWhiteSpace(Genre);
        }

        // Hovedmetode: registrerer en ny film
        private void RegisterMovie()
        {
            // Opretter et nyt Movie-objekt med de indtastede værdier
            var movie = new Movie
            {
                Title = Title,
                Duration = Duration,
                Genre = Genre
            };

            // Tjek om filmen allerede findes (for at undgå dubletter, dvs. UC1 Exception Flow (4a))
            if (_repository.IsMovieRegistered(movie))
            {
                StatusMessage = "FEJL: Filmen findes allerede!";
                return;
            }

            // Gem filmen via repository (gemmer både i hukommelse og til fil)
            try
            {
                _repository.SaveMovie(movie);
            }
            catch (Exception ex)
            {
                StatusMessage = $"FEJL: Filmen kunne ikke gemmes: {ex.Message}";
                return;
            }

            // Succes: opdater status og ryd felter
            StatusMessage = $"Filmen '{Title}' er nu registreret!";
            Title = string.Empty;
            Duration = 0;
            Genre = string.Empty;
        }

        // Indlæser film ved opstart for at vise antal i status
        private void LoadMovies()
        {
            try
            {
                // Hent alle film. ?. sikrer at vi ikke crasher hvis repository er null
                var movies = _repository?.GetAll();
                // Tæl film. Hvis movies er null, bliver count 0 (pga. ?? 0)
                int count = movies?.Count() ?? 0;

                StatusMessage = count > 0
                    ? $"Indlæste {count} film fra fil"
                    : "Ingen film fundet - registrer en ny!";
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