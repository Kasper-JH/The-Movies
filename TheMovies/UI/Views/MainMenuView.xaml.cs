/*
 * SRP: Code-behind til startmenuen. DataContext sættes i App.xaml.cs,
 * så der er ingen logik her – det er altsammen i MainMenuViewModel.cs.
 */
using System.Windows;

namespace TheMovies.UI.Views
{
    public partial class MainMenuView : Window
    {
        public MainMenuView()
        {
            InitializeComponent();
        }
    }
}