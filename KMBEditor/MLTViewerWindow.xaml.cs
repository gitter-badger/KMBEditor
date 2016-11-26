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
        private MLTFileTreeClass _mlt_file_tree { get; set; }
        private MLTClass _current_preview_mlt { get; set; }

        public ReactiveProperty<string> PreviewText { get; private set; }
        public ReactiveProperty<string> ResourceDirectoryPath { get; private set; }
        public ReactiveProperty<List<MLTFileTreeNode>> MLTFileTreeNodes { get; private set; }

        public ReactiveCommand OpenResourceDirectoryCommand { get; private set; }
        public ReactiveCommand PreviewTextUpdateCommand { get; private set; }
        public ReactiveCommand PrevPageCommand { get; private set; }
        public ReactiveCommand NextPageCommand { get; private set; }
        public ReactiveCommand ShowAllPageCommand { get; private set; }

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
            // 変数初期化
            this._mlt_file_tree = new MLTFileTreeClass();
            this._current_preview_mlt = new MLTClass();
            this.PreviewText = new ReactiveProperty<string>("");
            this.ResourceDirectoryPath = new ReactiveProperty<string>("");
            this.MLTFileTreeNodes = new ReactiveProperty<List<MLTFileTreeNode>>();

            // コマンド初期化
            this.OpenResourceDirectoryCommand = new ReactiveCommand();
            this.PreviewTextUpdateCommand = new ReactiveCommand();
            this.PrevPageCommand = new ReactiveCommand();
            this.NextPageCommand = new ReactiveCommand();
            this.ShowAllPageCommand = new ReactiveCommand();

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
    }
}
