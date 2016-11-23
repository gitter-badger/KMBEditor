using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Reactive.Bindings;
using Microsoft.Win32;
using System.IO;

namespace KMBEditor
{
    /// <summary>
    /// MainWindow の ViewModel
    /// </summary>
    public class MainWindowViewModel
    {
        // プロパティ
        public ReactiveProperty<string> AA { get; private set; }

        // コマンド
        public ReactiveCommand OpenCommand { get; private set; }

        public ReactiveCommand PrevPageCommand { get; private set; }
        public ReactiveCommand NextPageCommand { get; private set; }

        // データ
        private MLTClass current_mlt;

        public MainWindowViewModel()
        {
            // 初期化
            this.AA = new ReactiveProperty<string>("");
            this.OpenCommand = new ReactiveCommand();
            this.PrevPageCommand = new ReactiveCommand();
            this.NextPageCommand = new ReactiveCommand();
            this.current_mlt = new MLTClass();

            // コマンド定義
            this.OpenCommand.Subscribe(_ => this.AA.Value = this.current_mlt.OpenMLTFile());
            this.PrevPageCommand.Subscribe(_ => this.AA.Value = this.current_mlt.GetPrevPage());
            this.NextPageCommand.Subscribe(_ => this.AA.Value = this.current_mlt.GetNextPage());
        }
    }

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // MLTViewerの表示
            var win = new MLTViewerWindow();
            win.Show();
        }
    }
}
