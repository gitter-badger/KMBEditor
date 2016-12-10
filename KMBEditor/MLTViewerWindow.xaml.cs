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
using System.Windows.Shapes;
using WinForms = System.Windows.Forms;

namespace KMBEditor
{
    /// <summary>
    /// MLTViewerWindow の ViewModel
    /// </summary>
    public class MLTViewerWindowViewModel
    {
        public class MLTPageIndex
        {
            public string Text { get; set; }
            public MLT.MLTPage Page { get; set; }
            public ObservableCollection<MLTPageIndex> Children { get; set; }
        }

        private MLTFileTreeClass _mlt_file_tree { get; set; } = new MLTFileTreeClass();
        private MLT.MLTFile _current_preview_mlt { get; set; } = new MLT.MLTFile();

        public ReactiveProperty<MLT.MLTPage> PreviewText { get; private set; } = new ReactiveProperty<MLT.MLTPage>();
        public ReactiveProperty<string> ResourceDirectoryPath { get; private set; } = new ReactiveProperty<string>("");
        public ReactiveProperty<List<MLTFileTreeNode>> MLTFileTreeNodes { get; private set; } = new ReactiveProperty<List<MLTFileTreeNode>>();
        public ReactiveProperty<ObservableCollection<MLT.MLTPage>> MLTPageList { get; private set; } = new ReactiveProperty<ObservableCollection<MLT.MLTPage>>();
        public ReactiveProperty<ObservableCollection<MLTPageIndex>> MLTPageIndexList { get; private set; } = new ReactiveProperty<ObservableCollection<MLTPageIndex>>();

        public ReactiveCommand OpenResourceDirectoryCommand { get; private set; } = new ReactiveCommand();
        public ReactiveCommand PreviewTextUpdateCommand { get; private set; } = new ReactiveCommand();

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

        private ObservableCollection<MLTPageIndex> createMLTIndexList()
        {
            // メモ：縦線が揃わなくて見た目がひどいので、インデックス値は０埋めで揃える

            var mltPageIndexList = new ObservableCollection<MLTPageIndex>();
            ObservableCollection<MLTPageIndex> children = null;
            var isCaptionChild = false;
            foreach (var page in this._current_preview_mlt.Pages)
            {
                if (page.IsCaption)
                {
                    children = new ObservableCollection<MLTPageIndex>();
                    mltPageIndexList.Add(new MLTPageIndex
                        {
                            Text = string.Format("{0:D3}. {1}", page.Index, page.AA),
                            Page = page,
                            Children = children
                        });
                    isCaptionChild = true;
                }
                else
                {
                    if (isCaptionChild)
                    {
                        children.Add(new MLTPageIndex
                            {
                                Text = string.Format("{0:D3}. {1}", page.Index, page.Name),
                                Page = page
                            });
                    }
                    else
                    {
                        mltPageIndexList.Add(new MLTPageIndex
                            {
                                Text = string.Format("{0:D3}. {1}", page.Index, page.Name),
                                Page = page
                            });
                    }
                }
            }
            return mltPageIndexList;
        }

        public MLTViewerWindowViewModel()
        {
            // コマンド定義
            this.OpenResourceDirectoryCommand.Subscribe(_ => this.ResourceDirectoryPath.Value = this.openResourceDirectory());
            this.PreviewTextUpdateCommand.Subscribe(obj =>
                {
                    MLTFileTreeNode node = (MLTFileTreeNode)obj;
                    if (node.IsDirectory == false)
                    {
                        // MLTファイルのオープン
                        this._current_preview_mlt.OpenMLTFile(node.Path);

                        // インデックスリストの作成
                        this.MLTPageIndexList.Value = this.createMLTIndexList();

                        // AA一覧表示更新
                        this.MLTPageList.Value = this._current_preview_mlt.Pages;
                    }
                });

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
