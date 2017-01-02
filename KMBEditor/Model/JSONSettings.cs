using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace KMBEditor.Model
{
    /// <summary>
    /// JSONのシリアライズ、デシリアライズと保存/復元ためのクラス
    /// </summary>
    /// <typeparam name="T">対象のオブジェクトの型</typeparam>
    public class JSONSettings<T>
    {
        /// <summary>
        /// ファイルパス保持変数
        /// </summary>
        private string _filePath = "";
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="filePath">保存先のファイルパス</param>
        public JSONSettings(string filePath)
        {
            this._filePath = filePath;
        }

        /// <summary>
        /// 指定されているファイルが存在しているかの確認
        /// </summary>
        /// <returns>対象ファイルが存在していればtrue、しなければfalse</returns>
        public bool FileExists()
        {
            return File.Exists(this._filePath);
        }

        /// <summary>
        /// オブジェクトをJSONにシリアライズして保存
        /// </summary>
        /// <param name="obj">保存対象のオブジェクト</param>
        public void Save(T obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            using (var sw = new StreamWriter(this._filePath, false, Encoding.Unicode))
            {
                sw.Write(json);
            }
        }
        
        /// <summary>
        /// JSONファイルからのデシリアライズ
        /// </summary>
        /// <returns></returns>
        public T Load()
        {
            using (var sr = new StreamReader(this._filePath, Encoding.Unicode))
            {
                var data = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<T>(data);
            }
        }
    }
}
