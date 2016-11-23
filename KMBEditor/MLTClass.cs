using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
        private List<string> pages;
        private int current_page_num;

        public MLTClass()
        {
            this.pages = new List<string>();
            this.pages.Add("");
            this.current_page_num = 0;
        }

        public string GetCurrentPage()
        {
            return this.pages[this.current_page_num];
        }

        public string GetPrevPage()
        {
            if (this.current_page_num > 0)
            {
                this.current_page_num -= 1;
            }
            return this.pages[this.current_page_num];
        }

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
        /// すでにデータがある場合は初期化される
        /// </summary>
        /// <returns></returns>
        public string OpenMLTFile()
        {
            // ダイアログからファイルパスの取得
            var dialog = new OpenFileDialog();
            dialog.Title = "ファイルを開く";
            dialog.Filter = "すべてのファイル(*.*)|*.*"; // FIXME: MLT & Textファイルのみの指定にする
            
            if (dialog.ShowDialog() == false)
            {
                return "";
            }

            // ファイルの存在チェック
            var filepath = dialog.FileName;
            if (File.Exists(filepath) == false)
            {
                MessageBox.Show("指定されたファイルが見つかりませんでした");
                return "";
            }

            // ページリストの初期化
            this.pages.Clear();
            this.current_page_num = 0;

            // MLTからページリストの更新
            foreach (var page in this.ReadMLT(filepath))
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
