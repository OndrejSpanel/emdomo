package name.spanel.emdomo.accutank

import scala.util.control.Breaks._

abstract class Simulated[T <: Simulated[T]] {
  this: T =>

  def simulate(time: Float): T

  def simulateLongTime(time: Float, deltaT: Float = 1): T = {
    var ret = this
    var timeLeft = time
    breakable {
      while (true) {
        if (timeLeft>deltaT) {
          ret = ret.simulate(deltaT)
          timeLeft = timeLeft - deltaT
        }
        else {
          ret = ret.simulate(timeLeft)
          break
        }
      }
    }
    ret

  }
}

object Tank {
  /// returns power in Watts
  type HeatSource = Float => Float

  case class HeatSourceList(sources: Vector[(Int, HeatSource)]) {
    def add(pos: Int, heatSource: HeatSource) = copy(sources = sources :+ pos -> heatSource)
  }

  val kcal = 4184.0f // https://en.wikipedia.org/wiki/Calorie
  val circulationCoef = 0.1f // empirical, how much is transferred per second / degree
}

import Tank._

class Tank(val mass: Float, val levelTemp: Vector[Float], val heatSources: HeatSourceList) extends Simulated[Tank] {
  def levelCount = levelTemp.size
  def bottomLevel = levelTemp.size - 1
  def levelMass = mass / levelCount

  def this(mass: Float, levelCount:Int, initTemp:Float) = this(mass, Vector.fill(levelCount)(initTemp), HeatSourceList(Vector()))

  def copy(mass: Float = mass, levelTemp: Vector[Float] = levelTemp, heatSources: HeatSourceList = heatSources) = {
    new Tank(mass, levelTemp, heatSources)
  }

  def topTemperature = levelTemp.head
  def botTemperature = levelTemp.last

  def addHeatSource(pos: Int, heatSource: HeatSource) = copy(heatSources = heatSources.add(pos,heatSource))

  private def setTemp(pos: Int, temp: Float) = copy(levelTemp = levelTemp.patch(pos, Seq(temp), 1))

  private def simulateHeatSource(pos: Int, heatSource: HeatSource)(implicit deltaT: Float)= {
    val power = heatSource(levelTemp(pos))
    val resTemp = levelTemp(pos) + power * deltaT / (kcal * levelMass)
    setTemp(pos, resTemp)
  }

  // heat sources are applied at their position
  private def simulateHeatSources(implicit deltaT: Float) = {
    var ret = this
    for (hs <- heatSources.sources) {
      ret = ret.simulateHeatSource(hs._1, hs._2)
    }
    ret
  }

  private def simulateCirculationAtLevel(pos: Int)(implicit deltaT: Float) = {
    require(pos>0)
    val upTemp = levelTemp(pos-1)
    val downTemp = levelTemp(pos)
    if (upTemp>=downTemp) {
      this
    } else {
      val delta = downTemp - upTemp
      val transfer = delta*deltaT*circulationCoef
      val transferSaturated = transfer min delta*0.5f // assme the temperature can level at most (no mass "swap" assumed)
      setTemp(pos-1, upTemp + transferSaturated).setTemp(pos, downTemp - transferSaturated)
    }
  }
  // warm water circulates from lower to higher levels
  private def simulateCirculation(implicit deltaT: Float) = {
    var ret = this
    for (l <- levelTemp.indices.tail) {
      ret = ret.simulateCirculationAtLevel(l)
    }
    ret
  }
  def simulate(time: Float) = {
    val h = simulateHeatSources(time)
    h.simulateCirculation(time)
  }

  private def shiftTopLevelOut(bottomTemp: Float) = {
    copy(levelTemp = levelTemp.patch(0, Nil, 1) :+ bottomTemp)
  }
  def pullTopLevel(bottomTemp: Float) = {
    (levelTemp(0), shiftTopLevelOut(bottomTemp))
  }
}
