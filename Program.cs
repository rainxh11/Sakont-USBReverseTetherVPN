using System;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using SakonitoReverseTetheringUSB.Properties;
using System.Net.NetworkInformation;
using System.Management;

namespace SakonitoReverseTetheringUSB
{
    class Program
    {
        private static void DrawXH()
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("\r\n                              .    .     \r\n                              Di   Dt    \r\n                   :KW,      LE#i  E#i   \r\n                    ,#W:   ,KGE#t  E#t   \r\n                     ;#W. jWi E#t  E#t   \r\n                      i#KED.  E########f.\r\n                       L#W.   E#j..K#j...\r\n                     .GKj#K.  E#t  E#t   \r\n                    iWf  i#K. E#t  E#t   \r\n                   LK:    t#E f#t  f#t   \r\n                   i       tDj ii   ii                          \r\n                ");
            Console.ResetColor();
        }
        ///------------------------------------------------------------------------///
        private static string GetTemporaryDirectory()
        {
            DirectoryInfo temp = new DirectoryInfo(Path.GetTempPath());
            foreach(DirectoryInfo dir in temp.GetDirectories())
            {
                if (dir.Name.Contains("XHCH_"))
                {
                    try
                    {
                        Directory.Delete(dir.FullName, true);
                    }
                    catch { }
                }
            }
            string path = Path.Combine(Path.GetTempPath(), "XHCH_" + Path.GetRandomFileName() + "\\");
            Directory.CreateDirectory(path);
            return path;
        }
        ///------------------------------------------------------------------------///
        /*
        private static void ExtractResource(byte[] resFile, string resFileOut)
        {
            using (FileStream fileStream = new FileStream(resFileOut, FileMode.Create)) 
                fileStream.Write(resFile, 0, resFile.Length);
        }*/
        ///------------------------------------------------------------------------///
        private static void RunProcess(string processName, string processArg)
        {
            Process process = new Process();
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.FileName = processName;
            process.StartInfo.Arguments = processArg;
            try
            {
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                process.Dispose();
            }
        }
        ///------------------------------------------------------------------------///
        private static void ExtractZipFile(FileInfo fileInfo)
        {
            try
            {
                ZipFile.ExtractToDirectory(fileInfo.FullName, fileInfo.DirectoryName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        ///------------------------------------------------------------------------///
        private static void RemoveTAPDriver(FileInfo tapctlBin)
        {
            NetworkInterface[] netAdapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface netInterface in netAdapters)
            {
                if (netInterface.Description.Contains("TAP") || netInterface.Description.Contains("Wintun"))
                {
                    //Console.WriteLine(netInterface.Name);
                    RunProcess(tapctlBin.FullName, $"delete \"{netInterface.Name}\""); // Remove TAP/TUN Driver                             
                    //Console.ReadKey();
                }
            }
        }
        ///------------------------------------------------------------------------///
        private static void ConfigureNetAdapters(NetworkInterface[] netAdapters, FileInfo pwshBin)
        {
            foreach (NetworkInterface netInterface in netAdapters)
            {
                if (netInterface.Description.Contains("NDIS"))
                {
                    RunProcess(pwshBin.FullName, $"-Command \"Get-NetAdapter -InterfaceDescription \"Remote*\" | New-NetIPAddress  -IPAddress 192.168.42.128 -DefaultGateway 192.168.42.129 -PrefixLength 24\"");
                }
                if(netInterface.Description.Contains("TAP-Windows Adapter"))
                {
                    RunProcess(pwshBin.FullName, $"-Command \"Set-MrInternetConnectionSharing -InternetInterfaceName Ethernet -LocalInterfaceName '{netInterface.Name}' -Enabled $true\"");
                    RunProcess(pwshBin.FullName, $"-Command \"Set-MrInternetConnectionSharing -InternetInterfaceName Wi-Fi -LocalInterfaceName '{netInterface.Name}' -Enabled $true\"");
                }
            }
        }
        ///------------------------------------------------------------------------///
        static void Main(string[] args)
        {
            Console.Title = "Sakont-USBReverseTether-OpenVPN";
            DrawXH();
            string currentDir = AppContext.BaseDirectory;
            ///------------------------------------------------------------------------///
            FileInfo pwshBin = new FileInfo(currentDir + @"powershell\pwsh.exe");

            FileInfo tapDriverFile = new FileInfo(currentDir + @"tapdriver\OemVista.inf");
            FileInfo tunDriverFile = new FileInfo(currentDir + @"wintundriver\wintun.inf");

            FileInfo devconBin = new FileInfo(currentDir + @"openvpn\devcon.exe");
            FileInfo tapctlBin = new FileInfo(currentDir + @"openvpn\tapctl.exe");

            FileInfo openvpnExe = new FileInfo(currentDir + @"openvpn\openvpn.exe");
            FileInfo openvpnServerConfig = new FileInfo(currentDir + @"server\ServerXH.ovpn");

            FileInfo driversInstaller = new FileInfo(currentDir + @"driver\DriverSetup.exe");
            FileInfo adbExe = new FileInfo(currentDir + @"adb\adb.exe");

            FileInfo openvpnApk = new FileInfo(currentDir + @"client\OpenVPN.apk");
            FileInfo openvpnClientConfig = new FileInfo(currentDir + @"client\AndroidXH.ovpn");
            ///------------------------------------------------------------------------///
            while (true)
            {
                Console.Clear();
                DrawXH();
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("======= Choose MOTHERFUCKER: =======");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[1] - Install Android Drivers");
                Console.WriteLine("[2] - Install TAP/TUN Driver");
                Console.WriteLine("[3] - Remove TAP/TUN Driver");
                Console.WriteLine("[4] - ADB Push OpenVPN.apk to Phone");
                Console.WriteLine("[5] - Configure Network Adapters");
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("[DEFAULT] - Start Reverse-Tether USB OpenVPN Server (PRESS ENTER)");
                Console.ResetColor();
                ///------------------------------------------------------------------------///

                ConsoleKeyInfo input = Console.ReadKey();
                switch (input.Key)
                {
                    case ConsoleKey.NumPad1:
                        Console.Clear();
                        DrawXH();
                        RunProcess(driversInstaller.FullName, null); // Install Android Drivers
                        break;
                    case ConsoleKey.NumPad2:
                        Console.Clear();
                        DrawXH();
                        RunProcess(devconBin.FullName, $"install {tapDriverFile} tap0901"); // Install TAP Driver 
                        RunProcess(devconBin.FullName, $"install {tunDriverFile} wintun"); // Install TUN Driver 
                        break;
                    case ConsoleKey.NumPad3:
                        Console.Clear();
                        DrawXH();
                        RemoveTAPDriver(tapctlBin);
                        break;
                    case ConsoleKey.NumPad4:
                        Console.Clear();
                        DrawXH();
                        RunProcess(adbExe.FullName, $"install {openvpnApk.FullName}"); // Install OpenVPN.apk to the phone
                        RunProcess(adbExe.FullName, $"push {openvpnClientConfig.FullName}"); // Push AndroidXH.ovpn to the phone
                        break;
                    case ConsoleKey.NumPad5:
                        Console.Clear();
                        DrawXH();
                        NetworkInterface[] netAdapters = NetworkInterface.GetAllNetworkInterfaces();
                        RunProcess(pwshBin.FullName, $"-Command \"Set-ExecutionPolicy -ExecutionPolicy Unrestricted\"");
                        ConfigureNetAdapters(netAdapters, pwshBin);
                        break;
                    case ConsoleKey.Enter:
                        Console.Clear();
                        DrawXH();
                        NetworkInterface[] netAdapters2 = NetworkInterface.GetAllNetworkInterfaces();
                        RunProcess(pwshBin.FullName, $"-Command \"Set-ExecutionPolicy -ExecutionPolicy Unrestricted\"");
                        ConfigureNetAdapters(netAdapters2, pwshBin);
                        RunProcess(openvpnExe.FullName, $@"--config {openvpnServerConfig.FullName} --client-config-dir {currentDir}server"); // Run OpenVPN Server
                        break;
                }

            }
            ///--------------------------------------------------------------------------////
        }
    }
}