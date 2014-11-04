// ThermoPredict.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "ThermoPredict.h"


/// daytime (in hours)
float currDayTime;

float sumTemp = 0;
float sumDuration = 0;

THERMOPREDICT_API
bool ThermoPredictSimulate(float deltaT, float outTemp, float roomTemp)
{
  // plot a curve, try to fit the history
  if (sumDuration + deltaT >= 1)
  {
    float deltaTIn = 1-sumDuration;
    float deltaTOut = sumDuration + deltaT - 1;
    sumTemp += outTemp * deltaTIn;
    float avgTemp = sumTemp;
    sumTemp = deltaTOut;
    sumDuration = 0;
  }
  else
  {
    sumTemp += outTemp * deltaT;
    sumDuration += deltaT;

  }
  {
    float avgTemp = sumTemp / sumDuration;

  }
  return false;
}
