using System.Numerics;

namespace CsCheats.Domain;

public class Game
{

  public Player player { get; set; }

  public Entity entity { get; set; }

  public GlowObject glowObject { get; set; }

  public int clientState { get; set; }


  public static float CalcMagnitude(Vector3 player, Vector3 enemy)
  {
    var result = (float)Math.Sqrt(
     Math.Pow(enemy.X - player.X, 2) +
     Math.Pow(enemy.Y - player.Y, 2) +
     Math.Pow(enemy.Z - player.Z, 2)
    );

    return result;
  }
}