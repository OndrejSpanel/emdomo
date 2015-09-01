package name.spanel.emdomo.accutank

object Tank {
  type HeatSource = () => Float

  case class HeatSourceList(sources: Vector[(Int, HeatSource)]) {
    def add(pos: Int, heatSource: HeatSource) = copy(sources = sources :+ pos -> heatSource)
  }
}

import Tank._

class Tank(val levels: Vector[Float], val heatSources: HeatSourceList) {
  def levelCount = levels.size

  def this(levelCount:Int, initTemp:Float) = this(Vector.fill(levelCount)(initTemp), HeatSourceList(Vector()))

  def copy(levels: Vector[Float] = levels, heatSources: HeatSourceList = heatSources) = {
    new Tank(levels, heatSources)
  }

  def topTemperature = levels.head
  def botTemperature = levels.last

  def addHeatSource(pos: Int, heatSource: HeatSource) = copy(heatSources = heatSources.add(pos,heatSource))

  private def simulateHeatSources(implicit deltaT: Float) = {
    this
  }
  private def simulateCirculation(implicit deltaT: Float) = {
    this
  }
  def simulate(implicit deltaT: Float) = {
    val h = simulateHeatSources
    h.simulateCirculation
  }
}
