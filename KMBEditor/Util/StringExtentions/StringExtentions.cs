using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace KMBEditor.Util.StringExtentions
{
    /// <summary>
    /// stringの拡張メソッドを定義
    /// </summary>
    public static class StringExtentions
    {
        /// <summary>
        /// Shift_JIS(cp932)での総バイト数を取得
        /// </summary>
        /// <param name="str"></param>
        /// <returns>総バイト数</returns>
        public static int GetShift_JISByteCount(this string str)
        {
            return Encoding.GetEncoding(932).GetByteCount(str);
        }

        /// <summary>
        /// 文字列の総行数を取得(改行コードをカウント)
        /// </summary>
        /// <param name="str"></param>
        /// <returns>総行数</returns>
        public static int GetLineCount(this string str)
        {
            int n = 0;
            foreach (var c in str)
            {
                // 改行が'\r\n'でも'\n'は含まれているので行数カウントには問題ない
                if (c == '\n') n++;
            }
            return n + 1;
        }

        /// <summary>
        /// IsCaptionで使用する正規表現の事前コンパイル
        /// </summary>
        private readonly static Regex _caption_regx = new Regex(@"^【.*】$", RegexOptions.Compiled);

        /// <summary>
        /// 見出しページかの判定
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsCaption(this string str)
        {
            return _caption_regx.IsMatch(str);
        }

        /// <summary>
        /// ユニコードのエスケープ文字のデコード
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UnicodeEscapeDecode(this string str)
        {
            // &#123 のようなエスケープされている文字をUnicodeに変換する
            return HttpUtility.HtmlDecode(str);
        }

        /// <summary>
        /// 文字列を行ごとに読み出し
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static IEnumerable<string> ReadLine(this string str)
        {
            if (str != null)
            {
                var lines = str.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    yield return line;
                }
            }
        }
    }
}
