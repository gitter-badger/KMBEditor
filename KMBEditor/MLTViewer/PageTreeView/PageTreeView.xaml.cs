using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KMBEditor.MLTViewer.PageTreeView
{
    public class MLTPageTreeViewItemConverter : ReactiveConverter<RoutedPropertyChangedEventArgs<object>, MLTPageIndex>
    {
        protected override IObservable<MLTPageIndex> OnConvert(IObservable<RoutedPropertyChangedEventArgs<object>> e)
        {
            return e.Select(x => x.NewValue as MLTPageIndex);
        }
    }

    public class PageTreeViewViewModel
    {
        public ReactiveCommand<MLTPageIndex> MLTPageTreeViewItemSelectCommand { get; private set; } = new ReactiveCommand<MLTPageIndex>();
        public ReactiveProperty<ObservableCollection<MLTPageIndex>> MLTPageIndexList { get; set; }

        /// <summary>
        /// 項目リストの選択時のイベント
        /// </summary>
        /// <param name="node"></param>
        private void updateSelectMLTPage(MLTPageIndex node)
        {
            if (node != null)
            {
                // MLTページ項目リストを選択時にAAListBoxを該当のページまでスクロール
                //this.View.AAListBox.ScrollIntoView(node.Page);
            }
        }

        public PageTreeViewViewModel()
        {
            this.MLTPageTreeViewItemSelectCommand.Subscribe(obj => this.updateSelectMLTPage(obj));
        }
    }

    /// <summary>
    /// PageTreeView.xaml の相互作用ロジック
    /// </summary>
    public partial class PageTreeView : UserControl
    {
        private PageTreeViewViewModel _vm;

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

        public PageTreeView()
        {
            InitializeComponent();

            this._vm = new PageTreeViewViewModel();

            this._vm.MLTPageIndexList = this.ToReactiveProperty<ObservableCollection<MLTPageIndex>>(MLTPageIndexListProperty);

            this.PageTreeViewUserControl.DataContext = _vm;
        }
    }
}
