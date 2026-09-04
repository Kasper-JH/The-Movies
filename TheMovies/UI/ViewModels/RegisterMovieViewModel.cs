/*
 * SRP: Denne ViewModel står for præsentationslogikken til registrering af film (UC1).
 * ViewModellen indeholder al logik til at validere input, bruge repository'ets 
 * IsMovieRegistered()-metode til at tjekke for dubletter (via modellens 
 * IsDuplicateOf()-regel) og gemme filmen via repository'et.
 */

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using TheMovies.Core.Models;
using TheMovies.Core.Repositories;
using TheMovies.UI.Commands;

namespace TheMovies.UI.ViewModels
{
    public class RegisterMovieViewModel : INotifyPropertyChanged
    {
        // Repository til at gemme og hente film. Modtages via constructor injection.
        private readonly IMovieRepository _repository;

        // Backing fields til properties - her gemmer vi de faktiske værdier.
        private string _title = string.Empty;
        private int _durationHours;
        private int _durationMinutes;
        private int _duration;
        private string _genre = string.Empty;
        private string _statusMessage = string.Empty;

        // Kommandoen til "Registrer film"-knappen.
        private readonly RelayCommand _registerMovieCommand;

        public RegisterMovieViewModel(IMovieRepository repository)
        {
            // Sikrer at repository ikke er null. Hvis det er, stopper programmet med en fejl.
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));

            // Opretter kommandoen - den kører RegisterMovie() når der trykkes,
            // og knappen er kun aktiv hvis CanRegister() returnerer true.
            _registerMovieCommand = new RelayCommand(_ => RegisterMovie(), _ => CanRegister());

            // Viser antallet af eksisterende film i statusfeltet ved opstart.
            LoadMovies();
        }

        // Properties - disse bindes til UI'et (TextBox'er i RegisterMovieView.xaml).

        public string Title
        {
            // Property getter der henter værdien fra backing field'et _title.
            get => _title;
            set
            {
                // Tjekker om værdien rent faktisk har ændret sig.
                // Hvis ikke, gør vi ingenting (behøver ikke opdatere UI).
                if (_title != value)
                {
                    _title = value; // Men hvis værdien er ændret, opdaterer vi backing field'et ovenfor til denne værdi.
                    OnPropertyChanged(); // Og fortæller UI'et at teksten er ændret.
                    _registerMovieCommand.RaiseCanExecuteChanged(); // Tjek om knappen kan aktiveres.
                }
            }
        }

        public int DurationMinutes
        {
            get => _durationMinutes;
            set
            {
                if (_durationMinutes != value)
                {
                    _durationMinutes = value;
                    OnPropertyChanged();

                    Duration = _durationHours * 60 + _durationMinutes;
                }
            }
        }

        public int DurationHours
        {
            get => _durationHours;
            set
            {
                if (_durationHours != value)
                {
                    _durationHours = value;
                    OnPropertyChanged();

                    Duration = _durationHours * 60 + _durationMinutes;
                }
            }
        }

        public int Duration
        {
            get => _duration;
            private set
            {
                // Samme mønster som Title, dvs. opdater UI hvis værdien rent faktisk ændrer sig.
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
                // Samme mønster som Title, dvs. opdater UI hvis værdien rent faktisk ændrer sig.
                if (_genre != value)
                {
                    _genre = value;
                    OnPropertyChanged();
                    _registerMovieCommand.RaiseCanExecuteChanged();
                }
            }
        }

        // StatusMessage vises i bunden af vinduet og giver feedback til brugeren.
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged(); // Opdater UI'et når statusbeskeden ændres.
                }
            }
        }

        // Eksponerer kommandoen til UI'et (knappen binder til denne).
        public ICommand RegisterMovieCommand => _registerMovieCommand;

        // Validerer om alle felter er udfyldt korrekt.
        // Returnerer true hvis knappen ("Registrer film") skal være aktiv, ellers false.
        private bool CanRegister()
        {
            return !string.IsNullOrWhiteSpace(Title) &&   // Titel må ikke være tom.
                   Duration > 0 &&                        // Varighed skal være større end 0.
                   !string.IsNullOrWhiteSpace(Genre);     // Genre må ikke være tom.
        }

        // Hovedmetoden: Registrerer en ny film (UC1).
        private void RegisterMovie()
        {
            // Opretter et nyt Movie-objekt med data fra inputfelterne.
            // Data hentes fra UI'et via properties: Title (tekstboks), Duration (tekstboks) og Genre (tekstboks).
            // Disse tre felter er bundet til hver deres TextBox i RegisterMovieView.xaml.
            var movie = new Movie
            {
                Title = Title,          // fra tekstboksen "Titel".
                Duration = Duration,    // fra tekstboksen "Varighed (min)".
                Genre = Genre           // fra tekstboksen "Genre".
            };

            // UC1 Undtagelsesflow 4a: Tjek om filmen allerede findes.
            // Hvis ja, vis en fejl og stop - filmen gemmes ikke.
            if (_repository.IsMovieRegistered(movie))
            {
                // StatusMessage sættes til en venlig fejlmeddelelse til brugeren.
                StatusMessage = "FEJL: Filmen findes allerede!";
                return;
            }

            // Forsøg at gemme filmen via repository.
            // Hvis det fejler (f.eks. pga. skrivebeskyttet fil), fanges fejlen og vises til brugeren.
            try
            {
                _repository.SaveMovie(movie);
            }
            catch (Exception ex)
            {
                StatusMessage = $"FEJL: Filmen kunne ikke gemmes: {ex.Message}";
                return;
            }

            // Venlig statusbesked til brugeren om at filmen er registreret.
            StatusMessage = $"Filmen '{Title}' er nu registreret!";

            // Nulstil inputfelterne efter registrering af en film.
            // Dette gør det nemt for brugeren at registrere endnu en film uden at skulle slette manuelt.
            Title = string.Empty;
            DurationHours = 0;
            DurationMinutes = 0;
            Genre = string.Empty;
        }

        // Indlæser film ved opstart for at vise antal i status.
        private void LoadMovies()
        {
            try
            {
                // Hent alle film fra repository. Dette kan returnere null, hvis der ikke er nogen film.
                var movies = _repository.GetAll();
                // Hvis movies IKKE er null, så kald Count() og assign resultatet til count (vi leder efter et heltal af film)
                // Hvis movies ER null (= ingen film), så brug tallet 0 i stedet.
                // Resultatet er dermed altid et tal, selv ved null (0) film.
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

        // INotifyPropertyChanged - sørger for at UI'et opdateres når properties ændres.
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}