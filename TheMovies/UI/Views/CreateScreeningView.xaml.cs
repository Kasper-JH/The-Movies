/*
 * SRP: Code-behind til oprettelsesvinduet. DataContext sættes udefra,
 * så der er ingen logik her.
 */
using System.Windows;

namespace TheMovies.UI.Views
{
    public partial class CreateScreeningView : Window
    {
        public CreateScreeningView()
        {
            InitializeComponent();
        }
    }
}