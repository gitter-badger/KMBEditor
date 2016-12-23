using KMBEditor.Model.MLT;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Interactivity;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KMBEditor.MLTViewer.PageTreeView
{
    /// <summary>
    /// 見出し管理用ツリー
    /// </summary>
    public class MLTPageIndex
    {
        /// <summary>
        /// 見出し名('^【.*】$'の場合) or ASTでのページ名
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 見出しページ or AAページ
        /// </summary>
        public MLTPage Page { get; set; }
        /// <summary>
        /// TreeViewで下位ツリーを表示されているかの状態
        /// </summary>
        public ReactiveProperty<bool> IsExpanded { get; set; } = new ReactiveProperty<bool>(false);
        /// <summary>
        /// TreeViewで選択されているかの状態
        /// </summary>
        public ReactiveProperty<bool> IsSelected { get; set; } = new ReactiveProperty<bool>(false);
        /// <summary>
        /// 見出しページ以下のAAページを管理（MLTの仕様では、通常は1階層のみ）
        /// </summary>
        public ObservableCollection<MLTPageIndex> Children { get; set; }
    }

    /// <summary>
    /// ViewModelの定義
    /// </summary>
    public class PageTreeViewViewModel
    {
        /// <summary>
        /// Viewのインスタンス
        /// </summary>
        public WeakReference<PageTreeView> View { get; set; }

        /// <summary>
        /// 表示対象のMLTFileの保持
        /// </summary>
        public ReactiveProperty<MLTFile> MLTFile { get; set; }
            = new ReactiveProperty<MLTFile>();

        /// <summary>
        /// 選択中のアイテムの保持、ListViewとの共有用
        /// </summary>
        public ReactiveProperty<MLTPage> SelectedItem { get; set; }
            = new ReactiveProperty<MLTPage>();

        /// <summary>
        /// TreeViewとバインドする用のデータ
        /// </summary>
        public ReactiveProperty<ObservableCollection<MLTPageIndex>> MLTPageIndexList { get; private set; }
            = new ReactiveProperty<ObservableCollection<MLTPageIndex>>();

        /// <summary>
        /// MLTFileからMLTPageIndexの生成
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private ObservableCollection<MLTPageIndex> createMLTIndexList(MLTFile file)
        {
            // メモ：縦線が揃わなくて見た目がひどいので、インデックス値は０埋めで揃える

            var mltPageIndexList = new ObservableCollection<MLTPageIndex>();
            ObservableCollection<MLTPageIndex> children = null;
            var isCaptionChild = false;
            foreach (var page in file.Pages)
            {
                if (page.IsCaption)
                {
                    children = new ObservableCollection<MLTPageIndex>();
                    mltPageIndexList.Add(new MLTPageIndex
                        {
                            Text = string.Format("{0:D3}. {1}", page.Index, page.RawText),
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

        /// <summary>
        /// MLTFileが更新された時の処理
        /// </summary>
        /// <param name="file"></param>
        private void updateMLTFile(MLTFile file)
        {
            if (file != null)
            {
                // TreeViewを更新
                this.MLTPageIndexList.Value = this.createMLTIndexList(file);
            }
        }

        /// <summary>
        /// ListViewの選択に合わせて、TreeViewの状態を更新
        /// </summary>
        /// <param name="page"></param>
        private void updateSelectedItemFromListView(MLTPage page)
        {
            if (page == null || this.MLTPageIndexList.Value == null)
            {
                return;
            }

            foreach (var node in this.MLTPageIndexList.Value)
            {
                if (node.Page == page)
                {
                    // 一致したら選択状態にする(一応子要素は閉じる)
                    node.IsExpanded.Value = false;
                    node.IsSelected.Value = true;
                    break;
                }
                else
                {
                    // 子要素のチェック
                    if (node.Children == null)
                    {
                        // なければ選択状態を解除
                        node.IsExpanded.Value = false;
                        continue;
                    }

                    // 子要素のチェック
                    // MLTの仕様として、子要素の確認は１段のみでよい
                    foreach (var child in node.Children)
                    {
                        if (child.Page == page)
                        {
                            // 一致ししたら子要素を開いて、選択状態にする
                            node.IsExpanded.Value = true;
                            child.IsSelected.Value = true;
                            break;
                        }

                        // 一致しなかったら子要素を閉じて、選択状態を解除
                        node.IsExpanded.Value = false;
                        child.IsSelected.Value = false;
                    }
                }
            }
        }

        /// <summary>
        /// ViewModelの初期化
        /// </summary>
        public void Init()
        {
            // プロパティの初期化
            this.MLTFile.Subscribe(this.updateMLTFile);
            this.SelectedItem.Subscribe(this.updateSelectedItemFromListView);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PageTreeViewViewModel()
        {
            // Dependency Propertyの設定等が必要なため、ここで初期化はしない
        }
    }

    /// <summary>
    /// PageTreeView.xaml の相互作用ロジック
    /// </summary>
    public partial class PageTreeView : UserControl
    {
        /// <summary>
        /// ViewModelのインスタンス
        /// </summary>
        private PageTreeViewViewModel _vm = new PageTreeViewViewModel();

        #region ItemSource
        public static readonly DependencyProperty MLTFileProperty =
            DependencyProperty.Register(
                nameof(ItemSource),
                typeof(MLTFile),
                typeof(PageTreeView),
                new PropertyMetadata(
                    null,
                    (d, e) => (d as PageTreeView)._vm.MLTFile.Value = e.NewValue as MLTFile));

        public MLTFile ItemSource
        {
            get { return (MLTFile)this.GetValue(MLTFileProperty); }
            set { this.SetValue(MLTFileProperty, value); }
        }
        #endregion

        #region SelectedItem
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(MLTPage),
                typeof(PageTreeView),
                new PropertyMetadata(
                    null,
                    (d, e) => (d as PageTreeView)._vm.SelectedItem.Value = e.NewValue as MLTPage));

        public MLTPage SelectedItem
        {
            get { return (MLTPage)this.GetValue(SelectedItemProperty); }
            set { this.SetValue(SelectedItemProperty, value); }
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PageTreeView()
        {
            InitializeComponent();

            // ViewModelの初期化
            this._vm.View = new WeakReference<PageTreeView>(this);
            this._vm.Init();

            // UserControlのDataContextの設定
            this.PageTreeViewUserControl.DataContext = _vm;
        }

        private void TreeViewItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // SelectedItemChangedイベントの処理だと、ListBoxの選択時に交互に更新が走ってデッドロックする可能性がある
            // そのため、TreeViewのほうはMouseLeftButtonDownイベントで実装する(Selectでは更新はかけない）

            TreeViewItem item = (TreeViewItem)sender;

            var pageIndex = item.DataContext as MLTPageIndex;
            this.SelectedItem = pageIndex.Page;

            // ここら辺のFocusの処理がなんで必要なのかはいまいち不明瞭
            // http://stackoverflow.com/questions/30356236/mouseleftbuttondown-event-not-raised-in-treeviewitem-why
            item.Focusable = true;
            item.Focus();
            item.Focusable = false;
            e.Handled = true;
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            var s = sender as TreeViewItem;
            // 選択されているアイテムが可視領域に入るまでスクロール
            s.BringIntoView();
            e.Handled = true;
        }
    }
}
