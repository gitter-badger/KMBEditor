using KMBEditor.Model.MLT;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Interactivity;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace KMBEditor.MLTViewer.PageListView
{
    /// <summary>
    /// ListViewの選択アイテムが変更された場合のConverter
    /// </summary>
    public class MLTPageListItemConverter : ReactiveConverter<SelectionChangedEventArgs, MLTPage>
    {
        protected override IObservable<MLTPage> OnConvert(IObservable<SelectionChangedEventArgs> e)
        {
            // AddItemsが空のリストの場合があるため、回避を入れる必要がある
            return e.Select(x => x.AddedItems.Count > 0 ? x.AddedItems[0] as MLTPage : null);
        }
    }

    /// <summary>
    /// ViewModelの定義
    /// </summary>
    public class PageListViewViewModel
    {
        /// <summary>
        /// Viewのインスタンス(Viewへの依存は可能な限り減らすこと)
        /// </summary>
        public WeakReference<PageListView> View { get; set; }

        /// <summary>
        /// 表示対象のMLTFile
        /// </summary>
        public ReactiveProperty<MLTFile> MLTFile { get; private set; }
            = new ReactiveProperty<MLTFile>();

        /// <summary>
        /// 選択中のアイテム保持/共有用変数
        /// </summary>
        public ReactiveProperty<MLTPage> SelectedItem { get; private set; }
            = new ReactiveProperty<MLTPage>();

        /// <summary>
        /// ListViewバインド用コレクション
        /// </summary>
        public ReactiveProperty<ObservableCollection<MLTPage>> MLTPageList { get; private set; }
            = new ReactiveProperty<ObservableCollection<MLTPage>>();

        /// <summary>
        /// アイテム選択時のコマンド
        /// </summary>
        public ReactiveCommand<MLTPage> MLTPageListItemSelectCommand { get; private set; }
            = new ReactiveCommand<MLTPage>();

        /// <summary>
        /// 設定されているMLTFileの更新
        /// </summary>
        /// <param name="file"></param>
        private void updateMLTFile(MLTFile file)
        {
            if (file != null)
            {
                this.MLTPageList.Value = file.Pages;
            }
        }

        /// <summary>
        /// ListViewの選択アイテム変更
        /// </summary>
        /// <param name="page"></param>
        private void updateSelectedItem(MLTPage page)
        {
            if (page != null)
            {
                PageListView obj;
                if (this.View.TryGetTarget(out obj))
                {
                    obj.SelectedItem = page;
                }
            }
        }

        /// <summary>
        /// TreeViewのアイテムが選択された場合のスクロール
        /// </summary>
        /// <param name="page"></param>
        private void updateSelectedItemFromTreeView(MLTPage page)
        {
            PageListView obj;
            if (this.View.TryGetTarget(out obj))
            {
                obj.AAListBox.ScrollIntoView(page);
            }
        }

        /// <summary>
        /// ViewModelの初期化
        /// </summary>
        public void Init()
        {
            // プロパティの初期化
            this.MLTFile.Subscribe(this.updateMLTFile);
            this.MLTPageListItemSelectCommand.Subscribe(this.updateSelectedItem);
            this.SelectedItem.Subscribe(this.updateSelectedItemFromTreeView);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PageListViewViewModel()
        {
            // Dependency Property等の受け渡しがあるため、ここでは初期化しない
        }
    }

    /// <summary>
    /// PageListView.xaml の相互作用ロジック
    /// </summary>
    public partial class PageListView : UserControl
    {
        /// <summary>
        /// ViewModelのインスタンス
        /// </summary>
        private PageListViewViewModel _vm = new PageListViewViewModel();

        #region ItemSource
        public static readonly DependencyProperty MLTFileProperty =
            DependencyProperty.Register(
                nameof(ItemSource),
                typeof(MLTFile),
                typeof(PageListView),
                new PropertyMetadata(
                    null,
                    (d, e) => (d as PageListView)._vm.MLTFile.Value = e.NewValue as MLTFile));

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
                typeof(PageListView),
                new PropertyMetadata(
                    null,
                    (d, e) => (d as PageListView)._vm.SelectedItem.Value = e.NewValue as MLTPage));

        public MLTPage SelectedItem
        {
            get { return (MLTPage)this.GetValue(SelectedItemProperty); }
            set { this.SetValue(SelectedItemProperty, value); }
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PageListView()
        {
            InitializeComponent();

            // ViewModel初期化
            this._vm.View = new WeakReference<PageListView>(this);
            this._vm.Init();

            // UserControl用のDataContextの設定
            this.PageListViewUserControl.DataContext = _vm;
        }
    }
}
