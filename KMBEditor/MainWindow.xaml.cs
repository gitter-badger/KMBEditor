using Reactive.Bindings;
using Reactive.Bindings.Extensions;
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
using KMBEditor.Model;

namespace KMBEditor
{
    /// <summary>
    /// MainWindow の ViewModel
    /// </summary>
    public class MainWindowViewModel
    {
        private GlobalSettings _global_settings = GlobalSettings.Instance; 

        // プロパティ
        public ReadOnlyObservableCollection<MLT.MLTPage> PageList { get; private set; }
        public ReactiveProperty<MLT.MLTPage> Page { get; private set; } = new ReactiveProperty<MLT.MLTPage>();
        public ReactiveProperty<string> OnlineDocumentURL { get; private set; }
        public ReactiveProperty<string> GitLabIssueURL { get; private set; }
        public ReactiveProperty<string> CurrentBoardURL { get; private set; }
        public ReactiveProperty<string> DevelopperTwtterURL { get; private set; }
        public ReactiveProperty<string> OrignalPageBytes { get; private set; } = new ReactiveProperty<string>();

        // コマンド
        public ReactiveCommand OpenCommand { get; private set; } = new ReactiveCommand();
        public ReactiveCommand OpenMLTViewerCommand { get; private set; } = new ReactiveCommand();

        public ReactiveCommand PrevPageCommand { get; private set; } = new ReactiveCommand();
        public ReactiveCommand NextPageCommand { get; private set; } = new ReactiveCommand();

        public ReactiveCommand BrowserOpenCommand_OnlineDocumentURL { get; private set; }
        public ReactiveCommand BrowserOpenCommand_GitLabIssueURL { get; private set; }
        public ReactiveCommand BrowserOpenCommand_CurrentBoardURL { get; private set; }
        public ReactiveCommand BrowserOpenCommand_DevelopperTwtterURL { get; private set; }

        // データ
        private MLT.MLTFile _current_mlt_file = new MLT.MLTFile();
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

            // 変数初期化(Model => ViewModel 単方向バインド)
            this.OnlineDocumentURL = this._global_settings.ObserveProperty(x => x.OnlineDocumentURL).ToReactiveProperty();
            this.GitLabIssueURL = this._global_settings.ObserveProperty(x => x.GitLabIssueURL).ToReactiveProperty();
            this.DevelopperTwtterURL = this._global_settings.ObserveProperty(x => x.DevelopperTwtterURL).ToReactiveProperty();
            this.CurrentBoardURL = this._global_settings.ObserveProperty(x => x.CurrentBoardURL).ToReactiveProperty();
            this.PageList = this._current_mlt_file.Pages.ToReadOnlyReactiveCollection();

            // コマンド初期化
            this.BrowserOpenCommand_OnlineDocumentURL = this.OnlineDocumentURL.Select(x => !string.IsNullOrEmpty(x)).ToReactiveCommand();
            this.BrowserOpenCommand_GitLabIssueURL = this.GitLabIssueURL.Select(x => !string.IsNullOrEmpty(x)).ToReactiveCommand();
            this.BrowserOpenCommand_DevelopperTwtterURL = this.DevelopperTwtterURL.Select(x => !string.IsNullOrEmpty(x)).ToReactiveCommand();
            this.BrowserOpenCommand_CurrentBoardURL = this.CurrentBoardURL.Select(x => !string.IsNullOrEmpty(x)).ToReactiveCommand();

            // コマンド定義
            this.OpenCommand.Subscribe(_ => this.Page.Value = this._current_mlt_file.OpemMLTFileWithDialog());
            this.OpenMLTViewerCommand.Subscribe(_ => this.MLTViewerWindowTogleVisible());
            this.PrevPageCommand.Subscribe(_ => this.Page.Value = this._current_mlt_file.GetPrevPage());
            this.NextPageCommand.Subscribe(_ => this.Page.Value = this._current_mlt_file.GetNextPage());
            this.BrowserOpenCommand_OnlineDocumentURL.Subscribe(url => System.Diagnostics.Process.Start(url.ToString()));
            this.BrowserOpenCommand_GitLabIssueURL.Subscribe(url => System.Diagnostics.Process.Start(url.ToString()));
            this.BrowserOpenCommand_DevelopperTwtterURL.Subscribe(url => System.Diagnostics.Process.Start(url.ToString()));
            this.BrowserOpenCommand_CurrentBoardURL.Subscribe(url => System.Diagnostics.Process.Start(url.ToString()));

            // リアクティブプロパティ設定
            this.OrignalPageBytes = this.Page
                    .Select(obj => obj == null ? 0 : obj.Bytes)
                    .Select(size => String.Format("{0} [Bytes]", size))
                    .ToReactiveProperty<string>();
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
