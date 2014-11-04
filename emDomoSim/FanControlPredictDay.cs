using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace emDomoSim
{
  namespace FanControlPrograms
  {
    public class FanControlPredictDay: FanControl
    {
      /// daytime (in hours)
      float currDayTime;

      float sumTemp;
      float sumDuration;

      public FanControlPredictDay()
      {
        sumTemp = 0;
        sumDuration = 0;
      }
      public void Simulate(float deltaT, FanControlInput input)
      {
        var temp = input.GetOutsideTemperature();
        // plot a curve, try to fit the history
        if (sumDuration + deltaT >= 1)
        {
          float deltaTIn = 1-sumDuration;
          float deltaTOut = sumDuration + deltaT - 1;
          sumTemp += temp * deltaTIn;
          float avgTemp = sumTemp;
          sumTemp = deltaTOut;
          sumDuration = 0;
        }
        else
        {
          sumTemp += temp*deltaT;
          sumDuration += deltaT;

        }
        {
          float avgTemp = sumTemp / sumDuration;

        }
      }
      public bool FanStatus() { return currDayTime<6; }
      public string Name() { return "Predict Day"; }
    }

  }
}
