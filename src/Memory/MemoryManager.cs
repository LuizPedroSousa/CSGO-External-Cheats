using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CsCheats.Memory;
public class MemoryManager
{

  [DllImport("kernel32.dll")]
  private static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, string buffer, int size, out int lpNumberOfBytesWritten);

  [DllImport("kernel32.dll")]
  public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

  [DllImport("kernel32.dll")]
  private static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] buffer, int size, ref int lpNumberOfBytesRead);

  [DllImport("kernel32.dll")]
  private static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] buffer, int size, out int lpNumberOfBytesWritten);

  public static IntPtr processHandle { get; set; }

  private static int bytesRead = 0;

  private static int bytesWritten = 0;



  public static Process OpenGame(string name)
  {
    Process gameProcess = Process.GetProcessesByName("csgo")[0];


    if (gameProcess.Id < 1)
    {
      Console.WriteLine("Could not find csgo");
      Process.GetCurrentProcess().Kill();
    }

    // VM OPERATION | VM READ | VM WRITE
    MemoryManager.processHandle = MemoryManager.OpenProcess(0x0008 | 0x0010 | 0x0020, false, gameProcess.Id);

    return gameProcess;
  }

  public static int GetModuleBaseAddress(Process process, string moduleName)
  {
    var module = process.Modules.Cast<ProcessModule>().SingleOrDefault(module => module.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase));

    return (int)module.BaseAddress;
  }

  public static T Read<T>(int address) where T : struct
  {
    int bufferSize = Marshal.SizeOf<T>();
    byte[] buffer = new byte[bufferSize];

    ReadProcessMemory((int)processHandle, address, buffer, bufferSize, ref bytesRead);

    return BytesToStructure<T>(buffer);
  }

  public static T Read<T>(int address, int size) where T : struct
  {
    byte[] buffer = new byte[size];

    ReadProcessMemory((int)processHandle, address, buffer, size, ref bytesRead);

    return BytesToStructure<T>(buffer);
  }

  public static byte[] ReadBytes(int address, int size)
  {
    byte[] buffer = new byte[size];

    ReadProcessMemory((int)processHandle, address, buffer, size, ref bytesRead);

    return buffer;

  }

  public static void Write<T>(int address, T value) where T : struct
  {
    byte[] buffer = StructureToBytes(value);

    WriteProcessMemory((int)processHandle, address, buffer, buffer.Length, out bytesRead);
  }



  public static T BytesToStructure<T>(byte[] bytes) where T : struct
  {
    var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

    try
    {
      return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
    }
    finally
    {
      handle.Free();
    }
  }


  public static byte[] StructureToBytes(object structure)
  {
    int size = Marshal.SizeOf(structure);

    byte[] bytes = new byte[size];

    IntPtr pointer = Marshal.AllocHGlobal(size);

    Marshal.StructureToPtr(structure, pointer, true);
    Marshal.Copy(pointer, bytes, 0, size);
    Marshal.FreeHGlobal(pointer);

    return bytes;
  }

}
