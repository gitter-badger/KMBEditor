using KMBEditor.Model.MLT;
using KMBEditor.Model.MLTFileTree;
using KMBEditor.Model;
using KMBEditor.CustomDialog.RenameDialog;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace KMBEditor.MLTViewer
{
    public class MLTFileTreeViewItemConverter : ReactiveConverter<RoutedPropertyChangedEventArgs<object>, MLTFileTreeNode>
    {
        protected override IObservable<MLTFileTreeNode> OnConvert(IObservable<RoutedPropertyChangedEventArgs<object>> e)
        {
            return e.Select(x => x.NewValue as MLTFileTreeNode);
        }
    }

    /// <summary>
    /// グループタブごとのDataContext定義
    /// </summary>
    public class GroupTabContext
    {
        /// <summary>
        /// タブのヘッダテキスト
        /// </summary>
        public ReactiveProperty<string> TabHeaderName { get; private set; }
            = new ReactiveProperty<string>();
        /// <summary>
        /// グループタブ内の表示ファイル一覧
        /// </summary>
        public ObservableCollection<MLTFile> TabFileList { get; private set; }
            = new ObservableCollection<MLTFile>();
    }

    /// <summary>
    /// MLTViewerWindow の ViewModel
    /// </summary>
    public class MLTViewerWindowViewModel
    {
        /// <summary>
        /// Viewのインスタンス(Viewへの依存は可能な限り減らすこと)
        /// </summary>
        public WeakReference<MLTViewerWindow> View { get; set; }

        /// <summary>
        /// グループタブのデータリスト
        /// </summary>
        public ObservableCollection<GroupTabContext> GroupTabList { get; private set; }
            = new ObservableCollection<GroupTabContext>();

        /// <summary>
        /// グループタブの選択インデックス
        /// </summary>
        public ReactiveProperty<int> SelectedIndex { get; private set; }
            = new ReactiveProperty<int>();

        public ReactiveProperty<string> ResourceDirectoryPath { get; private set; } = new ReactiveProperty<string>("");
        public ReactiveProperty<List<MLTFileTreeNode>> MLTFileTreeNodes { get; private set; } = new ReactiveProperty<List<MLTFileTreeNode>>();
        public ReactiveCommand OpenResourceDirectoryCommand { get; private set; } = new ReactiveCommand();
        public ReactiveCommand<MLTFileTreeNode> TreeItemSelectCommand { get; private set; } = new ReactiveCommand<MLTFileTreeNode>();
        public ReactiveCommand<MLTFileTreeNode> TreeItemDoubleClickCommand { get; private set; } = new ReactiveCommand<MLTFileTreeNode>();
        public ReactiveCommand<GroupTabContext> RenameTabHeaderCommand { get; private set; } = new ReactiveCommand<GroupTabContext>();

        /// <summary>
        /// リソースファイルツリーのデータ
        /// </summary>
        private MLTFileTreeClass _mlt_file_tree { get; set; } = new MLTFileTreeClass();

        /// <summary>
        /// タブの状態保持ファイル
        /// </summary>
        private JSONSettings<ObservableCollection<GroupTabContext>> _tabSettings;

        /// <summary>
        /// 複数行テンプレートリソースディレクトリへのパス保持ファイル
        /// </summary>
        private JSONSettings<string> _HukuTempPath;

        private void openResourceDirectory()
        {
            using (var dialog = new WinForms.FolderBrowserDialog())
            {
                dialog.Description = "読み込む対象ディレクトリを指定してください";
                dialog.SelectedPath = this.ResourceDirectoryPath.Value; // 前回選択したディレクトリへのパスを初期値として開始

                // 選択されなかった場合はなにもしない
                if (dialog.ShowDialog() != WinForms.DialogResult.OK)
                {
                    return;
                }
                
                // 同じディレクトリが再度選択された場合もなにもしない
                if (dialog.SelectedPath == this.ResourceDirectoryPath.Value)
                {
                    return;
                }

                // 選択されたディレクトリを起点に、MLTファイルを探索
                this.MLTFileTreeNodes.Value = this._mlt_file_tree.SearchMLTFile(dialog.SelectedPath);

                // リソースファイルパスを保存
                this.ResourceDirectoryPath.Value = dialog.SelectedPath;
            }
        }

        private GroupTabContext getCurrentGroupTabContext()
        {
            var index = this.SelectedIndex.Value;
            return this.GroupTabList[index];
        }

        private void addNewTab(MLTFileTreeNode node)
        {
            if (node == null)
            {
                return;
            }

            if (node.IsDirectory == false)
            {
                var file = new MLTFile();

                // MLTファイルのオープン
                file.OpenMLTFile(node.Path);

                // 現在のグループタブにタブ追加
                var groupTab = this.getCurrentGroupTabContext();
                groupTab.TabFileList.Add(file);
            }
        }

        private void updatePreviewTab(MLTFileTreeNode node)
        {
            if (node == null)
            {
                return;
            }

            if (node.IsDirectory == false)
            {
                var file = new MLTFile();

                // MLTファイルのオープン
                file.OpenMLTFile(node.Path);

                // 現在のグループタブのプレビュータブを更新
                var groupTab = this.getCurrentGroupTabContext();
                groupTab.TabFileList[0] = file;
            }
        }

        /// <summary>
        /// グループタブの初期化
        /// </summary>
        private void initGroupTab()
        {
            var maxTabCount = 12;

            foreach (var i in Enumerable.Range(0, maxTabCount))
            {
                var ctx = new GroupTabContext();
                // グループタブの初期ヘッダ名(1始まり)
                ctx.TabHeaderName.Value = $"Group {i + 1}";
                // Previewタブ用のダミーデータを追加
                var previewDammyFile = new MLTFile();
                previewDammyFile.Name = "";
                previewDammyFile.Pages.Clear();
                ctx.TabFileList.Add(previewDammyFile);

                this.GroupTabList.Add(ctx);
            }

            this.SelectedIndex.Value = 0;
        }

        /// <summary>
        /// グループタブの表示名変更
        /// </summary>
        /// <param name="ctx">対象のDataContext</param>
        private void RenameTabHeaderName(GroupTabContext ctx)
        {
            if (ctx == null)
            {
                return;
            }

            var dialog = new RenameDialogWindow(ctx.TabHeaderName.Value);
            if (dialog.ShowDialog() == true)
            {
                ctx.TabHeaderName.Value = dialog.ResponseText;
            }
        }

        /// <summary>
        /// 終了前のデータ保存
        /// </summary>
        public void SaveSettings()
        {
            // 複数行テンプレートリソースディレクトリへのパスを保存
            this._HukuTempPath.Save(this.ResourceDirectoryPath.Value);
            // グループタブの状態を保存(上書き)
            this._tabSettings.Save(this.GroupTabList);
        }

        /// <summary>
        /// グループタブの状態を復帰、または初期化
        /// </summary>
        private void loadGroupTabContextOrInit()
        {
            // グループタブの状態を復帰
            if (this._tabSettings.FileExists())
            {
                // 前回値の保存用設定ファイルが存在する場合
                var groupTabContexts = this._tabSettings.Load();
                foreach (var item in groupTabContexts)
                {
                    this.GroupTabList.Add(item);
                }
            }
            else
            {
                // 前回値の保存用設定ファイルが存在しない場合
                // グループタブ初期化
                this.initGroupTab();
            }
        }

        /// <summary>
        /// MLTFileTreeの状態を復元
        /// </summary>
        private void loadMLTFileTree()
        {
            // 前回値の読出し
            var resourcePath = "";
            if (this._HukuTempPath.FileExists())
            {
                resourcePath = this._HukuTempPath.Load();
            }

            // 以下バリデーション
            // 前回値を利用できない場合は前回値を初期化して返す

            // そもそも設定されていない場合
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                this.ResourceDirectoryPath.Value = "";
                return;
            }

            // 設定はされているが、ディレクトリが存在しない場合
            // リネーム、移動、新バージョン入れ替えなど
            if (!Directory.Exists(resourcePath))
            {
                this.ResourceDirectoryPath.Value = "";
                return;
            }
            
            this.MLTFileTreeNodes.Value = this._mlt_file_tree.SearchMLTFile(resourcePath);
            this.ResourceDirectoryPath.Value = resourcePath;
        }

        /// <summary>
        /// 前回値の復帰
        /// </summary>
        private void loadSettings()
        {
            /// MLTFileTreeの状態を復元
            loadMLTFileTree();
            /// グループタブの状態を復帰、または初期化
            loadGroupTabContextOrInit();
        }

        public void Init()
        {
            // コマンド定義
            this.OpenResourceDirectoryCommand.Subscribe(_ => this.openResourceDirectory());
            this.TreeItemSelectCommand.Subscribe(this.updatePreviewTab);
            this.TreeItemDoubleClickCommand.Subscribe(this.addNewTab);
            this.RenameTabHeaderCommand.Subscribe(this.RenameTabHeaderName);

            // プロパティ初期化
            this._tabSettings = new JSONSettings<ObservableCollection<GroupTabContext>>(@"mltviewer_tabsettings.json");
            this._HukuTempPath = new JSONSettings<string>(@"mltviewer_hukutemppath.json");

            // 前回値の復帰
            this.loadSettings();
        }

        public MLTViewerWindowViewModel()
        {
            // Viewの初期化が必要なためここで初期化しない
        }
    }

    /// <summary>
    /// MLTViewerWindow.xaml の相互作用ロジック
    /// 
    /// 機能要件:
    /// ・MLTのZipファイルの一覧表示機能を提供
    /// ・編集機能は持たない、閲覧機能のみを提供
    /// 
    /// </summary>
    public partial class MLTViewerWindow : Window
    {
        public MLTViewerWindow()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var view = sender as Grid;
            var viewModel = view.DataContext as MLTViewerWindowViewModel;

            viewModel.View = new WeakReference<MLTViewerWindow>(this);
            viewModel.Init();
        }

        /// <summary>
        /// <para>クローズボタンが押された時のイベント</para>
        /// <para>ファイルツリーの再インデックスに時間がかかるため、Closeはせずに隠すだけにする</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Windowは終了させない
            e.Cancel = true;
            // Windowを隠す。再表示はMainWindow側で行う
            this.Hide();
        }

        private void TreeViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = sender as TreeViewItem;
            var node = item.DataContext as MLTFileTreeNode;

            var vm = this.MLTViewer.DataContext as MLTViewerWindowViewModel;
            vm.TreeItemDoubleClickCommand.Execute(node);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            var vm = this.MLTViewer.DataContext as MLTViewerWindowViewModel;
            // 開いているタブなどの状態の保存
            vm.SaveSettings();
        }
    }
}
