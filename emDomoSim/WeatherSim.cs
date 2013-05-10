using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emDomoSim
{
  public struct MonthlyTemp
  {
    public double min, avg, max;
    public MonthlyTemp(double min, double avg, double max) { this.min = min; this.avg = avg; this.max = max; }

    public static MonthlyTemp operator * (MonthlyTemp t, double a) {return new MonthlyTemp(t.min*a,t.avg*a,t.max*a);}
    public static MonthlyTemp operator + (MonthlyTemp t, MonthlyTemp a) { return new MonthlyTemp(t.min + a.min, t.avg + a.avg, t.max + a.max); }
  };

  public struct SunTimes
  {
    public double rise,set;
    public SunTimes(double rise, double set) { this.rise = rise; this.set = set; }
  }

  public class WeatherSim
  {
    MonthlyTemp[] monthly;

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

    public WeatherSim()
    {
      monthly = new MonthlyTemp[]
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
    }

    public class Weather
    {
      public float minTemp,maxTemp;
      public float minTempTime, maxTempTime;
      public float curTemp;

      public Weather() { }
      public Weather(Weather w)
      {
        minTemp = w.minTemp;
        maxTemp = w.maxTemp;
        minTempTime = w.minTempTime;
        maxTempTime = w.maxTempTime;
        curTemp = w.curTemp;
      }
    };
    public Weather Simulate(int day, float timeOfDay)
    {
      Weather result = new Weather();

      double monthVal = day * 12.0 / 365 - 0.5;
      int prev = (int)Math.Floor(monthVal);
      double nextFactor = monthVal - prev;
      int next = prev + 1;
      if (prev < 0) prev += monthly.Length;
      if (next >= monthly.Length) next -= monthly.Length;
      MonthlyTemp temp = monthly[prev] * (1 - nextFactor) + monthly[next] * nextFactor;
      result.minTemp = (float)temp.min;
      result.maxTemp = (float)temp.max;
      SunTimes sunTimes = ComputeSunTimes(day);

      result.minTempTime = (float)sunTimes.rise;
      result.maxTempTime = (float)(12 + sunTimes.set) * 0.5f;

      // assume sinusoid, min in minTime, max in maxTime
      double tempFactor = 0;
      if (timeOfDay < result.minTempTime)
      {
        // assume sinus between maxTime-24h and minTime
        double timeFactor = (timeOfDay - (result.maxTempTime-24)) / (result.minTempTime - (result.maxTempTime-24));
        tempFactor = Math.Sin((timeFactor-0.5) * Math.PI)*-0.5+0.5;
      }
      else if (timeOfDay >= result.minTempTime && timeOfDay < result.maxTempTime)
      {
        // assume sinus between minTime and maxTime
        double timeFactor = (timeOfDay - result.minTempTime) / (result.maxTempTime - result.minTempTime);
        tempFactor = Math.Sin((timeFactor - 0.5) * Math.PI) * 0.5 + 0.5;
      }
      else
      {
        // assume sinus between maxTime and minTime+24
        double timeFactor = (timeOfDay - result.minTempTime) / (result.minTempTime+24 - result.maxTempTime);
        tempFactor = Math.Sin((timeFactor - 0.5) * Math.PI) * -0.5 + 0.5;
      }

      result.curTemp = (float)(result.minTempTime + (result.maxTempTime - result.minTempTime) * tempFactor);
      return result;
    }

  }
}
