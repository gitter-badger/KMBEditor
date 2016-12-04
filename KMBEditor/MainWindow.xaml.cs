using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ReactiveProperty<MLT.MLTPage> Page { get; private set; } = new ReactiveProperty<MLT.MLTPage>();
        public ReactiveProperty<string> GitLabIssueURL { get; private set; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> CurrentBoardURL { get; private set; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> DevelopperTwtterURL { get; private set; } = new ReactiveProperty<string>();

        // コマンド
        public ReactiveCommand OpenCommand { get; private set; } = new ReactiveCommand();
        public ReactiveCommand OpenMLTViewerCommand { get; private set; } = new ReactiveCommand();

        public ReactiveCommand PrevPageCommand { get; private set; } = new ReactiveCommand();
        public ReactiveCommand NextPageCommand { get; private set; } = new ReactiveCommand();

        public ReactiveCommand BrowserOpenCommand_GitLabIssueURL { get; private set; }
        public ReactiveCommand BrowserOpenCommand_CurrentBoardURL { get; private set; }
        public ReactiveCommand BrowserOpenCommand_DevelopperTwtterURL { get; private set; }

        // データ
        private MLT.MLTFile current_mlt = new MLT.MLTFile();
        private MLTViewerWindow _mlt_viewer;

        /// <summary>
        /// <para>MLTViewerを表示する</para>
        /// <para>初回はMLTViewerWindonwのインスタンスを生成する</para>
        /// </summary>
        private void MLTViewerWindowTogleVisible()
        {
            // 初期化は初回表示時まで遅延
            if (this._mlt_viewer == null)
            {
                this._mlt_viewer = new MLTViewerWindow();
            }

            // 表示、非表示の切り替え
            if (this._mlt_viewer.IsVisible)
            {
                this._mlt_viewer.Hide();
            }
            else
            {
                this._mlt_viewer.Show();
            }
        }

        public MainWindowViewModel()
        {

            // 変数初期化
            this.GitLabIssueURL.Value = "https://gitlab.com/tar_bin/KMBEditor/issues";
            this.CurrentBoardURL.Value = "";
            this.DevelopperTwtterURL.Value = "https://twitter.com/tar_bin";

            // コマンド初期化
            this.BrowserOpenCommand_GitLabIssueURL = this.GitLabIssueURL.Select(x => !string.IsNullOrEmpty(x)).ToReactiveCommand();
            this.BrowserOpenCommand_DevelopperTwtterURL = this.DevelopperTwtterURL.Select(x => !string.IsNullOrEmpty(x)).ToReactiveCommand();
            this.BrowserOpenCommand_CurrentBoardURL = this.CurrentBoardURL.Select(x => !string.IsNullOrEmpty(x)).ToReactiveCommand();

            // コマンド定義
            this.OpenCommand.Subscribe(_ => this.Page.Value = this.current_mlt.OpemMLTFileWithDialog());
            this.OpenMLTViewerCommand.Subscribe(_ => this.MLTViewerWindowTogleVisible());
            this.PrevPageCommand.Subscribe(_ => this.Page.Value = this.current_mlt.GetPrevPage());
            this.NextPageCommand.Subscribe(_ => this.Page.Value = this.current_mlt.GetNextPage());
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
        }
    }
}
