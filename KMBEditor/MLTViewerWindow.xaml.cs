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
        public ReactiveProperty<string> ResourceDirectoryPath { get; private set; }
        public ReactiveProperty<List<MLTFileTreeNode>> MLTFileTreeNodes { get; private set; }

        public ReactiveCommand OpenResourceDirectoryCommand { get; private set; }


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
                var mlt = new MLTFileTreeClass();
                this.MLTFileTreeNodes.Value = mlt.SearchMLTFile(dialog.SelectedPath);

                // 選択されたディレクトリへのパスを返す
                return dialog.SelectedPath;
            }
        }

        public MLTViewerWindowViewModel()
        {
            // 変数初期化
            this.ResourceDirectoryPath = new ReactiveProperty<string>("");
            this.MLTFileTreeNodes = new ReactiveProperty<List<MLTFileTreeNode>>(
                new List<MLTFileTreeNode> {
                    new MLTFileTreeNode
                    {
                        Name = "test1",
                        Path = "test1",
                        Children = new List<MLTFileTreeNode>
                        {
                            new MLTFileTreeNode
                            {
                                Name = "test2",
                                Path = "test2",
                                Children = new List<MLTFileTreeNode>
                                {
                                    new MLTFileTreeNode
                                    {
                                        Name = "FileName2",
                                        Path = "FilePath2",
                                    }
                                },
                            }
                        }
                    }
                });

            // コマンド初期化
            this.OpenResourceDirectoryCommand = new ReactiveCommand();

            // コマンド定義
            this.OpenResourceDirectoryCommand.Subscribe(_ => this.ResourceDirectoryPath.Value = this.OpenResourceDirectory());
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
