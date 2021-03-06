﻿using System;
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
    public void SelectFanControlRecalc(string fcName)
    {
      SelectFanControl(fcName);
      SetTimeWithWarmUp(dayInYear_, timeOfDay_, 7, true);
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
      if (dayInYear_ != dayInYear) delta += (dayInYear - dayInYear_)*24;
      if (delta < 0) delta = 0;
      if (delta > maxDelta) delta = maxDelta;
      dayInYear_ = dayInYear;
      timeOfDay_ = timeOfDay;
      return delta;
    }

    public State SetTimeWithWarmUp(int dayInYear, float timeOfDay, int warmUpDays, bool forceWarmUp=false)
    {
      if (!forceWarmUp)
      {
        float dt = SetTime(dayInYear, timeOfDay);
        if (dt > 0 && dt < 0.2f)
        {
          return Simulate(dt);
        }

      }

      int dayInYearWarmUp = dayInYear - warmUpDays;
      if (dayInYearWarmUp < 0) dayInYearWarmUp += 366;

      SetTime(dayInYearWarmUp, timeOfDay);

      float deltaT = 0.05f;
      for (float t = 0; t < 24.0f * warmUpDays; t += deltaT)
      {
        AdvanceTime(deltaT);
      }

      // warm up may cause some rounding errors, eliminate them by setting the time again
      SetTime(dayInYear, timeOfDay);
      return state_;
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
      fan_.Simulate(deltaT, this);
      float ambientTemperature = (weather.curTemp * (1 - houseWeight) + houseTemperature * houseWeight);
      result.roomTemperature_ += (ambientTemperature - result.roomTemperature_) * 0.007f * deltaT;
      if (fan_.FanStatus())
      {
        result.roomTemperature_ += (weather.curTemp - result.roomTemperature_) * 0.02f * deltaT;
      }
      state_ = result;
      return result;
    }

    public State GetState() { return state_; }

    public float GetOutsideTemperature() { return state_.curTemp; }
    public float GetRoomTemperature() { return state_.roomTemperature_; }

    internal bool FanStatus()
    {
      return fan_.FanStatus();
    }
  };

}
