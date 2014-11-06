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
using System.Globalization;
using System.Windows.Markup;

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

    enum OscState {Up,Down,None};

    private SimulationResults.FanControlResults SimulateDays(FanControl fc, int days)
    {
      float duration = 24.0f * days;

      //DoSimulateDay();

      int dayInYear = DayInYearInput();

      int warmUpDays = 7; // TODO: find shortest warm-up necessary, longer warm-up should make almost no difference

      var room = new RoomSimulator();

      room.SelectFanControl(fc);
      room.SetTimeWithWarmUp(dayInYear, 0, warmUpDays);

      float deltaT = 0.05f;

      int nSamples = 0;
      float sumTemp = 0;
      float sumFanOn = 0;
      float sumTempOsc = 0;
      float lastTemp = room.GetRoomTemperature();

      float lastMin = lastTemp;
      float lastMax = lastTemp;
      OscState oscState = OscState.None;
      float sumOsc = 0;
      int nOsc = 0;


      for (float t = 0; t < duration; t += deltaT)
      {
        RoomSimulator.State roomState = room.AdvanceTime(deltaT);
        nSamples++;
        float temp = roomState.roomTemperature_;
        sumTemp += temp;

        if (temp > lastTemp)
        {
          if (oscState == OscState.Down)
          {
            lastMin = lastTemp;
          }
          oscState = OscState.Up;
        }
        else
        {
          if (oscState == OscState.Up)
          {
            lastMax = lastTemp;
            sumOsc += lastMax - lastMin;
            nOsc++;
          }
          oscState = OscState.Down;

        }
        sumTempOsc += Math.Abs(temp - lastTemp)/deltaT;
        lastTemp = temp;
        sumFanOn += (room.FanStatus() ? 1 : 0) * deltaT;
      }
      return new SimulationResults.FanControlResults(fc.Name(), sumTemp / nSamples, sumTempOsc / nSamples, sumFanOn * 100 / duration);
      //return new SimulationResults.FanControlResults(fc.Name(), sumTemp / nSamples, sumOsc / nOsc, sumFanOn * 100 / duration);
    }

    private void SimulateDays(int days)
    {
      // Instantiate the dialog box
      var dlg = new SimulWindow();

      // Configure the dialog box
      dlg.Owner = this;

      var results = new SimulationResults();

      var fcList = new FanControlsList();

      foreach (var fc in fcList.Types())
      {
        var result = SimulateDays((FanControl)Activator.CreateInstance(fc), days);
        results.Results.Add(result);
      }


      dlg.resultsGrid.ItemsSource = results.Results;


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
      //float deltaT =
      var roomState = room_.SetTimeWithWarmUp(dayInYear, time, 7);
      //RoomSimulator.State roomState = room_.Simulate(deltaT);
      fanStatus.Text = room_.FanStatus() ? "On" : "Off";
      TimeSpan ts = TimeSpan.FromHours(time);
      curTime.Text = ts.ToString(@"hh\:mm");
      curTemp.Text = roomState.curTemp.ToString("f1");
      minTemp.Text = roomState.minTemp.ToString("f1");
      maxTemp.Text = roomState.maxTemp.ToString("f1");
      roomTemp.Text = roomState.roomTemperature_.ToString("f1");
    }

    private void RefreshRoomState()
    {
      var roomState = room_.GetState();
      fanStatus.Text = room_.FanStatus() ? "On" : "Off";
      curTemp.Text = roomState.curTemp.ToString("f1");
      minTemp.Text = roomState.minTemp.ToString("f1");
      maxTemp.Text = roomState.maxTemp.ToString("f1");
      roomTemp.Text = roomState.roomTemperature_.ToString("f1");

    }
    private int DayInYearInput()
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
      int dayInYear = DayInYearInput();
      SimulateTo(dayInYear, time);
    }

    private void FanControlProgram_DropDownClosed(Object sender, EventArgs e)
    {
      //string text = (e.AddedItems[0] as ComboBoxItem).Content as string; 
      room_.SelectFanControlRecalc(fanControlProgram.Text);
      RefreshRoomState();
    }

  }

  public class FanControlsList
  {
    public IEnumerable<Type> Types()
    {
      string @namespace = "emDomoSim.FanControlPrograms";
      return (from lAssembly in AppDomain.CurrentDomain.GetAssemblies()
                         from lType in lAssembly.GetTypes()
                         where lType.Namespace == @namespace
                         where typeof(FanControl).IsAssignableFrom(lType)
                         select lType);

    }
    public List<String> Names()
    {
      var ret = new List<String>();
      var fanControls = Types();
      foreach (var fc in fanControls)
      {
        ret.Add(fc.FullName.Split('.').Last());
      }
      return ret;
    }
  }

  public class FanControlsListToListString : MarkupExtension, IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      var src = (FanControlsList)value;
      return src.Names();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return DependencyProperty.UnsetValue;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      return this;
    }
  }
}
