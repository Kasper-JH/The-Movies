/*
 * SRP: Denne fil indeholder kun konstruktoren til CreateScreeningView.
 * DataContext sættes udefra (i App.xaml.cs), så der er ingen logik her.
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
