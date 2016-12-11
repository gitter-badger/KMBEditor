using KMBEditor.Model.MLT;
using KMBEditor.Model.MLTFileTree;
using KMBEditor.MLTViewer.View;
using Reactive.Bindings;
using Reactive.Bindings.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using WinForms = System.Windows.Forms;
using System.Windows.Data;

namespace KMBEditor.MLTViewer.ViewModel
{
    public class MLTFileTreeViewItemConverter : ReactiveConverter<RoutedPropertyChangedEventArgs<object>, MLTFileTreeNode>
    {
        protected override IObservable<MLTFileTreeNode> OnConvert(IObservable<RoutedPropertyChangedEventArgs<object>> e)
        {
            return e.Select(x => x.NewValue as MLTFileTreeNode);
        }
    }

    public class MLTPageTreeViewItemConverter : ReactiveConverter<RoutedPropertyChangedEventArgs<object>, MLTPageIndex>
    {
        protected override IObservable<MLTPageIndex> OnConvert(IObservable<RoutedPropertyChangedEventArgs<object>> e)
        {
            return e.Select(x => x.NewValue as MLTPageIndex);
        }
    }

    public class MLTPageIndex
    {
        public string Text { get; set; }
        public MLTPage Page { get; set; }
        public ObservableCollection<MLTPageIndex> Children { get; set; }
    }

    /// <summary>
    /// MLTViewerWindow の ViewModel
    /// </summary>
    public class MLTViewerWindowViewModel
    {
        /// <summary>
        /// Viewのインスタンスを保持
        /// </summary>
        public MLTViewerWindow View { get; private set; }

        private MLTFileTreeClass _mlt_file_tree { get; set; } = new MLTFileTreeClass();
        private MLTFile _current_preview_mlt { get; set; } = new MLTFile();

        public ReactiveProperty<string> TabName { get; private set; } = new ReactiveProperty<string>("");
        public ReactiveProperty<MLTPage> PreviewText { get; private set; } = new ReactiveProperty<MLTPage>();
        public ReactiveProperty<string> ResourceDirectoryPath { get; private set; } = new ReactiveProperty<string>("");
        public ReactiveProperty<List<MLTFileTreeNode>> MLTFileTreeNodes { get; private set; } = new ReactiveProperty<List<MLTFileTreeNode>>();
        public ReactiveProperty<ObservableCollection<MLTPage>> MLTPageList { get; private set; } = new ReactiveProperty<ObservableCollection<MLTPage>>();
        public ReactiveProperty<ObservableCollection<MLTPageIndex>> MLTPageIndexList { get; private set; } = new ReactiveProperty<ObservableCollection<MLTPageIndex>>();

        public ReactiveCommand OpenResourceDirectoryCommand { get; private set; } = new ReactiveCommand();
        public ReactiveCommand<MLTFileTreeNode> TreeItemSelectCommand { get; private set; } = new ReactiveCommand<MLTFileTreeNode>();
        public ReactiveCommand<MLTPageIndex> MLTPageTreeViewItemSelectCommand { get; private set; } = new ReactiveCommand<MLTPageIndex>();

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

        private void updateTabItemContext(MLTFileTreeNode node)
        {
            if (node.IsDirectory == false)
            {
                // MLTファイルのオープン
                this._current_preview_mlt.OpenMLTFile(node.Path);

                // タブ名の更新
                this.TabName.Value = node.Name;

                // インデックスリストの作成
                this.MLTPageIndexList.Value = this.createMLTIndexList();

                // AA一覧表示更新
                this.MLTPageList.Value = this._current_preview_mlt.Pages;
            }
        }

        /// <summary>
        /// 項目リストの選択時のイベント
        /// </summary>
        /// <param name="node"></param>
        private void updateSelectMLTPage(MLTPageIndex node)
        {
            if (node != null)
            {
                // MLTページ項目リストを選択時にAAListBoxを該当のページまでスクロール
                this.View.AAListBox.ScrollIntoView(node.Page);
            }
        }

        public MLTViewerWindowViewModel(MLTViewerWindow view)
        {
            // Viewのインスタンスを取得
            this.View = view;

            // コマンド定義
            this.OpenResourceDirectoryCommand.Subscribe(_ => this.ResourceDirectoryPath.Value = this.openResourceDirectory());
            this.TreeItemSelectCommand.Subscribe(obj => this.updateTabItemContext(obj));
            this.MLTPageTreeViewItemSelectCommand.Subscribe(obj => this.updateSelectMLTPage(obj));

            // FileTreeの初期化
            this.MLTFileTreeNodes.Value = this._mlt_file_tree.SearchMLTFile(@"C:\Users\user\Documents\AA\HukuTemp_v21.0_20161120\HukuTemp");
        }
    }

}
