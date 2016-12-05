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
using KMBEditor.StringExtentions;
using System.Collections.Specialized;

namespace KMBEditor.AAEditorUserControl
{
    public class BindableTextBlock : TextBlock
    {
        public static readonly DependencyProperty InlineListProperty =
            DependencyProperty.Register(
                "InlineList",
                typeof(ObservableCollection<Inline>),
                typeof(BindableTextBlock),
                new UIPropertyMetadata(null, OnPropertyChanged));

        public ObservableCollection<Inline> InlineList
        {
            get { return (ObservableCollection<Inline>)GetValue(InlineListProperty); }
            set { SetValue(InlineListProperty, value); }
        }

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = sender as BindableTextBlock;
            var list = e.NewValue as ObservableCollection<Inline>;
            list.CollectionChanged += new NotifyCollectionChangedEventHandler(textBlock.InlineCollectionChanged);
        }

        private void InlineCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                Debug.WriteLine("Inline Collection Add");
                var idx = e.NewItems.Count - 1;
                var run = e.NewItems[idx] as Inline;
                this.Inlines.Add(run);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                Debug.WriteLine("Inline Collection Reset");
                this.Inlines.Clear();
            }
        }

        public BindableTextBlock()
        {
        }
    }

    public class AAEditorViewModel
    {
        public ObservableCollection<int> LineNumberList { get; private set; } = new ObservableCollection<int> { 0 };
        public ObservableCollection<Inline> FormatList { get; private set; } = new ObservableCollection<Inline> {};

        public ReactiveProperty<string> Text { get; private set; }
        public ReactiveProperty<int> LineCount { set; private get; } = new ReactiveProperty<int>(0);

        public ReactiveCommand LineAddCommand { get; private set; } = new ReactiveCommand();
        public ReactiveCommand LineDeleteCommand { get; private set; }
        public ReactiveCommand LineResetCommand { get; private set; } = new ReactiveCommand();

        private bool isVisibleZenkakuSpace = true;
        private bool isVisibleHankakuSpace = true;

        private void TextUpdateEvent(string s)
        {
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

        private void updateVisualText(string s)
        {
            // Visual Textの差し替え
            // FIXME: 1文字ごとに全差し替えなのでめっちゃ重い
            this.FormatList.Clear();
            foreach (var line in s.ReadLine())
            {
                var inlines = new List<Inline>();

                // 先頭空白文字の判定
                foreach (var c in line)
                {
                    switch (c)
                    {
                        case ' ':  // 半角スペース
                            if (this.isVisibleHankakuSpace) {
                                var run = new Run
                                {
                                    Text = c.ToString(),
                                    Foreground = new SolidColorBrush(Colors.LightBlue),
                                    TextDecorations = TextDecorations.Underline
                                };
                                inlines.Add(run);
                            }
                            break;
                        case '　': // 全角スペース
                            if (this.isVisibleZenkakuSpace)
                            {
                                var run = new Run
                                {
                                    Text = c.ToString(),
                                    Foreground = new SolidColorBrush(Colors.LightSlateGray),
                                    TextDecorations = TextDecorations.Underline
                                };
                                inlines.Add(run);
                            }
                            break;
                        default: // その他の文字
                            {
                                var run = new Run
                                {
                                    Text = c.ToString(),
                                    Foreground = new SolidColorBrush(Colors.Black)
                                };
                                inlines.Add(run);
                                break;
                            }
                    }
                }
                // 改行文字の追加
                var runend = new Run
                    {
                        Text = "↓" + System.Environment.NewLine,
                        Foreground = new SolidColorBrush(Colors.Green)
                    };
                inlines.Add(runend);

                // アップデート
                inlines.ForEach(this.FormatList.Add);
            }
        }

        public AAEditorViewModel(ReactiveProperty<string> text_rp)
        {
            this.Text = text_rp;

            this.Text.Subscribe(s => this.TextUpdateEvent(s));
            this.Text.Subscribe(s => this.updateVisualText(s));

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

            this.AAEditorUserControlGrid.DataContext = _vm;
        }
    }
}
