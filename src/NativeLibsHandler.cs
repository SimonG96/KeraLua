using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace KeraLua
{
    internal static class NativeLibsHandler
    {
        public static void RegisterPathForDll(string name)
        {
            if (!name.EndsWith(".dll"))
                throw new ArgumentException("DLL name has to include file extension .dll.", nameof(name));

            string assemblyPath = GetAssemblyPath();
            string path = Path.Combine(assemblyPath, name);

            // If the .dll already exists in the current assembly path don't try register another path
            if (File.Exists(path))
                return;

            if (IntPtr.Size == 8)
                Register64BitPath(assemblyPath, name);
            else
                Register32BitPath(assemblyPath, name);
        }

        private static string GetAssemblyPath()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);

            return Path.GetDirectoryName(path);
        }

        private static void Register64BitPath(string assemblyPath, string dllName)
        {
            string x64Path = Path.Combine(assemblyPath, "x64");
            if (Directory.Exists(x64Path) && File.Exists(Path.Combine(x64Path, dllName)))
            {
                Kernel32.SetDllDirectory(x64Path);
                return;
            }

            string amd64Path = Path.Combine(assemblyPath, "amd64");
            if (Directory.Exists(amd64Path) && File.Exists(Path.Combine(amd64Path, dllName)))
            {
                Kernel32.SetDllDirectory(amd64Path);
                return;
            }

            throw new DllNotFoundException($"Could not find {dllName} in directory {assemblyPath}, {x64Path} or {amd64Path}");
        }

        private static void Register32BitPath(string assemblyPath, string dllName)
        {
            string x86Path = Path.Combine(assemblyPath, "x86");
            if (Directory.Exists(x86Path) && File.Exists(Path.Combine(x86Path, dllName)))
            {
                Kernel32.SetDllDirectory(x86Path);
                return;
            }

            string i386Path = Path.Combine(assemblyPath, "i386");
            if (Directory.Exists(i386Path) && File.Exists(Path.Combine(i386Path, dllName)))
            {
                Kernel32.SetDllDirectory(i386Path);
                return;
            }

            throw new DllNotFoundException($"Could not find {dllName} in directory {assemblyPath}, {x86Path} or {i386Path}");
        }

        private static class Kernel32
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern bool SetDllDirectory(string path);
        }
    }
}
