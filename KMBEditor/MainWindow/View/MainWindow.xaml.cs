using System.Windows;
using System.Windows.Controls;
using KMBEditor.MainWindow.ViewModel;

namespace KMBEditor.MainWindow.View
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();

            _vm = new MainWindowViewModel();

            this.DataContext = _vm;
        }

        private void dataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "AA":
                    // AAは表示しない
                    e.Cancel = true;
                    break;
            }
        }
    }
}
