using Reactive.Bindings;
using System;
using System.Collections.Generic;
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
    public class Person
    {
        public string Name { get; set; }
        public List<Person> Children { get; set; }
    }


    /// <summary>
    /// MLTViewerWindow の ViewModel
    /// </summary>
    public class MLTViewerWindowViewModel
    {
        public ReactiveProperty<string> resource_path { get; private set; }

        public ReactiveCommand OpenResourceDirectoryCommand { get; private set; }

        public string OpenResourceDirectory()
        {
            using (var dialog = new WinForms.FolderBrowserDialog())
            {
                dialog.Description = "読み込む対象ディレクトリを指定してください";
                dialog.SelectedPath = this.resource_path.Value; // 前回選択したディレクトリへのパスを初期値として開始

                // 選択されなかった場合はなにもしない
                if (dialog.ShowDialog() != WinForms.DialogResult.OK)
                {
                    return ""; // FIXME: このままだとエラーになるので要修正
                }

                // 選択されたディレクトリへのパスを返す
                return dialog.SelectedPath;
            }
        }

        public MLTViewerWindowViewModel()
        {
            // 変数初期化
            this.resource_path = new ReactiveProperty<string>("");

            // コマンド初期化
            this.OpenResourceDirectoryCommand = new ReactiveCommand();

            // コマンド定義
            this.OpenResourceDirectoryCommand.Subscribe(_ => this.resource_path.Value = this.OpenResourceDirectory());
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

            this.treeView.ItemsSource = new List<Person>
            {
                new Person
                {
                    Name = "田中　太郎",
                    Children = new List<Person>
                    {
                        new Person { Name = "田中　花子" },
                        new Person { Name = "田中　一郎" },
                        new Person
                        {
                            Name = "木村　貫太郎",
                            Children = new List<Person>
                            {
                                new Person { Name = "木村　はな" },
                                new Person { Name = "木村　梅" },
                            }
                        }
                    }
                },
                new Person
                {
                    Name = "田中　次郎",
                    Children = new List<Person>
                    {
                        new Person { Name = "田中　三郎" }
                    }
                }
            };
        }
    }
}
