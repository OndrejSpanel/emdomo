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

namespace emDomoSim
{
  /// <summary>
  /// Interaction logic for SimulWindow.xaml
  /// </summary>
  public partial class SimulWindow : Window
  {
    public SimulWindow()
    {
      InitializeComponent();
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      /*
      avgRoomTemp.Text = String.Format("{0:0.0} °C", sumTemp / nSamples);
      tempOsc.Text = String.Format("{0:0.0} °C", maxTemp - minTemp);
      
      fanOnTime.Text = String.Format("{0:0} %", sumFanOn * 100 / duration);
      */

    }
  }
}
