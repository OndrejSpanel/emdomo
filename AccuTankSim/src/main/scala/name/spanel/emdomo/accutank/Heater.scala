package name.spanel.emdomo.accutank

class Heater(val maxTemp: Float, val power: Float) extends ((Float) => Float) {
  override def apply(temp: Float) = {
    if (temp<maxTemp) power
    else 0
  }
}
