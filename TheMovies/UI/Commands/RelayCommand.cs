/*
 * Single Responsibility Principle (SRP):
 * Denne klasse har kun én opgave: at implementere ICommand-grænsefladen.
 * Den fungerer som en "bro" mellem UI'et (WPF) og ViewModel'ens metoder.
 * 
 * RelayCommand gør det muligt at binde UI-handlinger (f.eks. knap-klik)
 * til ViewModel-metoder uden at View'et har direkte kendskab til ViewModel'ens logik.
 * 
 * Dette er en genbrugelig komponent - den bruges til ALLE kommandoer i projektet:
 * - RegisterMovieCommand (gem film)
 * - NextMovieCommand (næste film)
 * - PreviousMovieCommand (forrige film)
 * 
 * CanExecute funktionen gør det muligt at aktivere/deaktivere knapper dynamisk,
 * f.eks. deaktiveres "Næste"-knappen når man er på sidste film.
 */

using System;
using System.Windows.Input;

namespace TheMovies.UI.Commands
{
    public class RelayCommand : ICommand
    {
        // Delegates til at udføre kommandoen og tjekke om den kan udføres
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        // Constructor - execute er påkrævet, canExecute er valgfri
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Event der underretter UI'et om, at CanExecute har ændret sig
        // F.eks. når en property ændres, der påvirker om en knap skal være aktiv
        public event EventHandler CanExecuteChanged;

        // Bestemmer om kommandoen kan udføres (f.eks. om knappen er aktiv)
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        // Udfører selve handlingen (kalder den metode, der blev givet i constructoren)
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        // Metode der kan kaldes for at opdatere UI'et (f.eks. når en property ændres)
        // Bruges i ViewModel efter ændringer af Title, Duration, Genre eller navigation
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}