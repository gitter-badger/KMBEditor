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

    public class GroupTabContext
    {
        public ReactiveProperty<string> TabHeaderName { get; private set; }
            = new ReactiveProperty<string>();
        public ObservableCollection<MLTFile> TabFileList { get; private set; }
            = new ObservableCollection<MLTFile>();
    }

    /// <summary>
    /// MLTViewerWindow の ViewModel
    /// </summary>
    public class MLTViewerWindowViewModel
    {
        /// <summary>
        /// Viewのインスタンスを保持
        /// </summary>
        public WeakReference<MLTViewerWindow> View { get; set; }

        private MLTFileTreeClass _mlt_file_tree { get; set; } = new MLTFileTreeClass();

        public ObservableCollection<GroupTabContext> GroupTabList { get; private set; }
            = new ObservableCollection<GroupTabContext>();

        public ReactiveProperty<string> ResourceDirectoryPath { get; private set; } = new ReactiveProperty<string>("");
        public ReactiveProperty<List<MLTFileTreeNode>> MLTFileTreeNodes { get; private set; } = new ReactiveProperty<List<MLTFileTreeNode>>();
        public ReactiveCommand OpenResourceDirectoryCommand { get; private set; } = new ReactiveCommand();
        public ReactiveCommand<MLTFileTreeNode> TreeItemSelectCommand { get; private set; } = new ReactiveCommand<MLTFileTreeNode>();
        public ReactiveCommand<MLTFileTreeNode> TreeItemDoubleClickCommand { get; private set; } = new ReactiveCommand<MLTFileTreeNode>();

        private string openResourceDirectory()
        {
            using (var dialog = new WinForms.FolderBrowserDialog())
            {
                dialog.Description = "読み込む対象ディレクトリを指定してください";
                dialog.SelectedPath = this.ResourceDirectoryPath.Value; // 前回選択したディレクトリへのパスを初期値として開始

                // 選択されなかった場合はなにもしない
                if (dialog.ShowDialog() != WinForms.DialogResult.OK)
                {
                    return ""; // FIXME: このままだとエラーになるので要修正
                }

                // 選択されたディレクトリを起点に、MLTファイルを探索
                this.MLTFileTreeNodes.Value = this._mlt_file_tree.SearchMLTFile(dialog.SelectedPath);

                // 選択されたディレクトリへのパスを返す
                return dialog.SelectedPath;
            }
        }

        private GroupTabContext getCurrentGroupTabContext()
        {
            return this.GroupTabList[0];
        }

        private void addNewTab(MLTFileTreeNode node)
        {
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

        private void addGroupTabCountext(GroupTabContext ctx)
        {
            var index = this.GroupTabList.IndexOf(ctx);

            // 追加されたグループタブを選択
            MLTViewerWindow obj;
            if (this.View.TryGetTarget(out obj))
            {
                obj.GroupTabControl.SelectedIndex = index;
            }

        }

        public void Init()
        {
            // コマンド定義
            this.OpenResourceDirectoryCommand.Subscribe(_ => this.ResourceDirectoryPath.Value = this.openResourceDirectory());
            this.TreeItemSelectCommand.Subscribe(this.updatePreviewTab);
            this.TreeItemDoubleClickCommand.Subscribe(this.addNewTab);
            this.GroupTabList.ObserveAddChanged().Subscribe(this.addGroupTabCountext);

            // FileTreeの初期化
            this.MLTFileTreeNodes.Value = this._mlt_file_tree.SearchMLTFile(@"C:\Users\user\Documents\AA\HukuTemp_v21.0_20161120\HukuTemp");

            // グループタブ初期化
            var ctx = new GroupTabContext();
            ctx.TabHeaderName.Value = "Group 1";
            ctx.TabFileList.Add(new MLTFile());
            this.GroupTabList.Add(ctx);
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
