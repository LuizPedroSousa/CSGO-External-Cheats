using System.Numerics;

namespace CsCheats.Domain;

public class Entity
{
  public int address { get; set; }
  public int team { get; set; }
  public Vector3 head { get; set; }
  public Vector3 feet { get; set; }
  public int hp { get; set; }

  public float magnitude { get; set; }

  public bool spotted { get; set; }

  public bool dormant { get; set; }

  public static Vector3 FormatHead(byte[] bones)
  {
    return new Vector3(
      x: BitConverter.ToSingle(bones, 0xC), // 3x4
      y: BitConverter.ToSingle(bones, 0x1C),
      z: BitConverter.ToSingle(bones, 0x2C)
    );
  }
}