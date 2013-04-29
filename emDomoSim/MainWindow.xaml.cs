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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfTest
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    private void Run_Click(object sender, RoutedEventArgs e)
    {

    }
    private void Exit_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void Simulate_Click(object sender, RoutedEventArgs e)
    {
      Run_Click(sender, e);
    }

    private void Time_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      double time = e.NewValue;
      double minTempV = Convert.ToSingle(minTemp.Text);
      double maxTempV = Convert.ToSingle(maxTemp.Text);
      double minTimeV = DateTime.ParseExact(minTempTime.Text, "H:mm", null).TimeOfDay.TotalHours;
      double maxTimeV = DateTime.ParseExact(maxTempTime.Text, "H:mm", null).TimeOfDay.TotalHours;
      // assume sinusoid, min in minTime, max in maxTime
      double tempFactor = 0;
      if (time < minTimeV)
      {
        // assume sinus between maxTime-24h and minTime
        double timeFactor = (time - (maxTimeV-24)) / (minTimeV - (maxTimeV-24));
        tempFactor = Math.Sin((timeFactor-0.5) * Math.PI)*-0.5+0.5;
      }
      else if (time >= minTimeV && time < maxTimeV)
      {
        // assume sinus between minTime and maxTime
        double timeFactor = (time - minTimeV) / (maxTimeV - minTimeV);
        tempFactor = Math.Sin((timeFactor - 0.5) * Math.PI) * 0.5 + 0.5;
      }
      else
      {
        // assume sinus between maxTime and minTime+24
        double timeFactor = (time - maxTimeV) / (minTimeV+24 - maxTimeV);
        tempFactor = Math.Sin((timeFactor - 0.5) * Math.PI) * -0.5 + 0.5;
      }

      double curTempV = minTempV + (maxTempV - minTempV) * tempFactor;

      TimeSpan ts= System.TimeSpan.FromHours(time);
      curTime.Text = ts.ToString(@"hh\:mm");
      curTemp.Text = curTempV.ToString("0.0");
    }
  }
}
