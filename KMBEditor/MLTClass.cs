using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using WinForms = System.Windows.Forms;

namespace KMBEditor
{
    /// <summary>
    /// MLT単位の状態を管理するクラス
    /// 
    /// MLTの基本単位:
    /// `^[SPLIT]$` にてAAを区切る
    /// 
    /// </summary>
    class MLTClass
    {
        private string _file_path { get; set; }
        private string _raw_page { get; set; }
        private List<string> pages { get; set; }
        private int current_page_num { get; set; }

        public MLTClass()
        {
            this.pages = new List<string>();
            this.pages.Add("");
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
        public string GetCurrentPage()
        {
            return this.pages[this.current_page_num];
        }

        /// <summary>
        /// 前ページを開く
        /// </summary>
        /// <returns></returns>
        public string GetPrevPage()
        {
            if (this.current_page_num > 0)
            {
                this.current_page_num -= 1;
            }
            return this.pages[this.current_page_num];
        }

        /// <summary>
        /// 次ページを開く
        /// </summary>
        /// <returns></returns>
        public string GetNextPage()
        {
            if (this.current_page_num < (this.pages.Count -1))
            {
                this.current_page_num += 1;
            }
            return this.pages[this.current_page_num];
        }

        /// <summary>
        /// MLTファイルのオープンと読み込み
        /// 
        /// ファイル選択ダイアログを表示する
        /// </summary>
        /// <returns></returns>
        public string OpemMLTFileWithDialog()
        {
            using (var dialog = new WinForms.OpenFileDialog())
            {
                dialog.Title = "ファイルを開く";
                dialog.Filter = "すべてのファイル(*.*)|*.*"; // FIXME: MLT & Textファイルのみの指定にする

                if (dialog.ShowDialog() != WinForms.DialogResult.OK)
                {
                    return "";
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
        public string OpenMLTFile(string file_path)
        {
            // ファイルの存在チェック
            if (File.Exists(file_path) == false)
            {
                MessageBox.Show("指定されたファイルが見つかりませんでした");
                return "";
            }

            // 現在ページのPATHの更新
            this._file_path = file_path;

            // ページリストの初期化
            this.pages.Clear();
            this.current_page_num = 0;

            // MLTからページリストの更新
            foreach (var page in this.ReadMLT(file_path))
            {
                pages.Add(page);
            }

            return this.pages.First(); // 初回は先頭ページを開く
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
