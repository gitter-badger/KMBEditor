using KMBEditor.Model.MLT;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KMBEditor.MLTViewer.MLTFileTabControl
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
        public ReactiveProperty<MLTPage> SelectedItem { get; private set; } = new ReactiveProperty<MLTPage>();
    }

    public class MLTFileTabControlViewModel
    {
        /// <summary>
        /// Viewのインスタンス
        /// </summary>
        public WeakReference<MLTFileTabControl> View { get; set; }

        /// <summary>
        /// ViewのItemSourceと連動するリスト
        /// </summary>
        public ReactiveProperty<ObservableCollection<MLTFile>> MLTFileList { get; private set; }
            = new ReactiveProperty<ObservableCollection<MLTFile>>();

        /// <summary>
        /// TabControlバインド用データ
        /// </summary>
        public ObservableCollection<TabContext> TabContextList { get; private set; }
                = new ObservableCollection<TabContext>();

        /// <summary>
        /// Tab削除コマンド
        /// </summary>
        public ReactiveCommand<TabContext> DeleteTabCommand { get; private set; }
            = new ReactiveCommand<TabContext>();

        /// <summary>
        /// MLTFileが追加された場合の処理。TabContextListを生成
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

            // 追加したタブを選択
            MLTFileTabControl obj;
            if (this.View.TryGetTarget(out obj))
            {
                obj.tabControl.SelectedIndex = this.TabContextList.Count - 1;
            }
        }

        /// <summary>
        /// MLTFileの入れ替えが発生した場合の処理
        /// </summary>
        /// <param name="newFile"></param>
        /// <param name="oldFile"></param>
        private void replaceTabContext(MLTFile newFile, MLTFile oldFile)
        {
            var item = this.TabContextList.First(x => x.MLTFile.Value == oldFile);
            var index = this.TabContextList.IndexOf(item);

            // indexが0ならプレビュー用のプレフィックスを追加
            Func<string> getName = () => {
                if (index == 0)
                {
                    return $"[Preview] {newFile.Name}";
                }
                return newFile.Name;
            };

            var tabContext = new TabContext();
            tabContext.TabHeaderName.Value = getName();
            tabContext.MLTFile.Value = newFile;

            // 入れ替え
            this.TabContextList[index] = tabContext;

            // 入れ替えたタブを選択
            MLTFileTabControl obj;
            if (this.View.TryGetTarget(out obj))
            {
                obj.tabControl.SelectedIndex = index;
            }
        }

        /// <summary>
        /// MLTFileListの初期化
        /// </summary>
        /// <param name="files"></param>
        private void initMLTFileList(ObservableCollection<MLTFile> files)
        {
            if (files == null)
            {
                return;
            }

            // 既存のファイルのタブを追加
            // 基本的にはPreviewTabのみの追加
            foreach (var file in files)
            {
                this.addTabContextToList(file);
            }

            // コレクション状態変更時の処理を追加
            files.ObserveAddChanged().Subscribe(this.addTabContextToList);
            files.ObserveReplaceChanged().Subscribe(x => this.replaceTabContext(x.NewItem, x.OldItem));
        }

        /// <summary>
        /// タブの削除
        /// </summary>
        /// <param name="tabctx"></param>
        private void deleteTab(TabContext tabctx)
        {
            var index = this.TabContextList.IndexOf(tabctx);

            // Previewタブの場合は削除しない
            if (index == 0)
            {
                return;
            }

            // タブを削除
            this.TabContextList.Remove(tabctx);

            // 元のMLTFileを削除
            this.MLTFileList.Value.Remove(tabctx.MLTFile.Value);
            tabctx.MLTFile.Value = null;
        }

        /// <summary>
        /// ViewModelの初期化
        /// </summary>
        public void Init()
        {
            // MLTFileList自体が入れ替わった時の処理
            this.MLTFileList.Subscribe(this.initMLTFileList);

            // コマンド初期化
            this.DeleteTabCommand.Subscribe(this.deleteTab);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
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
                typeof(MLTFileTabControl),
                new PropertyMetadata(
                    null,
                    (d, e) => (d as MLTFileTabControl)._vm.MLTFileList.Value = e.NewValue as ObservableCollection<MLTFile>));

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
            this._vm.View = new WeakReference<MLTFileTabControl>(this);
            this._vm.Init();

            // UserControlのDataContextの設定
            this.MLTFileTabControlGrid.DataContext = _vm;
        }

        /// <summary>
        /// タブの削除ボタンを押したときのイベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var tab = sender as Button;
            var tabContext = tab.DataContext as TabContext;

            this._vm.DeleteTabCommand.Execute(tabContext);
        }
    }
}
