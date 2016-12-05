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
    public class VisualLine
    {
        public string Line { get; set; }
    }

    public class AAEditorViewModel
    {
        public ObservableCollection<int> LineNumberList { get; private set; } = new ObservableCollection<int> { 1 };
        public ObservableCollection<VisualLine> VisualLineList { get; private set; } = new ObservableCollection<VisualLine>();

        public ReactiveProperty<string> Text { get; private set; }

        private bool isVisibleZenkakuSpace = true;
        private bool isVisibleHankakuSpace = true;

        private void TextUpdateEvent(string s)
        {
            // テキストが未定義の場合はなにもしない
            if (s == null) {
                return;
            }

            // ライン数を計算
            var new_count = s.GetLineCount();
            var old_count = LineNumberList.Count;

            if (new_count == old_count)
            {
                // 更新がなければそのまま返す
                return;
            }
            else if (new_count < old_count)
            {
                // ライン数が減少している場合
                for (var i = old_count; i > new_count; --i)
                {
                    // 後ろから削除(indexなので-1する)
                    this.LineNumberList.RemoveAt(i - 1);
                }
            }
            else if (new_count > old_count)
            {
                // ライン数が増加している場合
                for (var i = old_count; i < new_count; ++i)
                {
                    // 差分を追加
                    this.LineNumberList.Add(i + 1);
                }
            }
        }

        private void updateVisualText(string s)
        {
            // Visual Textの差し替え
            // FIXME: 1文字ごとに全差し替えなのでめっちゃ重い
            this.VisualLineList.Clear();

            foreach (var line in s.ReadLine())
            {
                this.VisualLineList.Add(new VisualLine
                {
                    Line = line
                });
#if false
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
#endif
            }
        }

        public AAEditorViewModel(ReactiveProperty<string> text_rp)
        {
            this.Text = text_rp;

            this.Text.Subscribe(s => this.TextUpdateEvent(s));
            this.Text.Subscribe(s => this.updateVisualText(s));
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
