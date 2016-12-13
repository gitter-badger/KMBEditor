using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings;
using System.Collections.ObjectModel;

namespace KMBEditor.MyUserControl.Ruler.ViewModel
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
}
