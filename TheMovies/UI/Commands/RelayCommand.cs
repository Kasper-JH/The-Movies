/*
 * SRP: Denne klasse implementerer ICommand-grænsefladen, så ViewModel kan 
 * binde handlinger til UI-elementer (f.eks. knapper).
 * Den fungerer som en bro mellem View og ViewModel.
 */

using System;
using System.Windows.Input;

namespace TheMovies.UI.Commands
{
    public class RelayCommand : ICommand
    {
        // _execute indeholder den metode, der skal udføres, _canExecute tjekker om den kan udføres
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            // Null-coalescing operator (??) tjekker om execute er null.
            // Hvis ja: smid en ArgumentNullException (execute skal altid være sat)
            // Hvis nej: gem execute i _execute
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Event der fortæller UI, at CanExecute-status er ændret
        public event EventHandler CanExecuteChanged;

        // Kan kommandoen udføres? (bruges til at aktivere/deaktivere knapper)
        public bool CanExecute(object parameter)
        {
            // Hvis _canExecute er null, er knappen altid aktiv
            return _canExecute == null || _canExecute(parameter);
        }

        // Udfør selve handlingen (kalder den gemte metode)
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        // Metode der kan kaldes for at opdatere UI (f.eks. når en property ændres)
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}