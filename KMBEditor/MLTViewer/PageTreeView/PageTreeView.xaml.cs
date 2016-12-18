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
        /// 項目リストの選択時のイベント
        /// </summary>
        /// <param name="node"></param>
        private void updateCurrentPage(MLTPageIndex node)
        {
            if (node != null)
            {
                // 選択されているページのバインディング
                this.View.SelectedItem = node.Page;
            }
        }

        private void updateSelectedItemFromListView(MLTPage page)
        {
            if (page != null)
            {
                foreach (var node in this.MLTPageIndexList.Value)
                {
                    if (node.Page == page)
                    {
                        node.IsExpanded.Value = false;
                        node.IsSelected.Value = true;
                        break;
                    }
                    else
                    {
                        if (node.Children == null)
                        {
                            node.IsExpanded.Value = false;
                            continue;
                        }

                        foreach (var child in node.Children)
                        {
                            if (child.Page == page)
                            {
                                node.IsExpanded.Value = true;
                                child.IsSelected.Value = true;
                                break;
                            }
                            node.IsExpanded.Value = false;
                        }
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
    }
}
