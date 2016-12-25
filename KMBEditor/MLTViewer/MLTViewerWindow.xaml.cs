using KMBEditor.Model.MLT;
using KMBEditor.Model.MLTFileTree;
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

        private MLTFileTreeClass _mlt_file_tree { get; set; } = new MLTFileTreeClass();

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

        public void Init()
        {
            // コマンド定義
            this.OpenResourceDirectoryCommand.Subscribe(_ => this.openResourceDirectory());
            this.TreeItemSelectCommand.Subscribe(this.updatePreviewTab);
            this.TreeItemDoubleClickCommand.Subscribe(this.addNewTab);

            // FileTreeの初期化
            // XXX: リソースファイル切り替えるユースケースってある？
            // 前回値の読出し
            var resourcePath = Properties.Settings.Default.MLTResourcePath;
            if (!string.IsNullOrWhiteSpace(resourcePath))
            {
                this.MLTFileTreeNodes.Value = this._mlt_file_tree.SearchMLTFile(resourcePath);
                this.ResourceDirectoryPath.Value = resourcePath;
            }
            
            // パスの変更時に、次回の起動時用のパスとして保存する
            this.ResourceDirectoryPath.Subscribe(s => 
                {
                    Properties.Settings.Default.MLTResourcePath = s;
                    Properties.Settings.Default.Save();
                });

            // グループタブ初期化
            this.initGroupTab();
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
        private MLTViewerWindowViewModel _vm = new MLTViewerWindowViewModel();

        public MLTViewerWindow()
        {
            InitializeComponent();

            // ViewModelの初期化
            this._vm.View = new WeakReference<MLTViewerWindow>(this);
            this._vm.Init();

            // DataContextの設定
            this.DataContext = _vm;
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

            this._vm.TreeItemDoubleClickCommand.Execute(node);
        }
    }
}
