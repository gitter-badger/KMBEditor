using KMBEditor.Model.MLT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace KMBEditor.MLTViewer.PageListView
{
    public class PageListViewViewModel
    {
        public ReactiveProperty<ObservableCollection<MLTPage>> MLTPageList { get; set; }

        public PageListViewViewModel()
        {

        }
    }

    /// <summary>
    /// PageListView.xaml の相互作用ロジック
    /// </summary>
    public partial class PageListView : UserControl
    {
        private PageListViewViewModel _vm;

        public static readonly DependencyProperty MLTPageListProperty =
            DependencyProperty.Register(
                "ItemSource",
                typeof(ObservableCollection<MLTPage>),
                typeof(PageListView));

        public ObservableCollection<MLTPage> ItemSource
        {
            get { return (ObservableCollection<MLTPage>)this.GetValue(MLTPageListProperty); }
            set { this.SetValue(MLTPageListProperty, value); }
        }

        public PageListView()
        {
            InitializeComponent();

            this._vm = new PageListViewViewModel();

            this._vm.MLTPageList = this.ToReactiveProperty<ObservableCollection<MLTPage>>(MLTPageListProperty);

            this.PageListViewUserControl.DataContext = _vm;
        }
    }
}
