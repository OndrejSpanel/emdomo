// ThermoPredict.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "ThermoPredict.h"
#include "math.h"


/// daytime (in hours)
float currDayTime;

float sumTemp = 0;
float sumDuration = 0;

float lastTemp;

class History {
  static const int nValues = 48;
  float temp[nValues];
  int curr;

public:
  History() {
    for (int i=0; i<nValues; i++) temp[i] = 0;
    curr = 0;
  }

  void AddTemp(float t) {
    temp[curr++] = t;
    if (curr>=nValues) curr = 0;
  }

  float TempAtTime(float time) {
    int index = (int)floor(time);
    float frac = time-index;

    float pTemp = temp[(curr+nValues+index-1)%nValues];
    float nTemp = temp[(curr+nValues+index)%nValues];
    return pTemp*(1-frac)+nTemp*frac;
  }

  float Forecast(float currTemp, float time) {
    float tempNowMinus24h = TempAtTime(0);
    float tempTimeMinus24h = TempAtTime(time);
    return currTemp+tempTimeMinus24h-tempNowMinus24h;

  }
};

static History THistory;

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
    THistory.AddTemp(avgTemp);
  }
  else
  {
    sumTemp += outTemp * deltaT;
    sumDuration += deltaT;

  }
  {
    float avgTemp = sumTemp / sumDuration;
    lastTemp = avgTemp;

  }
  return lastTemp < 7;
}
