/*
 * SRP: Code-behind til registreringsvinduet. DataContext sættes udefra,
 * så der er ingen logik her.
 */
using System.Windows;
using System.Windows.Input;

namespace TheMovies.UI.Views
{
    public partial class RegisterMovieView : Window
    {
        public RegisterMovieView()
        {
            InitializeComponent();
        }

        // Validering: Tillader kun tal i int-felter
        private void NumberOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }
    }
}