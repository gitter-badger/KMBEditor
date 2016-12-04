using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace KMBEditor.MLT
{
    public class MLTPage
    {
        /// <summary>
        /// ページ番号
        /// </summary>
        public int Index { get; set; } = 0;
        /// <summary>
        /// AST形式でのページ名(MLTの場合は定義なし)
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// ページの総バイト数
        /// </summary>
        public int Bytes { get; set; } = 0;
        /// <summary>
        /// ページの総行数(最低1行は存在)
        /// </summary>
        public int Lines { get; set; } = 1;
        /// <summary>
        /// ページのAAテキストデータ
        /// </summary>
        public string AA { get; set; } = "";
    }

    /// <summary>
    /// MLT単位の状態を管理するクラス
    /// 
    /// MLTの基本単位:
    /// `^[SPLIT]$` にてAAを区切る
    /// 
    /// </summary>
    public class MLTFile
    {
        private string _file_path { get; set; }
        private string _raw_page { get; set; }
        private int current_page_num { get; set; }

        public ObservableCollection<MLTPage> Pages { get; private set; }

        public MLTFile()
        {
            this.Pages = new ObservableCollection<MLTPage>();
            this.Pages.Add(new MLTPage {});
        }

        /// <summary>
        /// 未加工のMLTファイルの取得
        /// </summary>
        /// <returns></returns>
        public string GetRawMLT()
        {
            // ファイルの存在チェック
            if (File.Exists(this._file_path) == false)
            {
                MessageBox.Show("指定されたファイルが見つかりませんでした");
                return "";
            }

            // FIXME: メモリ優先か速度優先か
            using (var reader = new StreamReader(this._file_path, System.Text.Encoding.Default))
            {
                return reader.ReadToEnd();
            }
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
            foreach (var page in this.ReadMLT(file_path))
            {
                Pages.Add(new MLTPage
                {
                    AA = page
                });
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
                var line = reader.ReadLine() + System.Environment.NewLine;
                page += line;

                while (reader.Peek() >= 0)
                {
                    line = reader.ReadLine() + System.Environment.NewLine;
                    // XXX: AST形式の場合でも問題ないか要確認
                    if (line.Contains("[SPLIT]") == true)
                    {
                        // 行が`[SPLIT]`(区切り文字)の場合は、ページを返す
                        yield return page;
                        // ページをリセット
                        page = "";
                    }
                    else
                    {
                        // 区切り文字は含まない
                        page += line;
                    }
                }

                yield return page;
            }
        }
    }
}
