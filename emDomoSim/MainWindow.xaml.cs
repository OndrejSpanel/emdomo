using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace emDomoSim
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

      // The Work to perform on another thread
      ThreadStart start = delegate()
      {
        // move time from left to right
        for (double f = 0; f < 24; f += 0.05)
        {
          Dispatcher.Invoke(delegate()
          {
            time.Value = f;
          });
          System.Threading.Thread.Sleep(20);
        }
      };

      // Create the thread and kick it started!
      new Thread(start).Start();
    }
    private void Exit_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void Simulate_Click(object sender, RoutedEventArgs e)
    {
      Run_Click(sender, e); //
    }

    private void Time_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      double time = e.NewValue;
      double minTempV = Convert.ToDouble(minTemp.Text);
      double maxTempV = Convert.ToDouble(maxTemp.Text);
      double minTimeV = 6;
      double maxTimeV = 15;
      try
      {
        minTimeV = DateTime.ParseExact(minTempTime.Text, @"H:mm", null).TimeOfDay.TotalHours;
        maxTimeV = DateTime.ParseExact(maxTempTime.Text, @"H:mm", null).TimeOfDay.TotalHours;
      }
      catch
      {
      
      }
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

      TimeSpan ts= TimeSpan.FromHours(time);
      curTime.Text = ts.ToString(@"hh\:mm");
      curTemp.Text = curTempV.ToString("0.0");
    }

    struct MonthlyTemp
    {
      public double min, avg, max;
      public MonthlyTemp(double min, double avg, double max) { this.min = min; this.avg = avg; this.max = max; }

      public static MonthlyTemp operator * (MonthlyTemp t, double a) {return new MonthlyTemp(t.min*a,t.avg*a,t.max*a);}
      public static MonthlyTemp operator + (MonthlyTemp t, MonthlyTemp a) { return new MonthlyTemp(t.min + a.min, t.avg + a.avg, t.max + a.max); }
    };

    struct SunTimes
    {
      public double rise,set;
      public SunTimes(double rise, double set) { this.rise = rise; this.set = set; }
    }

    static SunTimes ComputeSunTimes(double dayInYear)
    {
      // http://stackoverflow.com/a/5095972/16673
      double to_r = Math.PI / 180.0;
      double latitude = 50.0*to_r;


      //double et = -7.633 * Math.Sin(dayInYear * (2 * Math.PI)/365.24) + 9.65 * Math.Sin((dayInYear - 78) * 180/92 * to_r);
      double a_sin = Math.Sin(23.433 * to_r) * Math.Sin((2 * Math.PI/366) * (dayInYear - 81));
      double declination = Math.Asin(a_sin);

      double cos_omega = Math.Sin(-0.83 * to_r) - Math.Tan(latitude) * Math.Tan(declination);
      double semi_diurnal_arc = Math.Acos(cos_omega);
      double rise = 12 - semi_diurnal_arc*12/Math.PI;
      double set  = 12 + semi_diurnal_arc * 12 / Math.PI;

      return new SunTimes(rise, set);
    }

    private void SetWeather_Click(object sender, RoutedEventArgs e)
    {
      MonthlyTemp[] monthly = new MonthlyTemp[]
      {
        new MonthlyTemp (-5, -2, 0 ),
        new MonthlyTemp (-3.5, -1, 2.55 ),
        new MonthlyTemp (-1, 3, 7),
        new MonthlyTemp (3, 8, 13),
        new MonthlyTemp (7.5, 13, 18 ),
        new MonthlyTemp (11, 16, 22 ),
        new MonthlyTemp (12, 17.5, 23 ),
        new MonthlyTemp (12, 17, 23 ),
        new MonthlyTemp (9, 13.5, 19 ),
        new MonthlyTemp (4, 8, 12.5 ),
        new MonthlyTemp (0, 2.5, 6 ),
        new MonthlyTemp (-3.5, 0, 2.5 ),
      };
      double day = 180;
      try
      {
        DateTime date = new DateTime(2000, Convert.ToInt16(month.Text), Convert.ToInt16(dayInMonth.Text));
        day = date.DayOfYear;
      }
      catch
      {
      }
      double monthVal = day * 12 / 365 - 0.5;
      int prev = (int)Math.Floor(monthVal);
      double nextFactor = monthVal-prev;
      int next = prev + 1;
      if (prev < 0) prev += monthly.Length;
      if (next >= monthly.Length) next -= monthly.Length;
      MonthlyTemp temp = monthly[prev] * (1 - nextFactor) + monthly[next] * nextFactor;
      minTemp.Text = temp.min.ToString("f1");
      maxTemp.Text = temp.max.ToString("f1");
      SunTimes sunTimes = ComputeSunTimes(day);
      double minTime = sunTimes.rise;
      double maxTime = (12+sunTimes.set) *0.5;

      minTempTime.Text = TimeSpan.FromHours(minTime).ToString(@"hh\:mm");
      maxTempTime.Text = TimeSpan.FromHours(maxTime).ToString(@"hh\:mm");
    }
  }
}
