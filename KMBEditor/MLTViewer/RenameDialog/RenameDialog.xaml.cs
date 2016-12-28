using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Reactive.Bindings;

namespace KMBEditor.MLTViewer.RenameDialog
{
    /// <summary>
    /// RenameDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class RenameDialogWindow : Window
    {
        public ReactiveProperty<string> InputText { get; private set; } = new ReactiveProperty<string>("default");
        public ReactiveCommand OkButtonClickCommand { get; private set; } = new ReactiveCommand();

        public string ResponseText
        {
            get { return this.InputText.Value; }
            set { this.InputText.Value = value; }
        }

        public RenameDialogWindow(string headerText)
        {
            InitializeComponent();

            this.InputText.Value = headerText;
            this.OkButtonClickCommand.Subscribe(_ => this.DialogResult = true);
            this.DataContext = this;
        }
    }
}
