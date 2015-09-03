package name.spanel.emdomo.accutank

import scala.swing._

class TankPanel extends Panel {
  override protected def paintComponent(g: Graphics2D) = {
    super.paintComponent(g)
    val c = new java.awt.Color(150,150,80)
    g.setColor(c)
    g.fillRect(0, 0, size.width, size.height)
  }
}
