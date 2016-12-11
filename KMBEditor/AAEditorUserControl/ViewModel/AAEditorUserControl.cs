using KMBEditor.Util.StringExtentions;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace KMBEditor.AAEditorUserControl.ViewModel
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
            BindableTextBlock textBlock = (BindableTextBlock)sender;
            textBlock.Inlines.Clear();
            textBlock.Inlines.AddRange((ObservableCollection<Inline>)e.NewValue);
        }
    }

    public class VisualLine
    {
        public ReactiveProperty<string> Line { get; private set; } = new ReactiveProperty<string>("");
        public ObservableCollection<Inline> InlineList { get; private set; } = new ObservableCollection<Inline>();

        private bool isVisibleZenkakuSpace = true;
        private bool isVisibleHankakuSpace = true;

        private void updateSyntaxHighlight(string line)
        {
            this.InlineList.Clear();

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
                            // FIXME: その他の文字が一文字ずつRunでラップされてしまってるのでまとめて一つにする
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
                    Text = "⇂",
                    Foreground = new SolidColorBrush(Colors.Green)
                };
            inlines.Add(runend);

            // アップデート
            inlines.ForEach(this.InlineList.Add);
        }

        public VisualLine()
        {
            this.Line.Subscribe(s => this.updateSyntaxHighlight(s));
        }
    }

    public class AAEditorViewModel
    {
        public ObservableCollection<int> LineNumberList { get; private set; } = new ObservableCollection<int> { 1 };
        public ObservableCollection<VisualLine> VisualLineList { get; private set; } = new ObservableCollection<VisualLine>();

        public ReactiveProperty<string> BindingOriginalText { get; private set; }
        public ReactiveProperty<string> EditAreaText { get; private set; } = new ReactiveProperty<string>();

        private bool isUpdatedOringinalText = false;

        /// <summary>
        /// 行番号更新
        /// </summary>
        /// <param name="s">編集領域のテキスト</param>
        private void updateLineNumber(string s)
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

        /// <summary>
        /// 表示領域をすべて書き換え（初期化時）
        /// </summary>
        /// <param name="s"></param>
        private void updateAllVisualText(string s)
        {
            this.VisualLineList.Clear();

            foreach (var line in s.ReadLine())
            {
                var vl = new VisualLine();
                vl.Line.Value = line;
                this.VisualLineList.Add(vl);
            }
        }

        /// <summary>
        /// 表示領域を差分更新
        /// </summary>
        /// <param name="s"></param>
        private void updateDifferenceVisualText(string s)
        {
            // 現在の表示領域のインデックス最大値
            var maxIndex = this.VisualLineList.Count - 1;

            var index = 0;
            foreach (var line in s.ReadLine())
            {
                if (index <= maxIndex)
                {
                    // 既存表示領域に存在するライン
                    // 比較して差分があれば更新
                    // シンタックスハイライトの更新負荷が高いため、できるだけ更新しない
                    if (this.VisualLineList[index].Line.Value != line)
                    {
                        var vl = new VisualLine();
                        vl.Line.Value = line;
                        this.VisualLineList[index] = vl;
                    }
                }
                else
                {
                    // 追加ライン
                    var vl = new VisualLine();
                    vl.Line.Value = line;
                    this.VisualLineList.Add(vl);
                }

                index++;
            }
            
            // 行の削除があれば、削除された表示領域行を削除
            if (index - 1 < maxIndex)
            {
                for (var i = index - 1; i < maxIndex; i++)
                {
                    this.VisualLineList.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// 表示領域の更新
        /// </summary>
        /// <param name="s">編集領域のテキスト</param>
        private void updateVisualText(string s)
        {
            // 全差し替えかの判定
            if (this.isUpdatedOringinalText)
            {
                // 全更新
                this.updateAllVisualText(s);
                this.isUpdatedOringinalText = false;
            }
            else
            {
                // 差分更新
                this.updateDifferenceVisualText(s);
            }
        }

        /// <summary>
        /// バインディングされている元のテキストが変わった場合の処理 
        /// </summary>
        /// <param name="s">バインディングされている元のテキスト</param>
        private void updateBindingOringinalText(string s)
        {
            // オリジナルのテキストがアップデートされたかの状態フラグ有効化
            this.isUpdatedOringinalText = true;

            // 編集領域のテキストの更新
            this.EditAreaText.Value = s ?? "";
        }

        public AAEditorViewModel(ReactiveProperty<string> text_rp)
        {
            // AAEditorのDipendencyPropertyの取得
            this.BindingOriginalText = text_rp;

            // バインディングされているテキストが入れ替わった場合の処理
            // 入れ替わりのタイミングで編集領域のテキストを全書き換えする
            // 編集中の処理が重くなりすぎるため、直接変更はせず内部でキャッシュする
            // AAEditor側からの書き戻しのタイミングは保存時
            // FIXME: 現状変更内容があっても無視されるため、保存確認ダイアログを出力する
            this.BindingOriginalText.Subscribe(s => this.updateBindingOringinalText(s));

            // 編集領域のテキストが更新された時の処理
            this.EditAreaText.Subscribe(s => this.updateLineNumber(s));
            this.EditAreaText.Subscribe(s => this.updateVisualText(s));
        }
    }

}
