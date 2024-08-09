namespace Deenote.UI.Windows
{
    partial class AboutWindow
    {
        public void OpenWindow(AboutPage page)
        {
            _window.IsActivated = true;

            var pageController = page switch {
                AboutPage.AboutDevelopers => _developersPage,
                AboutPage.UpdateHistory => _updateHistoryPage,
                AboutPage.Tutorials => _tutorialsPage,
                _ => _developersPage,
            };

            LoadPage(pageController);
        }

        public enum AboutPage
        {
            AboutDevelopers,
            UpdateHistory,
            Tutorials,
        }
    }
}