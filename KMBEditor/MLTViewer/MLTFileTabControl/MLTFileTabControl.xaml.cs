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
            /// MLTFile
            /// </summary>
            public ReactiveProperty<MLTFile> MLTFile { get; private set; } = new ReactiveProperty<MLTFile>();
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
        public ObservableCollection<TabContext> TabContextList { get; private set; }
                = new ObservableCollection<TabContext>();

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
            tabContext.MLTFile.Value = file;
            this.TabContextList.Add(tabContext);
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
            this.TabContextList.Add(tabContext);

            // MLTFileList自体が入れ替わった時の処理
            this.MLTFileList.Subscribe(this.initMLTFileList);
        }

        public MLTFileTabControlViewModel()
        {
            // Dependency Propertyの設定等が必要なため、ここで初期化はしない
        }
    }

    /// <summary>
    /// MLTFileTabControl.xaml の相互作用ロジック
    /// </summary>
    public partial class MLTFileTabControl : UserControl
    {
        /// <summary>
        /// ViewModelのインスタンス
        /// </summary>
        private MLTFileTabControlViewModel _vm = new MLTFileTabControlViewModel();

        #region ItemSource
        public static readonly DependencyProperty MLTFileListProperty =
            DependencyProperty.Register(
                nameof(ItemSource),
                typeof(ObservableCollection<MLTFile>),
                typeof(MLTFileTabControl));

        public ObservableCollection<MLTFile> ItemSource
        {
            get { return (ObservableCollection<MLTFile>)this.GetValue(MLTFileListProperty); }
            set { this.SetValue(MLTFileListProperty, value); }
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MLTFileTabControl()
        {
            InitializeComponent();

            // ViewModelの初期化
            this._vm.View = this;
            this._vm.MLTFileList = this.ToReactiveProperty<ObservableCollection<MLTFile>>(MLTFileListProperty);

            this._vm.Init();

            // UserControlのDataContextの設定
            this.MLTFileTabControlGrid.DataContext = _vm;
        }
    }
}
