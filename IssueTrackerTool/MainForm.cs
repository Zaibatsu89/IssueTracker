using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IssueTrackerTool
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            // referentie: https://github.com/microsoft/winappCli/blob/main/src/winapp-GUI/winapp-GUI/MainWindow.xaml.cs#L62
            SetupWindow();
        }
        // referentie: https://github.com/microsoft/winappCli/blob/main/src/winapp-GUI/winapp-GUI/MainWindow.xaml.cs#L81
        private void SetupWindow()
        {
            Size = new Size(1000, 1000);
        }
    }
}
