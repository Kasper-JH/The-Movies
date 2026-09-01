/*
 * SRP: Denne ViewModel står for præsentationslogikken til registrering af film (UC1).
 * Den modtager en IMovieRepository via constructor injection.
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
        private readonly IMovieRepository _repository;

        private string _title = string.Empty;
        private int _duration;
        private string _genre = string.Empty;
        private string _statusMessage = string.Empty;

        private readonly RelayCommand _registerMovieCommand;

        public RegisterMovieViewModel(IMovieRepository repository)
        {

            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _registerMovieCommand = new RelayCommand(_ => RegisterMovie(), _ => CanRegister());
            LoadMovies();
        }

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                    _registerMovieCommand.RaiseCanExecuteChanged();
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

        public ICommand RegisterMovieCommand => _registerMovieCommand;

        private bool CanRegister()
        {
            return !string.IsNullOrWhiteSpace(Title) &&
                   Duration > 0 &&
                   !string.IsNullOrWhiteSpace(Genre);
        }

        private void RegisterMovie()
        {
            var movie = new Movie
            {
                Title = Title,
                Duration = Duration,
                Genre = Genre
            };

            // UC1 Exception Flow 4a: Tjek om filmen allerede findes
            if (_repository.IsMovieRegistered(movie))
            {
                StatusMessage = "FEJL: Filmen findes allerede!";
                return;
            }

            try
            {
                _repository.SaveMovie(movie);
            }
            catch (Exception ex)
            {
                StatusMessage = $"FEJL: Filmen kunne ikke gemmes: {ex.Message}";
                return;
            }

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
                var movies = _repository?.GetAll();
                int count = movies?.Count() ?? 0;

                StatusMessage = count > 0
                    ? $"Indlæste {count} film fra fil"
                    : "Ingen film fundet – registrer en ny!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Fejl ved indlæsning: {ex.Message}";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}