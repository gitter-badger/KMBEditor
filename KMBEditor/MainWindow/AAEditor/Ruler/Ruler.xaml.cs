using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace KMBEditor.MainWindow.AAEditor.Ruler
{
    public class RulerNumber
    {
        public int Number { get; set; }
    }

    class RulerViewModel
    {
        public List<RulerNumber> RulerList { get; private set; } = new List<RulerNumber>();

        public RulerViewModel()
        {
            foreach (var i in Enumerable.Range(0,12).Select(x => x * 100))
            {
                this.RulerList.Add(new RulerNumber { Number = i });
            }
        }
    }

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
