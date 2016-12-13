using KMBEditor.MyUserControl.Ruler.ViewModel;
using System.Windows.Controls;

namespace KMBEditor.MyUserControl.Ruler.View
{
    /// <summary>
    /// Ruler.xaml の相互作用ロジック
    /// </summary>
    public partial class Ruler : UserControl
    {
        public Ruler()
        {
            InitializeComponent();

            this.RulerUserControl.DataContext = new RulerViewModel();
        }
    }
}
