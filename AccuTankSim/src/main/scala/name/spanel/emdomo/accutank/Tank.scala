package name.spanel.emdomo.accutank

class Tank(val levelCount:Int, initTemp:Float) {
  val level = Vector.fill(levelCount)(initTemp)

  def topTemperature = level.head
  def botTemperature = level.last
}
