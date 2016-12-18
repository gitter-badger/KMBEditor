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
using Reactive.Bindings.Interactivity;
using System.Reactive.Linq;

namespace KMBEditor.MLTViewer.PageListView
{
    public class MLTPageListItemConverter : ReactiveConverter<SelectionChangedEventArgs, MLTPage>
    {
        protected override IObservable<MLTPage> OnConvert(IObservable<SelectionChangedEventArgs> e)
        {
            return e.Select(x => x.AddedItems[0] as MLTPage);
        }
    }

    public class PageListViewViewModel
    {
        public PageListView View { get; set; }

        public ReactiveCommand<MLTPage> MLTPageListItemSelectCommand { get; private set; } = new ReactiveCommand<MLTPage>();

        public ReactiveProperty<ObservableCollection<MLTPage>> MLTPageList { get; set; }
        public ReactiveProperty<MLTPage> SelectedItem { get; set; }

        private void updateSelectedItem(MLTPage page)
        {
            if (page != null)
            {
                this.View.SelectedItem = page;
            }
        }

        private void updateSelectedItemFromTreeView(MLTPage page)
        {
            this.View.AAListBox.ScrollIntoView(page);
        }

        public void Init()
        {
            this.MLTPageListItemSelectCommand.Subscribe(x => this.updateSelectedItem(x));
            this.SelectedItem.Subscribe(x => this.updateSelectedItemFromTreeView(x));
        }

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

        #region ItemSource
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
        #endregion

        #region SelectedItem
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                "SelectedItem",
                typeof(MLTPage),
                typeof(PageListView));

        public MLTPage SelectedItem
        {
            get { return (MLTPage)this.GetValue(SelectedItemProperty); }
            set { this.SetValue(SelectedItemProperty, value); }
        }
        #endregion

        public PageListView()
        {
            InitializeComponent();

            this._vm = new PageListViewViewModel();

            this._vm.View = this;

            this._vm.MLTPageList = this.ToReactiveProperty<ObservableCollection<MLTPage>>(MLTPageListProperty);
            this._vm.SelectedItem = this.ToReactiveProperty<MLTPage>(SelectedItemProperty);

            this._vm.Init();

            this.PageListViewUserControl.DataContext = _vm;
        }
    }
}
