package name.spanel.emdomo.accutank

object Tank {
  /// returns power in Watts
  type HeatSource = () => Float

  case class HeatSourceList(sources: Vector[(Int, HeatSource)]) {
    def add(pos: Int, heatSource: HeatSource) = copy(sources = sources :+ pos -> heatSource)
  }

  val kcal = 4184.0f // https://en.wikipedia.org/wiki/Calorie
}

import Tank._

class Tank(val mass: Float, val levelTemp: Vector[Float], val heatSources: HeatSourceList) {
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
    val power = heatSource()
    val resTemp = levelTemp(pos) + power * deltaT / kcal
    setTemp(pos, resTemp)
  }
  private def simulateHeatSources(implicit deltaT: Float) = {
    var ret = this
    for (hs <- heatSources.sources) {
      ret = ret.simulateHeatSource(hs._1, hs._2)
    }
    ret
  }
  private def simulateCirculation(implicit deltaT: Float) = {
    this
  }
  def simulate(implicit deltaT: Float) = {
    val h = simulateHeatSources
    h.simulateCirculation
  }
}
