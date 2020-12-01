using System;
using System.Windows;

namespace HiPlayer.Windows
{
    /// <summary>
    /// OpenUrlWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OpenUrlWindow : Window
    {
        public event EventHandler<EventArgs> AddUrlEvent;
        public OpenUrlWindow()
        {
            InitializeComponent();
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btn_confirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tb_url.Text))
            {
                return;
            }
            AddUrlEvent?.Invoke(tb_url.Text, null);
            this.Close();
        }
    }
}
