/*
 * SRP: Denne klasse repræsenterer en film som data.
 * Den har properties for titel, varighed, genre samt (fra UC2) instruktør og premieredato.
 * INotifyPropertyChanged er implementeret her, fordi projektrammerne kræver det til UI-opdatering.
 */

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TheMovies.Core.Models
{
    public class Movie : INotifyPropertyChanged
    {
        // Backing fields til properties
        private string _title;
        private int _duration;
        private string _genre;

        // "Director" er først en (brugt) del af Movie.cs under UC2 (UC2, trin 4, angiv instruktør), men et
        // Movie-objekt skal stadig kunne oprettes (registreres) under UC1, dvs. uden instruktør.
        // Derfor er Director sat til at være "string.Empty" som default, dvs. indtil brugeren vælger
        // (modificerer) en film og angiver instruktør under UC2.
        // Kort sagt er værdien tom som default (UC1) indtil den konkret skal bruges (UC2), og dermed kan vi
        // oprette de samme Movie-objekter under både UC1 og UC2.
        private string _director = string.Empty;
        // Og samme gør sig gældende med premiereDate (UC2, trin 5, angiv premieredato).
        private DateTime? _premiereDate;

        // Tom constructor til JSON-deserialisering (når filen læses)
        public Movie() { }

        public Movie(string title, int duration, string genre)
        {
            Title = title;
            Duration = duration;
            Genre = genre;
        }

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(); // Fortæller UI'et at værdien er ændret
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
                }
            }
        }

        // Nullable, fordi de ikke er en del af UC1 (oprettelse af en film) -
        // de udfyldes først når filmen indgår i en forestilling, jf. UC2 trin 4-5.
        public string? Director
        {
            get => _director;
            set
            {
                if (_director != value)
                {
                    _director = value;
                    OnPropertyChanged();
                }
            }
        }

        // Nullable af samme grund som Director - sættes først i UC2.
        public DateTime? PremiereDate
        {
            get => _premiereDate;
            set
            {
                if (_premiereDate != value)
                {
                    _premiereDate = value;
                    OnPropertyChanged();
                }
            }
        }

        // Event der bruges til at underrette UI om ændringer
        public event PropertyChangedEventHandler PropertyChanged;

        // Domæneregel (UC1 undtagelsesflow 4a): to film regnes som duplikater, hvis de har
        // samme titel, varighed og genre (case-insensitive på tekstfelterne). Lå tidligere
        // som en LINQ-forespørgsel inde i FileMovieRepository.IsMovieRegistered() - flyttet
        // hertil, da selve reglen for hvad "duplikat" betyder er domænelogik, ikke noget der
        // hører til i persistenslaget. Samme mønster som Screening.OverlapsWith().
        public bool IsDuplicateOf(Movie other)
        {
            return other != null &&
                   string.Equals(Title, other.Title, StringComparison.OrdinalIgnoreCase) &&
                   Duration == other.Duration &&
                   string.Equals(Genre, other.Genre, StringComparison.OrdinalIgnoreCase);
        }

        // Hjælpemetode der kalder PropertyChanged
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
