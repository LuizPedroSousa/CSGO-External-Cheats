namespace CsCheats.Aimbot;

using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
using CsCheats.Domain;
using CsCheats.Memory;

public class Program
{
  [DllImport("user32.dll")]
  public static extern short GetAsyncKeyState(
    int vKey
  );

  public static async Task Main()
  {
    var offsets = await Offsets.FetchOffsets();
    Process gameProcess = MemoryManager.OpenGame("csgo");


    int clientDll = MemoryManager.GetModuleBaseAddress(gameProcess, "client.dll");
    int engineDll = MemoryManager.GetModuleBaseAddress(gameProcess, "engine.dll");

    Game game = new Game();


    List<Entity> entities = new List<Entity>();
    while (true)
    {

      int clientState = MemoryManager.Read<int>(engineDll + offsets.signatures.dwClientState);
      int maxPlayers = MemoryManager.Read<int>(clientState + offsets.signatures.dwClientState_MaxPlayer);

      if (maxPlayers < 1)
      {
        continue;
      }

      ReadPlayer();
      ReadEntities(maxPlayers);

      if (GetAsyncKeyState(0x06) < 0 && entities.Count > 1)
      {
        float smooth = 20f;
        float magnitudeDown = 30f;

        float deltaX = (entities[0].head.X - game.player.feet.X);
        float deltaY = (entities[0].head.Y - game.player.feet.Y);
        float deltaZ = (entities[0].head.Z - game.player.feet.Z);

        float x = (float)(Math.Atan2(deltaY, deltaX) * 180 / Math.PI);
        float y = -(float)(Math.Atan2(deltaZ, entities[0].magnitude + magnitudeDown) * 180 / Math.PI);

        var normalizeVector = (float oldVector, float vector) => ((vector - oldVector) / smooth + oldVector);

        float oldY = MemoryManager.Read<float>(clientState + offsets.signatures.dwClientState_ViewAngles);
        float oldX = MemoryManager.Read<float>(clientState + offsets.signatures.dwClientState_ViewAngles + 0x4);

        MemoryManager.Write<float>(clientState + offsets.signatures.dwClientState_ViewAngles, normalizeVector(oldY, y));
        MemoryManager.Write<float>(clientState + offsets.signatures.dwClientState_ViewAngles + 0x4, normalizeVector(oldX, x));
      }
    }

    void ReadPlayer()
    {
      game.player = new Player()
      {
        address = MemoryManager.Read<int>(clientDll + offsets.signatures.dwLocalPlayer)
      };
      game.player.team = MemoryManager.Read<int>(game.player.address + offsets.netvars.m_iTeamNum, 4);

      var vecOrigiginBytes = MemoryManager.ReadBytes(game.player.address + offsets.netvars.m_vecOrigin, 12);
      var vecViewBytes = MemoryManager.ReadBytes(game.player.address + offsets.netvars.m_vecViewOffset, 12);


      var feet = new Vector3(
         BitConverter.ToSingle(vecOrigiginBytes, 0),
         BitConverter.ToSingle(vecOrigiginBytes, 4),
         BitConverter.ToSingle(vecOrigiginBytes, 8)
       );




      feet.Z += MemoryManager.Read<float>(game.player.address + offsets.netvars.m_vecViewOffset + 0x8, 4);

      game.player.feet = feet;
    }

    void ReadEntities(int maxPlayers)
    {
      entities.Clear();
      for (int i = 0; i < maxPlayers; i++)
      {

        int address = MemoryManager.Read<int>(clientDll + offsets.signatures.dwEntityList + i * 0x10);




        int hp = MemoryManager.Read<int>(address + offsets.netvars.m_iHealth);
        bool dormant = MemoryManager.Read<bool>(address + offsets.signatures.m_bDormant);


        int team = MemoryManager.Read<int>(address + offsets.netvars.m_iTeamNum);


        if (team == game.player.team || dormant || hp < 1)
          continue;



        int bonesAddress = MemoryManager.Read<int>(address + offsets.netvars.m_dwBoneMatrix);

        byte[] bone = MemoryManager.ReadBytes(bonesAddress + 0x30 * 8, 0x30);

        Vector3 head = Entity.FormatHead(bone);


        Entity entity = new Entity()
        {
          address = address,
          team = team,
          hp = hp,
          dormant = dormant,
          head = head,
          magnitude = Game.CalcMagnitude(game.player.feet, head)
        };

        entities.Add(entity);
      }

      entities = entities.OrderBy(entity => entity.magnitude).ToList();
    }
  }
}
