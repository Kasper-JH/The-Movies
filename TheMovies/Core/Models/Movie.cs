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
        private string? _director;
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
        // de udfyldes først når filmen indgår i en forestilling, jf. UC2 trin 3-4.
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

        // Hjælpemetode der kalder PropertyChanged
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
