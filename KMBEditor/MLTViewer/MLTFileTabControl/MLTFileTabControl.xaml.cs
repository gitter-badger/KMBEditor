using KMBEditor.Model.MLT;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
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

namespace KMBEditor.MLTViewer.MLTFileTabControl
{
    public class MLTFileTabControlViewModel
    {
        /// <summary>
        /// MLTページ表示用タブのDataContext定義
        /// </summary>
        public class TabContext
        {
            /// <summary>
            /// タブのヘッダーテキスト
            /// </summary>
            public ReactiveProperty<string> TabHeaderName { get; private set; } = new ReactiveProperty<string>();
            /// <summary>
            /// PageTreeView用バインド用データ
            /// </summary>
            public ReactiveProperty<ObservableCollection<MLTPageIndex>> MLTPageIndexList { get; private set; }
                = new ReactiveProperty<ObservableCollection<MLTPageIndex>>();
            /// <summary>
            /// PageListView用バインド用データ
            /// </summary>
            public ReactiveProperty<ObservableCollection<MLTPage>> MLTPageList { get; set; }
                = new ReactiveProperty<ObservableCollection<MLTPage>>();
            /// <summary>
            /// 選択中ページの共有用変数
            /// </summary>
            public ReactiveProperty<MLTPage> SelectedItem { get; set; } = new ReactiveProperty<MLTPage>();
        }

        /// <summary>
        /// Viewのインスタンス
        /// </summary>
        public MLTFileTabControl View { get; set; }

        /// <summary>
        /// ViewのItemSourceと連動するリスト
        /// </summary>
        public ReactiveProperty<ObservableCollection<MLTFile>> MLTFileList { get; set; }
            = new ReactiveProperty<ObservableCollection<MLTFile>>();

        /// <summary>
        /// TabControlバインド用データ
        /// </summary>
        public ReactiveProperty<ObservableCollection<TabContext>> TabContextList { get; private set; }
            = new ReactiveProperty<ObservableCollection<TabContext>>(
                new ObservableCollection<TabContext>());

        /// <summary>
        /// MLTFileが入れ替わった場合の処理。TabContextListを生成
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private void addTabContextToList(MLTFile file)
        {
            if (file == null)
            {
                return;
            }

            var tabContext = new TabContext();
            tabContext.TabHeaderName.Value = file.Name;
            tabContext.MLTPageList.Value = file.Pages;
            tabContext.MLTPageIndexList.Value = this.createMLTIndexList(file);
            this.TabContextList.Value.Add(tabContext);
        }

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

        private void initMLTFileList(ObservableCollection<MLTFile> files)
        {
            if (files == null)
            {
                return;
            }

            // コレクションが追加された場合の処理
            files.ObserveAddChanged()
                .Subscribe(this.addTabContextToList);
        }

        public void Init()
        {
            // 初期状態としてPreviewタブを追加
            var tabContext = new TabContext();
            tabContext.TabHeaderName.Value = "[Preview]";
            this.TabContextList.Value.Add(tabContext);

            // MLTFileList自体が入れ替わった時の処理
            this.MLTFileList.Subscribe(this.initMLTFileList);
        }

        public MLTFileTabControlViewModel()
        {

        }
    }

    /// <summary>
    /// MLTFileTabControl.xaml の相互作用ロジック
    /// </summary>
    public partial class MLTFileTabControl : UserControl
    {
        private MLTFileTabControlViewModel _vm = new MLTFileTabControlViewModel();

        #region ItemSource
        public static readonly DependencyProperty MLTFileListProperty =
            DependencyProperty.Register(
                "ItemSource",
                typeof(ObservableCollection<MLTFile>),
                typeof(MLTFileTabControl));

        public ObservableCollection<MLTFile> ItemSource
        {
            get { return (ObservableCollection<MLTFile>)this.GetValue(MLTFileListProperty); }
            set { this.SetValue(MLTFileListProperty, value); }
        }
        #endregion

        public MLTFileTabControl()
        {
            InitializeComponent();

            this._vm.View = this;
            this._vm.MLTFileList = this.ToReactiveProperty<ObservableCollection<MLTFile>>(MLTFileListProperty);

            this._vm.Init();

            this.MLTFileTabControlGrid.DataContext = _vm;
        }
    }
}
