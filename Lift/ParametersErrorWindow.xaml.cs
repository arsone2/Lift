using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Lift
{
    /// <summary>
    /// Interaction logic for ParametersErrorWindow.xaml
    /// </summary>
    public partial class ParametersErrorWindow : Window
    {
        public ParametersErrorWindow()
        {
            InitializeComponent();


            commandTextBlock.Text = "Command line: ";

            // get command line arguments array
            string[] argumentsArray = Environment.GetCommandLineArgs();

            for (int i = 0; i < argumentsArray.Length; i++)
			{
                commandTextBlock.Text += " " + argumentsArray[i];
			}

           

            describeTextBlock.Text = @"-f <how many floors> - number of floors in the building (5 - 20). Type: int. Default value: 10.
-h <floor height> - floor height (meters). Type: double. Default value: 3.
-s <lift speed> - lift speed (meters per second). Type: double. Default value: 2.
-o <openning time> - lift door openning time (seconds). Type: double. Default value: 1.
-c <closing time> - lift door closing time (seconds). Type: double. Default value: 1.
-w <waiting time when door opened> - waiting time lift door still opened (seconds). Type: double. Default value: 3.";


        }
    }
}
