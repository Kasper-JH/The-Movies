/*
 * SRP: Denne klasse står for opstart af applikationen og fungerer som 'composition root'.
 * Ved at samle al opsætning af afhængigheder her opnår vi en klar adskillelse mellem
 * opstart og forretningslogik, hvilket understøtter Single Responsibility Principle (SRP).
 * Repositories og ViewModels forbindes via constructor injection her, så ViewModels alene
 * afhænger af abstrakte interfaces frem for konkrete implementeringer ("new" repositories sendt med 
 * ind i constructor-kaldet til MainMenuViewModel, m.a.o.). Af dette får vi adgang til repositories 
 * (og dermed CRUD-funktionalitet) fra ét enkelt sted og undgår at skulle oprette nye instanser af 
 * repositories i flere forskellige klasser efter behov. Dette følger også Dependency Inversion Principle (DIP), da 
 * vi afhænger af abstrakte interfaces (IMovieRepository og IScreeningRepository) frem for konkrete 
 * implementeringer som FileMovieRepository. Derudover sikrer dette også lav kobling (til en konkret løsning 
 * ift. persistens), hvilket gør det nemt at udskifte datakilde (f.eks. fra JSON-fil til en mere GDPR-sikker 
 * løsning i form af en database senere, til trods for at projektrammerne ikke tillader dette for nuværende) 
 * uden at skulle ændre på store dele af koden. 
 */
using System;
using System.Windows;
using TheMovies.Core.Repositories;
using TheMovies.UI.ViewModels;
using TheMovies.UI.Views;

namespace TheMovies
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Opret repositories til persistens (JSON-filer)
                var movieRepository = new FileMovieRepository();
                var screeningRepository = new FileScreeningRepository();

                // Opret startmenu med tilhørende ViewModel
                var mainMenuView = new MainMenuView();
                // Injicer vores repositories i ViewModel, som derefter kan bruges til at hente og gemme data (composition root).
                mainMenuView.DataContext = new MainMenuViewModel(movieRepository, screeningRepository);
                mainMenuView.Show();
            }
            catch (InvalidOperationException ex)
            {
                // Hvis filerne er korrupte eller utilgængelige, vises en fejl og programmet lukker
                MessageBox.Show($"Kunne ikke starte: {ex.Message}", "Fejl ved opstart",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}