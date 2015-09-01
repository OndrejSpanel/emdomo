package name.spanel.emdomo.accutank

import org.scalactic.Tolerance._

class TankTest extends org.scalatest.FunSuite {
  test("Simple tank can be created") {
    val initTemp = 60.0f
    val eps = 1e-3f
    val tank = new Tank(1, initTemp)
    assert(tank.levelCount == 1)
    assert(tank.topTemperature === initTemp +- eps )
    assert(tank.botTemperature === initTemp +- eps)
  }
}
