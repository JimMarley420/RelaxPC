using System;
using System.Diagnostics;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace DarkAppShutdown
{
    public partial class MainWindow : Window
    {
        private readonly SoundPlayer _soundPlayer = new SoundPlayer("relax_music.wav"); // Fichier audio de fond
        private bool isCountdownActive = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private int GetSelectedDelayInSeconds()
        {
            int timeValue = 0;
            int.TryParse(TimeValueTextBox.Text, out timeValue);

            switch (TimeUnitComboBox.SelectedIndex)
            {
                case 0: // Secondes
                    return timeValue;
                case 1: // Minutes
                    return timeValue * 60;
                case 2: // Heures
                    return timeValue * 3600;
                default:
                    return timeValue;
            }
        }

        private async void ShutdownButton_Click(object sender, RoutedEventArgs e)
        {
            int delay = GetSelectedDelayInSeconds();

            if (delay <= 0)
            {
                DisplayNotification("Veuillez entrer un délai valide.");
                return;
            }

            ExecuteSystemCommand("shutdown -s -f -t " + delay);
            await StartCountdown(delay, "Arrêt");
        }

        private async void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            int delay = GetSelectedDelayInSeconds();

            if (delay <= 0)
            {
                DisplayNotification("Veuillez entrer un délai valide.");
                return;
            }

            ExecuteSystemCommand("shutdown -r -f -t " + delay);
            await StartCountdown(delay, "Redémarrage");
        }

        private void SleepButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(ShutdownManager.SetSystemSleep, DispatcherPriority.Normal);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteSystemCommand("shutdown -a");
            isCountdownActive = false;
            CountdownTextBlock.Text = "";
            DisplayNotification("L'action programmée a été annulée.");
        }

        private async Task StartCountdown(int delay, string action)
        {
            isCountdownActive = true;

            for (int i = delay; i > 0; i--)
            {
                if (!isCountdownActive)
                {
                    CountdownTextBlock.Text = "";
                    return;
                }

                CountdownTextBlock.Text = $"{action} dans {i} secondes...";
                await Task.Delay(1000);
            }

            CountdownTextBlock.Text = "";
            isCountdownActive = false;
        }

        private void ExecuteSystemCommand(string command)
        {
            Process.Start(new ProcessStartInfo("cmd.exe", "/c " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _soundPlayer.PlayLooping(); // Lance la musique de fond
        }

        private void DisplayNotification(string message)
        {
            // Vous pouvez personnaliser cette partie pour afficher des notifications plus élégantes
            MessageBox.Show(message, "Green Shutdown", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Permettre le déplacement de la fenêtre
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }

    public static class ShutdownManager
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void LockWorkStation();

        [System.Runtime.InteropServices.DllImport("PowrProf.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);

        public static void SetSystemSleep()
        {
            SetSuspendState(false, true, true);
        }
    }
}
