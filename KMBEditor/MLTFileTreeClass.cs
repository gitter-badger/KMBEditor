using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace KMBEditor
{
    public class MLTFileTreeNode
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public BitmapImage Icon { get; set; }
        public bool IsDirectory { get; set; }
        public List<MLTFileTreeNode> Children { get; set; }
    }

    class MLTFileTreeClass
    {
        /// <summary>
        /// サポートファイル形式
        /// 
        /// TODO: astのサポート
        /// </summary>
        private string _file_search_pattarn { get; set; } = "*.mlt";
        private BitmapImage _file_icon { get; set; } = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/Resources/File_48.png", UriKind.Absolute));
        private BitmapImage _folder_icon { get; set; } = new BitmapImage(new Uri(@"pack://siteoforigin:,,,/Resources/Folder_48.png", UriKind.Absolute)); 

        /// <summary>
        /// リソースディレクトリ配下のMLTファイルを再帰的に探索しツリー化を行う
        /// 
        /// TODO: 使用メモリ削減必須
        /// TODO: キャッシュの実装を検討する
        /// </summary>
        /// <param name="search_root_path"></param>
        /// <returns></returns>
        public List<MLTFileTreeNode> GetDirectoryNodes(string search_root_path)
        {
            var node = new List<MLTFileTreeNode>();

            // ディレクトリ配下のディレクトリの確認
            IEnumerable<string> directiry_paths =
                Directory.EnumerateDirectories(search_root_path);

            foreach (string dir_path in directiry_paths)
            {
                // Directory.EnumerateDirectoriesはルートのパスも含む
                // 自分自身が指定された場合は再参照しないでスキップ
                if (dir_path == search_root_path)
                {
                    continue;
                }

                // 子要素を参照
                node.Add(
                    new MLTFileTreeNode
                    {
                        Name = dir_path.Substring(search_root_path.Length + 1), // 親のパス長 + '\' の位置を指定
                        Path = dir_path,
                        Icon = _folder_icon,
                        IsDirectory = true,
                        Children = this.GetDirectoryNodes(dir_path)
                    });
            }

            // ディレクトリ配下のMLTファイルの確認
            IEnumerable<string> file_paths =
                Directory.EnumerateFiles(search_root_path, _file_search_pattarn);

            foreach (string file_path in file_paths)
            {
                node.Add(
                    new MLTFileTreeNode
                    {
                        Name = file_path.Substring(search_root_path.Length + 1), // 親のパス長 + '\' の位置を指定
                        Path = file_path,
                        Icon = _file_icon,
                        IsDirectory = false,
                    });
            }

            // 子要素がない場合は空のリストが返る
            return node;
        }

        /// <summary>
        /// ルートディレクトリから再帰的にMLTファイルを探索
        /// 
        /// 読み込み対象によってはかなり重くなるので注意
        /// </summary>
        /// <param name="root_path"></param>
        /// <returns></returns>
        public List<MLTFileTreeNode> SearchMLTFile(string root_path)
        {
            return this.GetDirectoryNodes(root_path);
        }
    }
}
