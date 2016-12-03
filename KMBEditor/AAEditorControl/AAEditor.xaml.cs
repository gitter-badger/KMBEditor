using Reactive.Bindings;
using Reactive.Bindings.Extensions;
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
using System.Diagnostics;

namespace KMBEditor
{
    public class AAEditorViewModel
    {
        public ObservableCollection<int> LineNumberList { get; private set; } = new ObservableCollection<int> { 0 };

        public ReactiveProperty<string> Text { get; private set; }
        public ReactiveProperty<int> LineCount { set; private get; } = new ReactiveProperty<int>(0);

        public ReactiveCommand LineAddCommand { get; private set; } = new ReactiveCommand();
        public ReactiveCommand LineDeleteCommand { get; private set; }
        public ReactiveCommand LineResetCommand { get; private set; } = new ReactiveCommand();

        private void TextUpdateEvent(string s)
        {
            Debug.WriteLine("change Text");

            if (s == null) {
                return;
            }

            // 一旦行番号をクリア
            // FIXME: 差分更新対応
            this.LineResetCommand.Execute();

            // 行番号の設定
            // FIXME: 初期化時には一度に更新したほうが良い
            foreach (var c in s)
            {
                if (c == '\n')
                {
                    this.LineAddCommand.Execute();
                }
            }

            // +1
            this.LineAddCommand.Execute();
        }

        public AAEditorViewModel(ReactiveProperty<string> text_rp)
        {
            this.Text = text_rp;

            this.Text.Subscribe(s => this.TextUpdateEvent(s));

            this.LineDeleteCommand = this.LineCount
                .Select(v => v != 0)
                .ToReactiveCommand();

            this.LineAddCommand
                .Subscribe(_ =>
                {
                    this.LineNumberList.Add(this.LineNumberList.Count);
                    this.LineCount.Value = this.LineNumberList.Count;
                });
            this.LineDeleteCommand
                .Subscribe(_ =>
                {
                    this.LineNumberList.RemoveAt(this.LineNumberList.Count - 1);
                    this.LineCount.Value = this.LineNumberList.Count;
                });
            this.LineResetCommand
                .Subscribe(_ =>
                {
                    this.LineNumberList.Clear();
                    this.LineCount.Value = this.LineNumberList.Count;
                });
        }
    }

    /// <summary>
    /// AAEditor.xaml の相互作用ロジック
    /// </summary>
    public partial class AAEditor : UserControl
    {
        private AAEditorViewModel _vm;

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(AAEditor));

        public string Text
        {
            get { return (string)this.GetValue(TextProperty); }
            set { this.SetValue(TextProperty, value); }
        }

        public AAEditor()
        {
            InitializeComponent();

            this._vm = new AAEditorViewModel(
                this.ToReactiveProperty<string>(TextProperty));

            this.AAEditorUserControl.DataContext = _vm;
        }
    }
}
