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
using System.Diagnostics;

namespace emDomoSim
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    RoomSimulator room_ = new RoomSimulator();

    AutoResetEvent cancel_ = new AutoResetEvent(false);

    Thread simulation_;

    public MainWindow()
    {
      InitializeComponent();

      fanControlProgram.Text = room_.FanControlName();
    }

    private void CancelSimulation()
    {
      if (simulation_ != null)
      {
        Trace.WriteLine("Cancel");
        cancel_.Set(); // TODO: avoid race
        simulation_.Join();
        cancel_.Reset();
        Trace.WriteLine("Canceled");
      }
    }

    private void Run_Click(object sender, RoutedEventArgs e)
    {
      SimulateDay_Click(sender, e); //
    }
    protected override void OnClosed(EventArgs e)
    {
      CancelSimulation();
      base.OnClosed(e);
    }
    private void Exit_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void DoSimulateDay()
    {
      time.IsEnabled = false;
      CancelSimulation();
      // The Work to perform on another thread
      ThreadStart start = delegate()
      {
        Trace.WriteLine("Started");
        // move time from left to right
        for (double f = 0; f < 24; f += 0.05)
        {
          Trace.WriteLine(String.Format("sim {0}", f));
          Dispatcher.BeginInvoke((Action)delegate() { time.Value = f; });
          if (cancel_.WaitOne(20))
          {
            Trace.WriteLine("Cancel received");
            break;
          }
        }
        Trace.WriteLine("Ending");
        simulation_ = null;
        Trace.WriteLine("Ended");

        Dispatcher.BeginInvoke((Action)delegate() { time.IsEnabled = true; });

      };

      // Create the thread and kick it started!
      simulation_ = new Thread(start);
      simulation_.Start();
    }

    private void SimulateDays(int days)
    {
      float duration = 24.0f * days;

      //DoSimulateDay();
      // Instantiate the dialog box
      var dlg = new SimulWindow();

      // Configure the dialog box
      dlg.Owner = this;

      int dayInYear = DayInYear();

      int warmUpDays = 7; // TODO: find shortest warm-up necessary, longer warm-up should make almost no difference

      dayInYear -= warmUpDays;
      if (dayInYear < 0) dayInYear += 366;

      float deltaT = 0.05f;
      var room = new RoomSimulator();

      var fanControlName = fanControlProgram.Text;
      room.SelectFanControl(fanControlProgram.Text);
      
      room.SetTime(dayInYear, 0);
      for (float t = 0; t < 24.0f * warmUpDays; t += deltaT)
      {
        room.AdvanceTime(deltaT);
      }

      int nSamples = 0;
      float sumTemp = 0;
      float minTemp = float.MaxValue;
      float maxTemp = float.MinValue;
      float sumFanOn = 0;


      for (float t = 0; t < duration; t += deltaT)
      {
        RoomSimulator.State roomState = room.AdvanceTime(deltaT);
        nSamples++;
        sumTemp += roomState.roomTemperature_;
        maxTemp = Math.Max(roomState.roomTemperature_, maxTemp);
        minTemp = Math.Min(roomState.roomTemperature_, minTemp);
        sumFanOn += (room.FanStatus() ? 1 : 0) * deltaT;
      }

      dlg.avgRoomTemp.Text = String.Format("{0:0.0} °C", sumTemp / nSamples);
      dlg.tempOsc.Text = String.Format("{0:0.0} °C", maxTemp - minTemp);

      dlg.fanOnTime.Text = String.Format("{0:0} %", sumFanOn * 100 / duration);

      //dlg.DocumentMargin = this.documentTextBox.Margin;

      // Open the dialog box modally 
      dlg.ShowDialog();
    }

    private void SimulateDay_Click(object sender, RoutedEventArgs e)
    {
      SimulateDays(1);
    }

    private void SimulateMonth_Click(object sender, RoutedEventArgs e)
    {
      SimulateDays(30);
    }

    private void SimulateYear_Click(object sender, RoutedEventArgs e)
    {
      SimulateDays(366);
    }

    /**
    fimeFrom / timeTo - time in day in hours
    */
    private void SimulateTo(int dayInYear, float time)
    {
      float deltaT = room_.SetTime(dayInYear, time);
      RoomSimulator.State roomState = room_.Simulate(deltaT);
      fanStatus.Text = room_.FanStatus() ? "On" : "Off";
      TimeSpan ts = TimeSpan.FromHours(time);
      curTime.Text = ts.ToString(@"hh\:mm");
      curTemp.Text = roomState.curTemp.ToString("f1");
      minTemp.Text = roomState.minTemp.ToString("f1");
      maxTemp.Text = roomState.maxTemp.ToString("f1");
      roomTemp.Text = roomState.roomTemperature_.ToString("f1");
    }

    private int DayInYear()
    {
      try
      {
        DateTime date = new DateTime(2000, Convert.ToInt16(month.Text), Convert.ToInt16(dayInMonth.Text));
        return date.DayOfYear;
      }
      catch
      {
      }
      return 180;
    }
    private void Time_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      float time = (float)e.NewValue;
      int dayInYear = DayInYear();
      SimulateTo(dayInYear, time);
    }

    private void FanControlProgram_DropDownClosed(Object sender, EventArgs e)
    {
      //string text = (e.AddedItems[0] as ComboBoxItem).Content as string; 
      room_.SelectFanControl(fanControlProgram.Text);
    }

  }

  public class MyFanControls : List<String>
  {
    public MyFanControls()
    {
      string @namespace = "emDomoSim.FanControlPrograms";
      var fanControls = (from lAssembly in AppDomain.CurrentDomain.GetAssemblies()
                         from lType in lAssembly.GetTypes()
                         where lType.Namespace == @namespace
                         where typeof(FanControl).IsAssignableFrom(lType)
                         select lType);
      foreach (var fc in fanControls)
      {
        this.Add(fc.FullName.Split('.').Last());
      }
    }
  }
}
