using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
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

namespace KMBEditor
{
    /// <summary>
    /// MainWindow の ViewModel
    /// </summary>
    public class MainWindowViewModel
    {
        // プロパティ
        public ReactiveProperty<string> AA { get; private set; }
        public ReactiveProperty<string> GitLabIssueURL { get; private set; }
        public ReactiveProperty<string> CurrentBoardURL { get; private set; }
        public ReactiveProperty<string> DevelopperTwtterURL { get; private set; }

        // コマンド
        public ReactiveCommand OpenCommand { get; private set; }

        public ReactiveCommand PrevPageCommand { get; private set; }
        public ReactiveCommand NextPageCommand { get; private set; }

        public ReactiveCommand BrowserOpenCommand_GitLabIssueURL { get; private set; }
        public ReactiveCommand BrowserOpenCommand_CurrentBoardURL { get; private set; }
        public ReactiveCommand BrowserOpenCommand_DevelopperTwtterURL { get; private set; }

        // データ
        private MLTClass current_mlt;

        public MainWindowViewModel()
        {
            this.current_mlt = new MLTClass();

            // 変数初期化
            this.AA = new ReactiveProperty<string>("");
            this.GitLabIssueURL = new ReactiveProperty<string>("https://gitlab.com/tar_bin/KMBEditor/issues");
            this.CurrentBoardURL = new ReactiveProperty<string>("");
            this.DevelopperTwtterURL = new ReactiveProperty<string>("https://twitter.com/tar_bin");

            // コマンド初期化
            this.OpenCommand = new ReactiveCommand();
            this.PrevPageCommand = new ReactiveCommand();
            this.NextPageCommand = new ReactiveCommand();
            this.BrowserOpenCommand_GitLabIssueURL = this.GitLabIssueURL.Select(x => !string.IsNullOrEmpty(x)).ToReactiveCommand();
            this.BrowserOpenCommand_DevelopperTwtterURL = this.DevelopperTwtterURL.Select(x => !string.IsNullOrEmpty(x)).ToReactiveCommand();
            this.BrowserOpenCommand_CurrentBoardURL = this.CurrentBoardURL.Select(x => !string.IsNullOrEmpty(x)).ToReactiveCommand();

            // コマンド定義
            this.OpenCommand.Subscribe(_ => this.AA.Value = this.current_mlt.OpemMLTFileWithDialog());
            this.PrevPageCommand.Subscribe(_ => this.AA.Value = this.current_mlt.GetPrevPage());
            this.NextPageCommand.Subscribe(_ => this.AA.Value = this.current_mlt.GetNextPage());
            this.BrowserOpenCommand_GitLabIssueURL.Subscribe(url => System.Diagnostics.Process.Start(url.ToString()));
            this.BrowserOpenCommand_DevelopperTwtterURL.Subscribe(url => System.Diagnostics.Process.Start(url.ToString()));
            this.BrowserOpenCommand_CurrentBoardURL.Subscribe(url => System.Diagnostics.Process.Start(url.ToString()));
        }
    }

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

            // MLTViewerの表示
            var win = new MLTViewerWindow();
            win.Show();
        }
    }
}
