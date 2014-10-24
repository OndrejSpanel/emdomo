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
    public class RoomSimulator : FanControl.Input
    {
      int dayInYear_;
      float timeOfDay_; //  in hours

      FanControl fan_ = new FanControl();
      WeatherSim weatherSim_ = new WeatherSim();
      State state_ = new State();

      public RoomSimulator()
      {
      }

      public class State: WeatherSim.Weather
      {
        public float roomTemperature_;

        public State()
        {
          roomTemperature_ = 15;
        }
        public State(State w)
          : base(w)
        {
          roomTemperature_ = w.roomTemperature_;
        }

      }

      public float SetTime(int dayInYear, float timeOfDay)
      {
        const float maxDelta = 0.2f;
        float delta = timeOfDay - timeOfDay_;
        if (dayInYear_ != dayInYear) delta = 0;
        else if (delta < 0) delta = 0;
        else if (delta > maxDelta) delta = maxDelta;
        dayInYear_ = dayInYear;
        timeOfDay_ = timeOfDay;
        return delta;
      }
      public State Simulate(float deltaT)
      {
        WeatherSim.Weather weather = weatherSim_.Simulate(dayInYear_, timeOfDay_);
        State result = new State(state_);
        result.SetWeather(weather);
        // simulate how the room responds
        float houseTemperature = 20;
        float houseWeight = 0.7f;
        float ambientTemperature = (weather.curTemp * (1 - houseWeight) + houseTemperature * houseWeight);
        result.roomTemperature_ += (ambientTemperature - result.roomTemperature_) * 0.007f * deltaT;
        state_ = result;
        fan_.Simulate(deltaT, this);
        if (fan_.FanStatus())
        {
          result.roomTemperature_ += (weather.curTemp - result.roomTemperature_)*0.02f*deltaT;
        }
        return result;
      }

      public float GetOutsideTemperature() { return state_.curTemp; }
      public float GetRoomTemperature() { return state_.roomTemperature_; }

      internal bool FanStatus()
      {
        return fan_.FanStatus();
      }
    };

    RoomSimulator room_ = new RoomSimulator();

    AutoResetEvent cancel_ = new AutoResetEvent(false);

    Thread simulation_;

    public MainWindow()
    {
      InitializeComponent();
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
          Dispatcher.BeginInvoke((Action)delegate() {time.Value = f;});
          if (cancel_.WaitOne(20))
          {
            Trace.WriteLine("Cancel received");
            break;
          }
        }
        Trace.WriteLine("Ending");
        simulation_ = null;
        Trace.WriteLine("Ended");

        Dispatcher.BeginInvoke((Action)delegate() { time.IsEnabled = true;});

      };

      // Create the thread and kick it started!
      simulation_ = new Thread(start);
      simulation_.Start();
    }

    private void SimulateDay_Click(object sender, RoutedEventArgs e)
    {
      //DoSimulateDay();
      // Instantiate the dialog box
      var dlg = new SimulWindow();

      // Configure the dialog box
      dlg.Owner = this;
      dlg.avgRoomTemp.Text = "15.7 °C";
      dlg.tempOsc.Text = "0.8 °C";

      dlg.fanOnTime.Text = "63 %";

      //dlg.DocumentMargin = this.documentTextBox.Margin;

      // Open the dialog box modally 
      dlg.ShowDialog();
    }

    private void SimulateMonth_Click(object sender, RoutedEventArgs e)
    {
      DoSimulateDay();
    }

    private void SimulateYear_Click(object sender, RoutedEventArgs e)
    {
      DoSimulateDay();
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

    private void Time_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      float time = (float)e.NewValue;
      int dayInYear = 180;
      try
      {
        DateTime date = new DateTime(2000, Convert.ToInt16(month.Text), Convert.ToInt16(dayInMonth.Text));
        dayInYear = date.DayOfYear;
      }
      catch
      {
      }
      SimulateTo(dayInYear, time);
    }

  }
}
