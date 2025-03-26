using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

public class Program
{
    private static string[] _paths = new string[] { "unpacked_apk", "decoded_apk_data" };

    public static void Main()
    {
        Console.Title = "SimpleAPKUnpacker | Made by https://github.com/ZygoteCode/";
        string inputApkPath = "";

        string javaInvokeFile = File.ReadAllText("d2j_invoke.bat");
        javaInvokeFile = javaInvokeFile.Replace("$JAVA_PATH", Path.GetFullPath("java\\bin\\java.exe"));
        File.WriteAllText("d2j_invoke.bat", javaInvokeFile);

        while (!File.Exists(inputApkPath) || !Path.GetExtension(inputApkPath).ToLower().Equals(".apk"))
        {
            Console.Write("Please, insert the path of the APK file to unpack here > ");
            inputApkPath = CorrectPath(Console.ReadLine());

            if (!File.Exists(inputApkPath))
            {
                Console.Write("The specified file does not exist. Please, try again.\r\n");
            }
            else if (!Path.GetExtension(inputApkPath).ToLower().Equals(".apk"))
            {
                Console.Write("The specified file has not a valid extension (it must be *.apk). Please, try again.\r\n");
            }
        }

        foreach (string folder in _paths)
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }

            Directory.CreateDirectory(folder);
            Console.WriteLine($"[!] Created folder '{folder}'.");
        }

        Console.WriteLine("[!] Unpacking the APK file in 'unpacked_apk' folder.");

        try
        {
            ZipFile.ExtractToDirectory(inputApkPath, "unpacked_apk");
        }
        catch
        {

        }
        
        Console.WriteLine("[!] Succesfully unpacked the APK file in 'unpacked_apk' folder.");
        Console.WriteLine("[!] Decoding the 'AppManifest.xml' file.");
        RunExecutable("axmldec.exe", $"-o \"{Path.GetFullPath("decoded_apk_data\\AndroidManifest.xml")}\" \"{Path.GetFullPath("unpacked_apk\\AndroidManifest.xml")}\"");
        Console.WriteLine("[!] Succesfully decoded the 'AppManifest.xml' file.");
        Console.WriteLine("[!] Initialized DEX classes to JAR conversion.");

        foreach (string file in Directory.GetFiles("unpacked_apk"))
        {
            if (Path.GetExtension(file).ToLower().Equals(".dex"))
            {
                Console.WriteLine($"[!] Converting '{Path.GetFileName(file)}' to a JAR file.");
                RunExecutable("d2j-dex2jar.bat", $"\"{file}\"");
                Console.WriteLine($"[!] Succesfully converted '{Path.GetFileName(file)}' to a JAR file.");
            }
        }

        if (File.Exists("classes-error.zip"))
        {
            File.Delete("classes-error.zip");
            Console.WriteLine("[!] Found the file 'classes-error.zip', deleted.");
        }
        else
        {
            Console.WriteLine("[!] File 'classes-error.zip' not found.");
        }

        Console.WriteLine("[!] Initialized JAR files moving to the APK decoded data folder.");

        foreach (string file in Directory.GetFiles(Environment.CurrentDirectory))
        {
            if (Path.GetExtension(file).ToLower().Equals(".jar"))
            {
                if (Path.GetFileNameWithoutExtension(file).ToLower().Equals("cfr_decompiler"))
                {
                    Console.WriteLine("[!] Skipped the CFR decompiler JAR file from moving.");
                    continue;
                }

                string fileName = Path.GetFileName(file).Replace("-dex2jar", "");
                File.Move(file, Path.GetFullPath($"decoded_apk_data\\{fileName}"));
                Console.WriteLine($"[!] Succesfully moved the file '{fileName}'.");
            }
        }

        Console.WriteLine("[!] Initialized the decompilation phase of all JAR files.");
        Directory.CreateDirectory("decoded_apk_data\\decompiled");

        foreach (string file in Directory.GetFiles("decoded_apk_data"))
        {
            if (Path.GetExtension(file).ToLower().Equals(".jar"))
            {
                string theName = Path.GetFileNameWithoutExtension(file);
                Console.WriteLine($"[!] Decompiling '{theName}.jar' file, it may take a while.");
                Directory.CreateDirectory($"decoded_apk_data\\decompiled\\{theName}");

                string batchContent = $"\"{Path.GetFullPath("java\\bin\\java.exe")}\" " +
                    $"-jar \"{Path.GetFullPath("cfr_decompiler.jar")}\" \"{Path.GetFullPath(file)}\" " +
                    $"--outputdir \"{Path.GetFullPath($"decoded_apk_data\\decompiled\\{theName}")}\"";
                Console.WriteLine(">>>>>   >>>>>>> > " + batchContent);
                File.WriteAllText("execution.bat", batchContent);
                RunExecutable("execution.bat", "");

                Console.WriteLine($"[!] Succesfully decompiled '{theName}.jar' file.");
            }
        }

        File.Delete("execution.bat");

        Console.WriteLine("[!] APK unpacking & decoding succesfully finished. ApkManifest.xml is now decoded and DEX classes files are now completely visible.");
        Console.WriteLine("[!] Press ENTER to exit from the program.");
        Console.ReadLine();
    }

    private static string CorrectPath(string path)
    {
        while (path.StartsWith("\"") || path.StartsWith(" ") || path.StartsWith("'") || path.StartsWith("/") || path.StartsWith("\\"))
        {
            path = path.Substring(1);
        }

        while (path.EndsWith("\"") || path.EndsWith(" ") || path.EndsWith("'") || path.EndsWith("/") || path.EndsWith("\\"))
        {
            path = path.Substring(0, path.Length - 1);
        }

        return path;
    }

    private static void RunExecutable(string executableName, string arguments)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = executableName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
        }).WaitForExit();
    }
}
