using KMBEditor.Model;
using KMBEditor.Model.MLT;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Interactivity;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KMBEditor.MainWindow
{
    /// <summary>
    /// タブごとのデータコンテキスト
    /// </summary>
    public class TabItemContent
    {
        public MLTFile File { get; set; }
        public ReactiveProperty<MLTPage> Page { get; set; }
    }

    /// <summary>
    /// MainWindow の ViewModel
    /// </summary>
    public class MainWindowViewModel
    {
        /// <summary>
        /// Viewのインスタンス(Viewへの依存は可能な限り減らすこと)
        /// </summary>
        public WeakReference<MainWindow> View;

        // プロパティ
        public ReactiveProperty<string> OnlineDocumentURL { get; private set; }
        public ReactiveProperty<string> GitHubIssueURL { get; private set; }
        public ReactiveProperty<string> CurrentBoardURL { get; private set; }
        public ReactiveProperty<string> DevelopperTwtterURL { get; private set; }
        public ReactiveProperty<string> OrignalPageBytes { get; private set; } = new ReactiveProperty<string>();
        public ObservableCollection<TabItemContent> TabItems { get; private set; } = new ObservableCollection<TabItemContent>();
        /// <summary>
        /// ページリスト表示用変数
        /// </summary>
        public ReactiveProperty<ObservableCollection<MLTPage>> PageList { get; private set; }
            = new ReactiveProperty<ObservableCollection<MLTPage>>();
        /// <summary>
        /// タブの選択インデックス
        /// </summary>
        public ReactiveProperty<int> SelectedIndex { get; private set; }
            = new ReactiveProperty<int>();

        // コマンド
        public ReactiveCommand CreateNewMLTFileCommand { get; private set; } = new ReactiveCommand();
        public ReactiveCommand OpenCommand { get; private set; } = new ReactiveCommand();
        public ReactiveCommand OpenMLTViewerCommand { get; private set; } = new ReactiveCommand();
        public ReactiveCommand<TabItemContent> CloseTabCommand { get; private set; } = new ReactiveCommand<TabItemContent>();

        public ReactiveCommand PrevPageCommand { get; private set; } = new ReactiveCommand();
        public ReactiveCommand NextPageCommand { get; private set; } = new ReactiveCommand();

        public ReactiveCommand BrowserOpenCommand_OnlineDocumentURL { get; private set; }
        public ReactiveCommand BrowserOpenCommand_GitHubIssueURL { get; private set; }
        public ReactiveCommand BrowserOpenCommand_CurrentBoardURL { get; private set; }
        public ReactiveCommand BrowserOpenCommand_DevelopperTwtterURL { get; private set; }

        /// <summary>
        /// 共通設定保持クラス
        /// </summary>
        private GlobalSettings _global_settings = GlobalSettings.Instance;

        /// <summary>
        /// MLTファイルの作成とタブへの追加
        /// </summary>
        private void createNewMLTFile()
        {
            var new_mlt_file = new MLTFile();

            // 空ページで初期化
            new_mlt_file.Pages.Add(new MLTPage());

            // タブを追加
            this.TabItems.Add(
                new TabItemContent
                {
                    File = new_mlt_file,
                    Page = new ReactiveProperty<MLTPage>(new_mlt_file.GetCurrentPage())
                });

            // 追加したタブを選択
            this.SelectedIndex.Value = this.TabItems.Count - 1;
        }

        /// <summary>
        /// 既存MLTファイルのオープンとタブへの追加
        /// </summary>
        private void openMLTFile()
        {
            var new_mlt_file = new MLTFile();

            // ファイル選択ダイアログを開く
            new_mlt_file.OpemMLTFileWithDialog();

            // タブを追加
            this.TabItems.Add(
                new TabItemContent
                {
                    File = new_mlt_file,
                    Page = new ReactiveProperty<MLTPage>(new_mlt_file.GetCurrentPage())
                });

            // 追加したタブを選択
            this.SelectedIndex.Value = this.TabItems.Count - 1;
        }

        private void movePrevPage()
        {
            // 追加したタブに遷移
            MainWindow obj;
            if (this.View.TryGetTarget(out obj))
            {
                var tab = obj.EditorTabControl;
                var tabindex = tab.SelectedIndex;
                var tabitem = this.TabItems[tabindex];
                tabitem.Page.Value = tabitem.File.GetPrevPage();
            }
        }

        private void moveNextPage()
        {
            // 追加したタブに遷移
            MainWindow obj;
            if (this.View.TryGetTarget(out obj))
            {
                var tab = obj.EditorTabControl;
                var tabindex = tab.SelectedIndex;
                var tabitem = this.TabItems[tabindex];
                tabitem.Page.Value = tabitem.File.GetNextPage();
            }
        }

        /// <summary>
        /// タブの削除コマンドアクション
        /// </summary>
        /// <param name="tabctx"></param>
        private void closeTabCommandAction(TabItemContent tabctx)
        {
            if (tabctx == null)
            {
                return;
            }

            this.TabItems.Remove(tabctx);
        }

        /// <summary>
        /// ページリストの更新
        /// </summary>
        /// <param name="index"></param>
        private void updatePageList(int index)
        {
            // 選択状態がリセットされた場合にインデックスが-1になる場合と
            // 上位で不正に指定された場合での動作を除外する
            if (this.TabItems.Count > index && index >= 0)
            {
                this.PageList.Value = this.TabItems[index].File.Pages;
            }
        }

        /// <summary>
        /// ViewModelの初期化
        /// </summary>
        public void Init()
        {
            // タブの初期化
            this.createNewMLTFile();

            // 変数初期化(Model => ViewModel 単方向バインド)
            this.OnlineDocumentURL = this._global_settings.ObserveProperty(x => x.OnlineDocumentURL).ToReactiveProperty();
            this.GitHubIssueURL = this._global_settings.ObserveProperty(x => x.GitHubIssueURL).ToReactiveProperty();
            this.DevelopperTwtterURL = this._global_settings.ObserveProperty(x => x.DevelopperTwtterURL).ToReactiveProperty();
            this.CurrentBoardURL = this._global_settings.ObserveProperty(x => x.CurrentBoardURL).ToReactiveProperty();

            // コマンド初期化
            this.BrowserOpenCommand_OnlineDocumentURL = this.OnlineDocumentURL.Select(x => !string.IsNullOrEmpty(x)).ToReactiveCommand();
            this.BrowserOpenCommand_GitHubIssueURL = this.GitHubIssueURL.Select(x => !string.IsNullOrEmpty(x)).ToReactiveCommand();
            this.BrowserOpenCommand_DevelopperTwtterURL = this.DevelopperTwtterURL.Select(x => !string.IsNullOrEmpty(x)).ToReactiveCommand();
            this.BrowserOpenCommand_CurrentBoardURL = this.CurrentBoardURL.Select(x => !string.IsNullOrEmpty(x)).ToReactiveCommand();

            // コマンド定義
            this.CreateNewMLTFileCommand.Subscribe(_ => this.createNewMLTFile());
            this.OpenCommand.Subscribe(_ => this.openMLTFile());
            this.PrevPageCommand.Subscribe(_ => this.movePrevPage());
            this.NextPageCommand.Subscribe(_ => this.moveNextPage());
            this.CloseTabCommand.Subscribe(this.closeTabCommandAction);
            this.BrowserOpenCommand_OnlineDocumentURL.Subscribe(url => System.Diagnostics.Process.Start(url.ToString()));
            this.BrowserOpenCommand_GitHubIssueURL.Subscribe(url => System.Diagnostics.Process.Start(url.ToString()));
            this.BrowserOpenCommand_DevelopperTwtterURL.Subscribe(url => System.Diagnostics.Process.Start(url.ToString()));
            this.BrowserOpenCommand_CurrentBoardURL.Subscribe(url => System.Diagnostics.Process.Start(url.ToString()));

            // プロパティ定義
            // ページリストの更新
            this.SelectedIndex.Subscribe(updatePageList);

            // リアクティブプロパティ設定
            //this.OrignalPageBytes = this.Page
            //        .Select(obj => obj == null ? 0 : obj.Bytes)
            //        .Select(size => String.Format("{0} [Bytes]", size))
            //        .ToReactiveProperty<string>();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel()
        {
            // Viewの設定等が必要なため、ここで初期化はしない
        }
    }

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : MahApps.Metro.Controls.MetroWindow
    {
        private MainWindowViewModel _vm = new MainWindowViewModel();

        public MainWindow()
        {
            InitializeComponent();

            // ViewModelの初期化
            this._vm.View = new WeakReference<MainWindow>(this);

            this._vm.Init();

            // DataContextの設定
            this.DataContext = _vm;
        }

        private void dataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "RawText":
                case "DecodeText":
                case "IsCaption":
                    // 表示しない
                    e.Cancel = true;
                    break;
            }
        }

        private void EditorTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
