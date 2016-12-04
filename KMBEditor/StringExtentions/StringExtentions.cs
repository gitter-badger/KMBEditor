using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KMBEditor.StringExtentions
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
                if (c == '\n') n++;
            }
            return n + 1;
        }
    }
}
