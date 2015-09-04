package name.spanel.emdomo.accutank


import scala.swing._

class TankPanel extends Panel {
  private[this] var _tank: Option[Tank] = None
  private[this] var _time: Float = 0f
  private[this] var _power: Boolean = false


  def tank = _tank

  def tank_=(value: Tank): Unit = {
    _tank = Some(value)
  }

  def time: Float = _time

  def time_=(value: Float): Unit = {
    _time = value
  }

  def power: Boolean = _power

  def power_=(value: Boolean): Unit = {
    _power = value
  }

  override protected def paintComponent(g: Graphics2D) = {
    import java.awt.{Color => JColor}
    import java.awt.RenderingHints

    super.paintComponent(g)
    val c = new JColor(150, 150, 80)
    g.setColor(c)
    g.fillRect(0, 0, size.width, size.height)

    g.setRenderingHint(RenderingHints.KEY_STROKE_CONTROL, RenderingHints.VALUE_STROKE_PURE)
    g.setRenderingHint(RenderingHints.KEY_ANTIALIASING, RenderingHints.VALUE_ANTIALIAS_ON)
    val textSize = 20
    g.setFont(new Font("Serif", java.awt.Font.BOLD, textSize))

    for (t <- _tank) {
      for ((l, i) <- t.levelTemp.zipWithIndex) {
        val h = 5
        def tempColor(temp: Float) = {
          val rTemp = 80f
          val gTemp = 20f
          val hot = if (temp >= rTemp) 1
          else if (temp <= gTemp) 0
          else (temp - gTemp) / (rTemp - gTemp)
          val hue = (1 - hot) * (240f / 360)

          val tc = JColor.getHSBColor(hue, 1, 0.7f)
          tc
        }
        val tc = tempColor(l)
        g.setColor(tc)
        g.fillRect(20, 20 + i * h, 60, h)
      }
      g.setColor(JColor.BLACK)
      g.drawString(f"${t.topTemperature}%.1f °C", 20, 40)
      g.drawString(f"${t.levelTemp(t.levelCount / 4)}%.1f °C", 20, 95)
      g.drawString(f"${t.levelTemp(t.levelCount / 2)}%.1f °C", 20, 150)
      g.drawString(f"${t.levelTemp(t.levelCount * 3 / 4)}%.1f °C", 20, 205)
      g.drawString(f"${t.botTemperature}%.1f °C", 20, 260)

      val day = math.floor(_time / (24 * 3600))
      val timeInDay = _time - day * (24 * 3600)
      val hour = math.floor(timeInDay / 3600)
      val minute = (timeInDay - hour * 3600) / 60

      g.drawString(f"$day%.0f $hour%2.0f:${math.floor(minute)}%2.0f", 20, 300)

      val color = if (power) JColor.green else JColor.darkGray
      g.setColor(color)
      g.fillRect(10, 10, 10, 10)
    }
  }
}

