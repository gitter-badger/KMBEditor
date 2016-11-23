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
        public List<string> pages;

        public MLTClass()
        {
            this.pages = new List<string>();
        }

        /// <summary>
        /// MLTファイルのオープンと読み込み
        /// </summary>
        /// <returns></returns>
        public string OpenMLTFile()
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "ファイルを開く";
            dialog.Filter = "すべてのファイル(*.*)|*.*";
            
            if (dialog.ShowDialog() == false)
            {
                return "";
            }

            var filepath = dialog.FileName;
            if (File.Exists(filepath) == false)
            {
                MessageBox.Show("指定されたファイルが見つかりませんでした");
                return "";
            }

            var reader = new StreamReader(filepath, System.Text.Encoding.Default);

            // TODO: [SPLIT] での分割、ページ単位での保持
            pages.Add(reader.ReadToEnd());

            return pages.First(); // 初回は先頭ページを開く
        }
    }
}
