using KMBEditor.Model.MLT;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Interactivity;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KMBEditor.MLTViewer.PageTreeView
{
    public class PageTreeViewViewModel
    {
        public PageTreeView View { get; set; }

        public ReactiveProperty<ObservableCollection<MLTPageIndex>> MLTPageIndexList { get; set; }
        public ReactiveProperty<MLTPage> SelectedItem { get; set; }

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

        public void Init()
        {
            this.SelectedItem.Subscribe(x => this.updateSelectedItemFromListView(x));
        }

        public PageTreeViewViewModel()
        {
        }
    }

    /// <summary>
    /// PageTreeView.xaml の相互作用ロジック
    /// </summary>
    public partial class PageTreeView : UserControl
    {
        private PageTreeViewViewModel _vm;

        #region ItemSource
        public static readonly DependencyProperty MLTPageIndexListProperty =
            DependencyProperty.Register(
                "ItemSource",
                typeof(ObservableCollection<MLTPageIndex>),
                typeof(PageTreeView));

        public ObservableCollection<MLTPageIndex> ItemSource
        {
            get { return (ObservableCollection<MLTPageIndex>)this.GetValue(MLTPageIndexListProperty); }
            set { this.SetValue(MLTPageIndexListProperty, value); }
        }
        #endregion

        #region SelectedItem
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                "SelectedItem",
                typeof(MLTPage),
                typeof(PageTreeView));

        public MLTPage SelectedItem
        {
            get { return (MLTPage)this.GetValue(SelectedItemProperty); }
            set { this.SetValue(SelectedItemProperty, value); }
        }
        #endregion

        public PageTreeView()
        {
            InitializeComponent();

            this._vm = new PageTreeViewViewModel();
            this._vm.View = this;

            this._vm.MLTPageIndexList = this.ToReactiveProperty<ObservableCollection<MLTPageIndex>>(MLTPageIndexListProperty);
            this._vm.SelectedItem = this.ToReactiveProperty<MLTPage>(SelectedItemProperty);

            this._vm.Init();

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
