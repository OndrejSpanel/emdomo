package name.spanel.emdomo.accutank

class TankTest extends org.scalatest.FunSuite {
  test("Simple tank can be created") {
    val tank = new Tank(1, 60)
    assert(tank.levelCount==1)
  }
}
