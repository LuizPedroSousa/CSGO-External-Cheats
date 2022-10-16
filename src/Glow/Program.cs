using System.Diagnostics;
using CsCheats.Domain;
using CsCheats.Memory;

namespace CsCheats.Glow;

public class Program
{
  public async static Task Main()
  {
    Offsets.Root offsets = await Offsets.FetchOffsets();
    Process gameProcess = MemoryManager.OpenGame("csgo");

    int clientDll = MemoryManager.GetModuleBaseAddress(gameProcess, "client.dll");
    int engineDll = MemoryManager.GetModuleBaseAddress(gameProcess, "engine.dll");

    GlowRenderSettings glowSettings = new GlowRenderSettings
    {
      teamColor = new GlowColor
      {
        red = 0,
        green = 0,
        blue = 1,
        alpha = 1.8f
      },
      enemiesColor = new GlowColor
      {
        red = 1,
        green = 0,
        blue = 0,
        alpha = 1.7f
      },
      enemiesBloom = new GlowBloom { renderOccluded = true, renderUnoccluded = false },
      teamBloom = new GlowBloom { renderOccluded = true, renderUnoccluded = false }
    };

    Game game = new Game()
    {
      glowObject = new GlowObject(glowSettings)
    };




    while (true)
    {

      game.clientState = MemoryManager.Read<int>(engineDll + offsets.signatures.dwClientState);

      int playersAmount = MemoryManager.Read<int>(game.clientState + offsets.signatures.dwClientState_MaxPlayer);
      game.glowObject.manager = MemoryManager.Read<int>(clientDll + offsets.signatures.dwGlowObjectManager);
      ReadLocalPlayer();

      for (int i = 0; i < playersAmount; i++)
      {
        ReadEntityValues(i);

        if (game.entity.dormant || game.entity.hp < 1)
          continue;

        if (game.entity.team != game.player.team)
        {
          WriteGlow(game.glowObject.settings.enemiesColor, game.glowObject.settings.enemiesBloom);
        }
        else
        {
          WriteGlow(game.glowObject.settings.teamColor, game.glowObject.settings.teamBloom);
        }

      }
    }


    void ReadLocalPlayer()
    {
      int localPlayerAddress = MemoryManager.Read<int>(clientDll + offsets.signatures.dwLocalPlayer);

      game.player = new Player()
      {
        address = localPlayerAddress,
        team = MemoryManager.Read<int>(localPlayerAddress + offsets.netvars.m_iTeamNum)
      };
    }

    void ReadEntityValues(int currentPlayer)
    {
      int address = MemoryManager.Read<int>(clientDll + offsets.signatures.dwEntityList + currentPlayer * 0x10);

      game.entity = new Entity()
      {
        address = address,
        team = MemoryManager.Read<int>(address + offsets.netvars.m_iTeamNum),
        hp = MemoryManager.Read<int>(address + offsets.netvars.m_iHealth),
        dormant = MemoryManager.Read<bool>(address + offsets.signatures.m_bDormant)
      };
    }


    void WriteGlow(GlowColor color, GlowBloom bloom)
    {
      game.glowObject.index = MemoryManager.Read<int>(game.entity.address + offsets.netvars.m_iGlowIndex);

      rgba colorRender = new rgba
      {
        //*255 idea from: https://stackoverflow.com/a/46575472/12897035
        r = (byte)Math.Round(color.red * 255.0),
        g = (byte)Math.Round(color.green * 255.0),
        b = (byte)Math.Round(color.blue * 255.0),
        a = (byte)Math.Round(color.alpha * 255.0)
      };

      MemoryManager.Write<GlowColor>(game.glowObject.manager + (game.glowObject.index * 0x38) + 0x8, color);
      MemoryManager.Write<rgba>(game.entity.address + offsets.netvars.m_clrRender, colorRender);
      MemoryManager.Write<GlowBloom>(game.glowObject.manager + ((game.glowObject.index * 0x38) + 0x28), bloom);
      MemoryManager.Write<bool>(game.entity.address + offsets.netvars.m_bSpotted, true);
    }
  }


  struct rgba//Overlay player color or "chams"
  {
    public byte r;
    public byte g;
    public byte b;
    public byte a;
  }

}