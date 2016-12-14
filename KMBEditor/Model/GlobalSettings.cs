using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KMBEditor.Util.BindableBase;

namespace KMBEditor.Model
{
    /// <summary>
    /// システム共通のプロパティを保持。Singletonパターンで実装。
    /// </summary>
    public sealed class GlobalSettings : BindableBase
    {
        // 基本設定

        /// <summary>
        /// インスタンスの生成
        /// </summary>
        private static readonly GlobalSettings _instance = new GlobalSettings();

        /// <summary>
        /// インスタンスを返すプロパティ
        /// </summary>
        public static GlobalSettings Instance { get { return _instance;  } }

        /// <summary>
        /// コンストラクタ（外部でのインスタンスの生成を禁止）
        /// </summary>
        private GlobalSettings() {}


        // 以下、システム共通プロパティ設定

        /// <summary>
        /// オンラインドキュメントのURL
        /// </summary>
        public string OnlineDocumentURL { get; } = "https://tar-bin.gitbooks.io/kmbeditor-document/content/";

        /// <summary>
        /// GitLab Issue ページのURL
        /// </summary>
        public string GitHubIssueURL { get; } = "https://github.com/tar-bin/KMBEditor/issues";

        /// <summary>
        /// サポート掲示板のURL
        /// </summary>
        public string CurrentBoardURL { get; } = "";

        /// <summary>
        /// サポート用のTwitterのURL
        /// </summary>
        public string DevelopperTwtterURL { get; } = "https://twitter.com/tar_bin";
    }
}
