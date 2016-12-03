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
using System.Windows.Shapes;
using WinForms = System.Windows.Forms;

namespace KMBEditor
{
    /// <summary>
    /// MLTViewerWindow の ViewModel
    /// </summary>
    public class MLTViewerWindowViewModel
    {
        private MLTFileTreeClass _mlt_file_tree { get; set; } = new MLTFileTreeClass();
        private MLTClass _current_preview_mlt { get; set; } = new MLTClass();

        public ReactiveProperty<string> PreviewText { get; private set; } = new ReactiveProperty<string>("");
        public ReactiveProperty<string> ResourceDirectoryPath { get; private set; } = new ReactiveProperty<string>("");
        public ReactiveProperty<List<MLTFileTreeNode>> MLTFileTreeNodes { get; private set; } = new ReactiveProperty<List<MLTFileTreeNode>>();

        public ReactiveCommand OpenResourceDirectoryCommand { get; private set; } = new ReactiveCommand();
        public ReactiveCommand PreviewTextUpdateCommand { get; private set; } = new ReactiveCommand();
        public ReactiveCommand PrevPageCommand { get; private set; } = new ReactiveCommand();
        public ReactiveCommand NextPageCommand { get; private set; } = new ReactiveCommand();
        public ReactiveCommand ShowAllPageCommand { get; private set; } = new ReactiveCommand();

        public string OpenResourceDirectory()
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

        public MLTViewerWindowViewModel()
        {
            // コマンド定義
            this.OpenResourceDirectoryCommand.Subscribe(_ => this.ResourceDirectoryPath.Value = this.OpenResourceDirectory());
            this.PreviewTextUpdateCommand.Subscribe(obj =>
                {
                    MLTFileTreeNode node = (MLTFileTreeNode)obj;
                    if (node.IsDirectory == false)
                    {
                        this.PreviewText.Value = this._current_preview_mlt.OpenMLTFile(node.Path);
                    }
                });
            this.PrevPageCommand.Subscribe(_ => this.PreviewText.Value = this._current_preview_mlt.GetPrevPage());
            this.NextPageCommand.Subscribe(_ => this.PreviewText.Value = this._current_preview_mlt.GetNextPage());
            this.ShowAllPageCommand.Subscribe(_ => this.PreviewText.Value = this._current_preview_mlt.GetRawMLT());

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
        public MLTViewerWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// <para>クローズボタンが押された時のイベント</para>
        /// <para>再インデックスに時間がかかるため、Closeはせずに隠すだけにする</para>
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
