using KMBEditor.Model.MLT;
using KMBEditor.Model.MLTFileTree;
using Reactive.Bindings;
using Reactive.Bindings.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
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
    /// MLTViewerWindow の ViewModel
    /// </summary>
    public class MLTViewerWindowViewModel
    {
        /// <summary>
        /// Viewのインスタンスを保持
        /// </summary>
        public WeakReference<MLTViewerWindow> View { get; set; }

        private MLTFileTreeClass _mlt_file_tree { get; set; } = new MLTFileTreeClass();

        public ObservableCollection<MLTFile> TabFileList { get; private set; }
            = new ObservableCollection<MLTFile> { new MLTFile() };

        public ReactiveProperty<string> ResourceDirectoryPath { get; private set; } = new ReactiveProperty<string>("");
        public ReactiveProperty<List<MLTFileTreeNode>> MLTFileTreeNodes { get; private set; } = new ReactiveProperty<List<MLTFileTreeNode>>();
        public ReactiveCommand OpenResourceDirectoryCommand { get; private set; } = new ReactiveCommand();
        public ReactiveCommand<MLTFileTreeNode> TreeItemSelectCommand { get; private set; } = new ReactiveCommand<MLTFileTreeNode>();

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

        private void addNewTab(MLTFileTreeNode node)
        {
            if (node.IsDirectory == false)
            {
                var file = new MLTFile();

                // MLTファイルのオープン
                file.OpenMLTFile(node.Path);

                // タブの要素追加
                this.TabFileList.Add(file);
            }
        }

        private void updatePreviewTab(MLTFileTreeNode node)
        {
            if (node.IsDirectory == false)
            {
                var file = new MLTFile();

                // MLTファイルのオープン
                file.OpenMLTFile(node.Path);

                // MLTファイルの更新
                this.TabFileList[0] = file;
            }
        }

        public MLTViewerWindowViewModel()
        {
            // コマンド定義
            this.OpenResourceDirectoryCommand.Subscribe(_ => this.ResourceDirectoryPath.Value = this.openResourceDirectory());
            this.TreeItemSelectCommand.Subscribe(this.updatePreviewTab);

            // FileTreeの初期化
            this.MLTFileTreeNodes.Value = this._mlt_file_tree.SearchMLTFile(@"C:\Users\user\Documents\AA\HukuTemp_v21.0_20161120\HukuTemp");
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
    }
}
