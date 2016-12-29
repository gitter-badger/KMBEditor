using Reactive.Bindings;
using System;
using System.Windows;

namespace KMBEditor.CustomDialog.RenameDialog
{
    /// <summary>
    /// RenameDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class RenameDialogWindow : Window
    {
        /// <summary>
        /// TextBoxとのバインド用変数
        /// </summary>
        public ReactiveProperty<string> InputText { get; private set; } = new ReactiveProperty<string>();
        /// <summary>
        /// OKボタンクリック時のコマンド
        /// </summary>
        public ReactiveCommand OkButtonClickCommand { get; private set; } = new ReactiveCommand();
        /// <summary>
        /// Cancelボタンクリック時のコマンド
        /// </summary>
        public ReactiveCommand CancelButtonClickCommand { get; private set; } = new ReactiveCommand();

        /// <summary>
        /// 結果取得用の外部公開プロパティ
        /// </summary>
        public string ResponseText { get; private set; }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="defaultText">入力テキストの初期値</param>
        public RenameDialogWindow(string defaultText)
        {
            InitializeComponent();

            // 変更前の値を保持
            this.ResponseText = defaultText;

            // テキストボックスの表示文字の初期化
            this.InputText.Value = defaultText;

            // コマンドの初期化
            this.OkButtonClickCommand.Subscribe(_ =>
            {
                // 結果をプロパティに代入して終了
                this.ResponseText = this.InputText.Value;
                this.DialogResult = true;
            });
            this.CancelButtonClickCommand.Subscribe(_ =>
            {
                // 特になにもせずに終了
                this.DialogResult = true;
            });

            // データコンテキストの設定
            this.DataContext = this;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // 入力テキストボックスを選択状態にする
            this.InputTextBox.SelectAll();
            this.InputTextBox.Focus();
        }
    }
}
