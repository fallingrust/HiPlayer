using HiPlayer.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace HiPlayer.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void br_top_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void slider_duration_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            var vm = this.DataContext as MainViewModel;
            vm._isSeeking = true;
        }

        private void slider_duration_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            var vm = this.DataContext as MainViewModel;
            vm._isSeeking = false;
            if (vm.SeekToCommand.CanExecute(slider_duration.Value))
            {
                vm.SeekToCommand.Execute(slider_duration.Value);
            }
        }

        private void slider_Volum_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var vm = this.DataContext as MainViewModel;
            if (vm.VolumeChangeCommand.CanExecute((int)slider_Volum.Value))
            {
                vm.VolumeChangeCommand.Execute((int)slider_Volum.Value);
            }
        }

        private void btn_min_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btn_max_Click(object sender, RoutedEventArgs e)
        {
            if(WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                return;
            }
            if(WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                return;
            }
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Application.Current.Shutdown();

        }
    }
}
