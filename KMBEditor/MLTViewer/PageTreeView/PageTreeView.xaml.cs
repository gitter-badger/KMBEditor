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

namespace KMBEditor.MLTViewer.PageTreeView
{
    public class MLTPageTreeViewItemConverter : ReactiveConverter<RoutedPropertyChangedEventArgs<object>, Tuple<TreeViewItem, MLTPageIndex>>
    {
        protected override IObservable<Tuple<TreeViewItem, MLTPageIndex>> OnConvert(IObservable<RoutedPropertyChangedEventArgs<object>> e)
        {
            var item = this.AssociateObject as TreeViewItem;
            return e.Select(x => new Tuple<TreeViewItem, MLTPageIndex>(item, x.NewValue as MLTPageIndex));
        }
    }

    public class PageTreeViewViewModel
    {
        public PageTreeView View { get; set; }

        public ReactiveCommand<Tuple<TreeViewItem, MLTPageIndex>> MLTPageTreeViewItemSelectCommand { get; private set; }
            = new ReactiveCommand<Tuple<TreeViewItem, MLTPageIndex>>();

        public ReactiveProperty<ObservableCollection<MLTPageIndex>> MLTPageIndexList { get; set; }
        public ReactiveProperty<MLTPage> SelectedItem { get; set; }

        /// <summary>
        /// 項目リストの選択時のイベント
        /// </summary>
        /// <param name="node"></param>
        private void updateSelectMLTPage(Tuple<TreeViewItem, MLTPageIndex> node)
        {
            if (node != null && node.Item2 != null)
            {
                // 選択されているページのバインディング
                this.View.SelectedItem = node.Item2.Page;
                //node.Item1.BringIntoView();
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
                        node.IsSelected = true;
                        break;
                    }
                    else
                    {
                        if (node.Children == null)
                        {
                            continue;
                        }

                        foreach (var child in node.Children)
                        {
                            if (child.Page == page)
                            {
                                node.IsSelected = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void Init()
        {
            this.MLTPageTreeViewItemSelectCommand.Subscribe(obj => this.updateSelectMLTPage(obj));
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
    }
}
