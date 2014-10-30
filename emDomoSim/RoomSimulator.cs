using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emDomoSim
{
  public class RoomSimulator : FanControlInput
  {
    int dayInYear_;
    float timeOfDay_; //  in hours

    FanControl fan_ = new FanControlPrograms.FanControlThermostat();
    WeatherSim weatherSim_ = new WeatherSim();
    State state_ = new State();

    public RoomSimulator()
    {
    }

    public string FanControlName()
    {
      return fan_.GetType().FullName.Split('.').Last();
    }
    public void SelectFanControl(FanControl fc)
    {
      fan_ = fc;
    }
    public void SelectFanControl(string fcName)
    {
      if (String.IsNullOrEmpty(fcName)) return;
      var fullFcName = "emDomoSim.FanControlPrograms." + fcName;
      var fanControl = Activator.CreateInstance(Type.GetType(fullFcName));
      if (fanControl is FanControl)
      {
        fan_ = (FanControl)fanControl;
      }
    }
    public class State : WeatherSim.Weather
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

    public State AdvanceTime(float deltaT)
    {
      const float oneDay = 24.0f;
      var ret = Simulate(deltaT);
      timeOfDay_ += deltaT;
      while (timeOfDay_ > oneDay)
      {
        timeOfDay_ -= oneDay;
        dayInYear_++;
        if (dayInYear_ >= 366) dayInYear_ = 0;
      }
      return ret;
    }

    public State Simulate(float deltaT)
    {
      WeatherSim.Weather weather = weatherSim_.Simulate(dayInYear_, timeOfDay_);
      State result = new State(state_);
      result.SetWeather(weather);
      // simulate how the room responds
      float houseTemperature = 20;
      float houseWeight = 0.9f;
      float ambientTemperature = (weather.curTemp * (1 - houseWeight) + houseTemperature * houseWeight);
      result.roomTemperature_ += (ambientTemperature - result.roomTemperature_) * 0.007f * deltaT;
      state_ = result;
      fan_.Simulate(deltaT, this);
      if (fan_.FanStatus())
      {
        result.roomTemperature_ += (weather.curTemp - result.roomTemperature_) * 0.02f * deltaT;
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

}
