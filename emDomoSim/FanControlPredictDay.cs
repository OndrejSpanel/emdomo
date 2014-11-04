using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace emDomoSim
{
  namespace FanControlPrograms
  {
    public class FanControlPredictDay: FanControl
    {
      [DllImport(
              "..\\..\\Debug\\ThermoPredict.dll",
              CharSet = CharSet.Ansi,
              CallingConvention = CallingConvention.Cdecl)]
      [return: MarshalAs(UnmanagedType.I1)]
      public static extern bool ThermoPredictSimulate(float deltaT, float outTemp, float roomTemp);

      bool fan;

      public FanControlPredictDay()
      {
        fan = false;
      }

      public void Simulate(float deltaT, FanControlInput input)
      {
        fan = ThermoPredictSimulate(deltaT, input.GetOutsideTemperature(), input.GetRoomTemperature());
      }
      public bool FanStatus() { return fan; }
      public string Name() { return "Predict Day"; }
    }

  }
}
