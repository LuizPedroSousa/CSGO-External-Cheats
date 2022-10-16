
namespace CsCheats.SkinChange;

using System.Diagnostics;
using CsCheats.Memory;

public class Program
{
  public async static Task Main()
  {

    Console.Title = "CSGO External Skin-Change Hack";

    Offsets.Root offsets = await Offsets.FetchOffsets();

    Process gameProcess = MemoryManager.OpenGame("csgo");

    int clientDll = MemoryManager.GetModuleBaseAddress(gameProcess, "client.dll");
    int engineDll = MemoryManager.GetModuleBaseAddress(gameProcess, "engine.dll");

    while (true)
    {
      int clientState = MemoryManager.Read<int>(engineDll + offsets.signatures.dwClientState);
      int maxPlayers = MemoryManager.Read<int>(clientState + offsets.signatures.dwClientState_MaxPlayer);
      int playerAddress = MemoryManager.Read<int>(clientDll + offsets.signatures.dwLocalPlayer);
      int playerHp = MemoryManager.Read<int>(playerAddress + offsets.netvars.m_iHealth);

      if (maxPlayers < 1 || playerHp < 1)
      {
        await Task.Delay(500);
        continue;
      }

      for (int i = 0; i < 3; i++)
      {
        int weapon = GetCurrentWeapon(playerAddress, i);

        if (weapon == 0)
        {
          continue;
        }

        ApplySkin(clientState: clientState, weapon, 524, 1f);
      }
    }

    int GetCurrentWeapon(int playerAddress, int weaponIndex)
    {

      var currentWeapon =
        MemoryManager.Read<int>(playerAddress + offsets.netvars.m_hMyWeapons + weaponIndex * 0x4, 4)
       & 0xfff;

      if (currentWeapon == 0)
      {
        return 0;
      }

      var weaponAddress =
        MemoryManager.Read<int>(clientDll + offsets.signatures.dwEntityList + (currentWeapon - 1) * 0x10, 4);

      var weapponId =
        MemoryManager.Read<int>(weaponAddress + offsets.netvars.m_iItemDefinitionIndex, 2);


      return weaponAddress;
    }

    void ApplySkin(int clientState, int weapponAddress, int skinId, float wear)
    {

      var currentSkin =
          MemoryManager.Read<int>(weapponAddress + offsets.netvars.m_nFallbackPaintKit, 4);

      if (currentSkin != skinId)
      {

        MemoryManager.Write<int>(
          weapponAddress + offsets.netvars.m_iItemIDHigh, -1
        );


        MemoryManager.Write<int>(
          weapponAddress + offsets.netvars.m_nFallbackPaintKit, skinId
        );


        MemoryManager.Write<float>(
          weapponAddress + offsets.netvars.m_flFallbackWear, wear
        );

        MemoryManager.Write<int>(
          clientState + 0x174, -1
        );
      }
    }

  }
}