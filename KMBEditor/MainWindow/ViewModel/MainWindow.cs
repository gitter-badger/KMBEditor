using KMBEditor.MLTViewer.View;
using KMBEditor.Model;
using KMBEditor.Model.MLT;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace KMBEditor.MainWindow.ViewModel
{
    /// <summary>
    /// MainWindow の ViewModel
    /// </summary>
    public class MainWindowViewModel
    {
        private GlobalSettings _global_settings = GlobalSettings.Instance; 

        // プロパティ
        public ReadOnlyObservableCollection<MLTPage> PageList { get; private set; }
        public ReactiveProperty<MLTPage> Page { get; private set; } = new ReactiveProperty<MLTPage>();
        public ReactiveProperty<string> OnlineDocumentURL { get; private set; }
        public ReactiveProperty<string> GitHubIssueURL { get; private set; }
        public ReactiveProperty<string> CurrentBoardURL { get; private set; }
        public ReactiveProperty<string> DevelopperTwtterURL { get; private set; }
        public ReactiveProperty<string> OrignalPageBytes { get; private set; } = new ReactiveProperty<string>();

        // コマンド
        public ReactiveCommand OpenCommand { get; private set; } = new ReactiveCommand();
        public ReactiveCommand OpenMLTViewerCommand { get; private set; } = new ReactiveCommand();

        public ReactiveCommand PrevPageCommand { get; private set; } = new ReactiveCommand();
        public ReactiveCommand NextPageCommand { get; private set; } = new ReactiveCommand();

        public ReactiveCommand BrowserOpenCommand_OnlineDocumentURL { get; private set; }
        public ReactiveCommand BrowserOpenCommand_GitHubIssueURL { get; private set; }
        public ReactiveCommand BrowserOpenCommand_CurrentBoardURL { get; private set; }
        public ReactiveCommand BrowserOpenCommand_DevelopperTwtterURL { get; private set; }

        // データ
        private MLTFile _current_mlt_file = new MLTFile();
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
            this.GitHubIssueURL = this._global_settings.ObserveProperty(x => x.GitHubIssueURL).ToReactiveProperty();
            this.DevelopperTwtterURL = this._global_settings.ObserveProperty(x => x.DevelopperTwtterURL).ToReactiveProperty();
            this.CurrentBoardURL = this._global_settings.ObserveProperty(x => x.CurrentBoardURL).ToReactiveProperty();
            this.PageList = this._current_mlt_file.Pages.ToReadOnlyReactiveCollection();

            // コマンド初期化
            this.BrowserOpenCommand_OnlineDocumentURL = this.OnlineDocumentURL.Select(x => !string.IsNullOrEmpty(x)).ToReactiveCommand();
            this.BrowserOpenCommand_GitHubIssueURL = this.GitHubIssueURL.Select(x => !string.IsNullOrEmpty(x)).ToReactiveCommand();
            this.BrowserOpenCommand_DevelopperTwtterURL = this.DevelopperTwtterURL.Select(x => !string.IsNullOrEmpty(x)).ToReactiveCommand();
            this.BrowserOpenCommand_CurrentBoardURL = this.CurrentBoardURL.Select(x => !string.IsNullOrEmpty(x)).ToReactiveCommand();

            // コマンド定義
            this.OpenCommand.Subscribe(_ => this.Page.Value = this._current_mlt_file.OpemMLTFileWithDialog());
            this.OpenMLTViewerCommand.Subscribe(_ => this.MLTViewerWindowTogleVisible());
            this.PrevPageCommand.Subscribe(_ => this.Page.Value = this._current_mlt_file.GetPrevPage());
            this.NextPageCommand.Subscribe(_ => this.Page.Value = this._current_mlt_file.GetNextPage());
            this.BrowserOpenCommand_OnlineDocumentURL.Subscribe(url => System.Diagnostics.Process.Start(url.ToString()));
            this.BrowserOpenCommand_GitHubIssueURL.Subscribe(url => System.Diagnostics.Process.Start(url.ToString()));
            this.BrowserOpenCommand_DevelopperTwtterURL.Subscribe(url => System.Diagnostics.Process.Start(url.ToString()));
            this.BrowserOpenCommand_CurrentBoardURL.Subscribe(url => System.Diagnostics.Process.Start(url.ToString()));

            // リアクティブプロパティ設定
            this.OrignalPageBytes = this.Page
                    .Select(obj => obj == null ? 0 : obj.Bytes)
                    .Select(size => String.Format("{0} [Bytes]", size))
                    .ToReactiveProperty<string>();
        }
    }

}
