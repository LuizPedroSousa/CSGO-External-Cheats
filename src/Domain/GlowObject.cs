namespace CsCheats.Domain;


public struct GlowColor
{
  public float red { get; set; }
  public float green { get; set; }
  public float blue { get; set; }
  public float alpha { get; set; }
}

public struct GlowBloom
{

  public bool renderOccluded { get; set; }
  public bool renderUnoccluded { get; set; }

  public GlowBloom(bool renderOccluded, bool renderUnoccluded)
  {
    this.renderOccluded = renderOccluded;
    this.renderUnoccluded = renderUnoccluded;
  }
}

public struct GlowRenderSettings
{

  public GlowColor teamColor { get; set; }
  public GlowColor enemiesColor { get; set; }
  public GlowBloom enemiesBloom { get; set; } = new GlowBloom() { renderOccluded = true };
  public GlowBloom teamBloom { get; set; } = new GlowBloom() { renderOccluded = true };

  public GlowRenderSettings(GlowColor teamColor, GlowColor enemiesColor, GlowBloom enemiesBloom, GlowBloom teamBloom)
  {
    this.teamColor = teamColor;
    this.enemiesColor = enemiesColor;
    this.enemiesBloom = enemiesBloom;
    this.teamBloom = teamBloom;
  }
}


public class GlowObject
{
  public GlowObject(GlowRenderSettings settings)
  {
    this.settings = settings;
  }

  public int index { get; set; }
  public int manager { get; set; }

  public GlowRenderSettings settings { get; set; }
}