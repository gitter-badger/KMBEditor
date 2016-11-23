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

        public void OpenMLTFile()
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "ファイルを開く";
            dialog.Filter = "すべてのファイル(*.*)|*.*";

            if (dialog.ShowDialog() == true)
            {
                var filepath = dialog.FileName;
                if (File.Exists(filepath) == false)
                {
                    MessageBox.Show("指定されたファイルが見つかりませんでした");
                    return;
                }

                var st = new StreamReader(filepath, System.Text.Encoding.Default);

                this.AA.Value = st.ReadToEnd();
            }
        }

        public MainWindowViewModel()
        {
            // 初期化
            this.AA = new ReactiveProperty<string>("");
            this.OpenCommand = new ReactiveCommand();

            // コマンド定義
            this.OpenCommand.Subscribe(_ => this.OpenMLTFile());
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

            var win = new MLTViewerWindow();
            win.Show();
        }
    }
}
