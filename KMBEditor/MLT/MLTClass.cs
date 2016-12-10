using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using WinForms = System.Windows.Forms;
using KMBEditor.StringExtentions;

namespace KMBEditor.MLT
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
        /// AST形式でのページ名(MLTの場合は定義なし)
        /// </summary>
        public string Name { get; set; } = "[No Name]";
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
        public string AA { get; set; } = "";
    }

    /// <summary>
    /// MLTファイル単位の状態管理クラス
    /// </summary>
    public class MLTFile
    {
        private string _file_path { get; set; }
        private string _raw_page { get; set; }
        private int current_page_num { get; set; }

        public ObservableCollection<MLTPage> Pages { get; private set; } = new ObservableCollection<MLTPage> { new MLTPage() };

        public MLTFile()
        {
        }

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
            this._file_path = file_path;

            // ページリストの初期化
            this.Pages.Clear();
            this.current_page_num = 0;

            // MLTからページリストの更新
            var index = 1;
            foreach (var page in this.ReadMLT(file_path))
            {
                Pages.Add(new MLTPage
                {
                    Index = index,
                    Name = "[No Name]",
                    Bytes = page.GetShift_JISByteCount(),
                    Lines = page.GetLineCount(),
                    IsCaption = page.IsCaption(),
                    AA = page
                });

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
        /// <returns></returns>
        private IEnumerable<string> ReadMLT(string filepath)
        {
            using (var reader = new StreamReader(filepath, System.Text.Encoding.Default))
            {
                var page = "";

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
                        // XXX: => ASTの場合は？情報を落とすと復元できなくなる？
                        yield return page.TrimEnd(System.Environment.NewLine.ToCharArray());
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
                yield return page.TrimEnd(System.Environment.NewLine.ToCharArray());
            }
        }
    }
}
