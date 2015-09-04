package name.spanel.emdomo.accutank


import scala.swing._

class TankPanel extends Panel {
  private[this] var _tank: Option[Tank] = None

  def tank = _tank
  def tank_=(value: Tank): Unit = {
    _tank = Some(value)
    repaint()
  }


  override protected def paintComponent(g: Graphics2D) = {
    import java.awt.{Color=>JColor}
    import java.awt.RenderingHints

    super.paintComponent(g)
    val c = new JColor(150,150,80)
    g.setColor(c)
    g.fillRect(0, 0, size.width, size.height)

    g.setColor(JColor.BLACK)
    g.setRenderingHint(RenderingHints.KEY_STROKE_CONTROL, RenderingHints.VALUE_STROKE_PURE)
    g.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON)
    val textSize = 20
    g.setFont(new Font("Serif", java.awt.Font.BOLD, textSize))

    for (t <- _tank) {
      g.drawString(f"${t.topTemperature}%.1f °C", 20, 20)
      g.drawString(f"${t.levelTemp(t.levelCount/4)}%.1f °C", 20, 70)
      g.drawString(f"${t.levelTemp(t.levelCount/2)}%.1f °C", 20, 120)
      g.drawString(f"${t.levelTemp(t.levelCount*3/4)}%.1f °C", 20, 190)
      g.drawString(f"${t.botTemperature}%.1f °C", 20, 240)
    }
    //for (l <-tank.levelTemp)
  }
}

