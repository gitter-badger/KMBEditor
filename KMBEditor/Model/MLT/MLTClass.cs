using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using WinForms = System.Windows.Forms;
using KMBEditor.Util.StringExtentions;
using Reactive.Bindings;
using System.Diagnostics;
using System;

namespace KMBEditor.Model.MLT
{
    /// <summary>
    /// MLTファイルに属するページ単位での状態管理クラス
    /// </summary>
    public class MLTPage
    {
        /// <summary>
        /// ページ番号(1始まり）
        /// </summary>
        public int Index { get; set; } = 1;
        /// <summary>
        /// 上位の区切り文字
        /// MLTの一番上のページのみnull許容
        /// </summary>
        public string SplitterText { get; set; } = null;
        /// <summary>
        /// AST形式でのページ名(MLTの場合は定義なし)
        /// </summary>
        public string Name { get; set; } = null;
        /// <summary>
        /// ページの総バイト数
        /// </summary>
        public int Bytes { get; set; } = 0;
        /// <summary>
        /// ページの総行数(最低1行は存在)
        /// </summary>
        public int Lines { get; set; } = 1;
        /// <summary>
        /// 見出し属性の有無
        /// </summary>
        public bool IsCaption { get; set; } = false;
        /// <summary>
        /// ページのAAテキストデータ
        /// </summary>
        public string RawText { get; set; } = "";
        /// <summary>
        /// ページのAAテキストデータ
        /// </summary>
        public string DecodeText { get; set; } = "";
    }

    /// <summary>
    /// MLTファイル単位の状態管理クラス
    /// </summary>
    public class MLTFile
    {
        /// <summary>
        /// 現在管理しているファイル名
        /// </summary>
        public string Name { get; set; } = "(無題).mlt";

        /// <summary>
        /// 内包しているページのリスト
        /// </summary>
        public ObservableCollection<MLTPage> Pages { get; private set; }
            = new ObservableCollection<MLTPage> {};

        /// <summary>
        /// ファイルの絶対パス
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 表示中のページ番号
        /// </summary>
        private int current_page_num { get; set; }

        /// <summary>
        /// 現在のページを取得する
        /// </summary>
        /// <returns></returns>
        public MLTPage GetCurrentPage()
        {
            return this.Pages[this.current_page_num];
        }

        /// <summary>
        /// 前ページを開く
        /// </summary>
        /// <returns></returns>
        public MLTPage GetPrevPage()
        {
            if (this.current_page_num > 0)
            {
                this.current_page_num -= 1;
            }
            return this.Pages[this.current_page_num];
        }

        /// <summary>
        /// 次ページを開く
        /// </summary>
        /// <returns></returns>
        public MLTPage GetNextPage()
        {
            if (this.current_page_num < (this.Pages.Count -1))
            {
                this.current_page_num += 1;
            }
            return this.Pages[this.current_page_num];
        }

        /// <summary>
        /// 新規保存/上書き保存
        /// </summary>
        public void SaveMLTFile()
        {
            // 既存のファイルでなければ新規作成
            if (string.IsNullOrEmpty(this.FilePath))
            {
                // 新規保存
                this.SaveAsMLTFile();
            }
            else
            {
                // 上書き保存
                this.saveMLTFile(this.FilePath);
            }
        }

        /// <summary>
        /// 名前をつけて保存
        /// </summary>
        /// <returns>変更後のファイル名</returns>
        public string SaveAsMLTFile()
        {
            using (var dialog = new WinForms.SaveFileDialog())
            {
                dialog.Title = "名前をつけて保存";
                dialog.FileName = Path.GetFileName(this.FilePath);
                dialog.InitialDirectory = Path.GetDirectoryName(this.FilePath);
                dialog.Filter = "MLTファイル(*.mlt)|*.mlt";

                if (dialog.ShowDialog() != WinForms.DialogResult.OK)
                {
                    // キャンセル時はなにもしない
                    // もともとのファイルパスを返す
                    return this.FilePath;
                }

                // ダイアログで選択されたファイルの保存
                this.saveMLTFile(dialog.FileName);

                // 対象ファイルが変更されている場合があるため
                // ダイアログで更新されたファイルのパスを返す
                return dialog.FileName;
            }
        }

        /// <summary>
        /// 指定されたファイルパスに現在の状態を保存
        /// </summary>
        private void saveMLTFile(string filePath)
        {
            using (var writer = new StreamWriter(filePath, false /* append */, System.Text.Encoding.GetEncoding("Shift_JIS")))
            {
                foreach (var page in this.Pages)
                {
                    // スプリッターがある場合は書き込み
                    if (!string.IsNullOrEmpty(page.SplitterText))
                    {
                        writer.WriteLine(page.SplitterText);
                    }
                    // ページを書き込み
                    writer.WriteLine(page.RawText);
                }
            }
        }

        /// <summary>
        /// MLTファイルのオープンと読み込み
        /// 
        /// ファイル選択ダイアログを表示する
        /// </summary>
        /// <returns></returns>
        public MLTPage OpemMLTFileWithDialog()
        {
            using (var dialog = new WinForms.OpenFileDialog())
            {
                dialog.Title = "ファイルを開く";
                dialog.Filter = "すべてのファイル(*.*)|*.*"; // FIXME: MLT & Textファイルのみの指定にする

                if (dialog.ShowDialog() != WinForms.DialogResult.OK)
                {
                    // FIXME: キャンセルした場合は前のページを表示するべき
                    return null;
                }

                return this.OpenMLTFile(dialog.FileName);
            }

        }

        /// <summary>
        /// MLTファイルのオープンと読み込み
        ///
        /// すでにデータがある場合は初期化される
        /// </summary>
        /// <returns></returns>
        public MLTPage OpenMLTFile(string file_path)
        {
            // ファイルの存在チェック
            if (File.Exists(file_path) == false)
            {
                MessageBox.Show("指定されたファイルが見つかりませんでした");
                return null;
            }

            // 現在ページのPATHの更新
            this.FilePath = file_path;

            // タイトルの更新
            this.Name = Path.GetFileName(file_path);

            // ページリストの初期化
            this.Pages.Clear();
            this.current_page_num = 0;

            // AST用向けにタイトル抽出用正規表現オブジェクトを生成
            var regex = new System.Text.RegularExpressions.Regex(@"\[SPLIT\]\[(?<title>.*?)\]");

            // MLTからページリストの更新
            var index = 1;
            string splitter = null;
            string title = null;
            foreach (var tuple in this.ReadMLT(file_path))
            {
                var page = tuple.Item1 as string;

                // ページを追加
                Pages.Add(new MLTPage
                {
                    Index = index,
                    SplitterText = splitter,
                    Name = title,
                    Bytes = page.GetShift_JISByteCount(),
                    Lines = page.GetLineCount(),
                    IsCaption = page.IsCaption(),
                    RawText = page,
                    DecodeText = page.UnicodeEscapeDecode(),
                });

                // スプリッター, タイトルは次のページに適用
                splitter = tuple.Item2 as string;

                if (!string.IsNullOrEmpty(splitter))
                {
                    // ASTの場合のページタイトルを抽出
                    title = regex.Match(splitter).Groups["title"].Value;
                }

                // ページインデックスをインクリメント
                index++;
            }

            // 初回は先頭ページを開く
            return this.Pages.First();
        }

        /// <summary>
        /// MLTファイルの読み込み
        ///
        /// 区切り文字 `[SPLIT]` でのページ分割を行う
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns>(string page, string splitter)</returns>
        private IEnumerable<Tuple<string, string>> ReadMLT(string filepath)
        {
            using (var reader = new StreamReader(filepath, System.Text.Encoding.GetEncoding("shift_jis")))
            {
                string page = "";

                // [SPLIT] 単位でのページ分割を実施
                // 最終行に到達したらwhileループから抜ける
                while (reader.Peek() >= 0)
                {
                    var line = reader.ReadLine();

                    // 区切り文字の判定
                    // TODO: AST形式の場合でも問題ないか要確認
                    if (line.Contains("[SPLIT]") == true)
                    {
                        // 行が区切り文字の場合は、それまでの行をページとして返す
                        // 区切り文字の行はどのページにも含まない
                        // 最終行は改行しない
                        yield return Tuple.Create<string, string>(
                            page.TrimEnd(System.Environment.NewLine.ToCharArray()),
                            line);
                        // ページ生成用変数をリセット
                        page = "";
                    }
                    else
                    {
                        // 区切り文字以外なら行を追加
                        page += line + System.Environment.NewLine;
                    }
                }

                // 最終ページを返す
                // 最終行は改行しない
                yield return Tuple.Create<string, string>(
                    page.TrimEnd(System.Environment.NewLine.ToCharArray()),
                    null);
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MLTFile()
        {
        }
    }
}
