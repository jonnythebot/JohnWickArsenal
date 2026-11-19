using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Dependency detection and installation guide for John Wick Arsenal mod development.
/// Detects Unity, .NET SDK, Android SDK, and provides platform-specific installation instructions.
/// </summary>
public class JWA_DependencyChecker : EditorWindow
{
    private Vector2 scrollPos;
    private DependencyReport lastReport;

    [MenuItem("John Wick Arsenal/Dependency Checker", priority = 5)]
    public static void ShowWindow()
    {
        var window = GetWindow<JWA_DependencyChecker>("JWA Dependency Checker");
        window.minSize = new Vector2(600, 700);
        window.Show();
    }

    private void OnEnable()
    {
        RunDependencyCheck();
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        DrawHeader();
        DrawSystemInfo();
        DrawDependencyStatus();
        DrawInstallationGuides();
        DrawRemoteBuildOptions();

        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("John Wick Arsenal - Dependency Checker", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Detects required tools and provides installation instructions", EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.Space(5);

        if (GUILayout.Button("🔄 Re-check Dependencies"))
        {
            RunDependencyCheck();
        }

        EditorGUILayout.Space(5);
    }

    private void DrawSystemInfo()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("System Information", EditorStyles.boldLabel);

        EditorGUILayout.LabelField($"OS: {SystemInfo.operatingSystem}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Unity: {Application.unityVersion}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"Platform: {Application.platform}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"CPU: {SystemInfo.processorType} ({SystemInfo.processorCount} cores)", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"RAM: {SystemInfo.systemMemorySize} MB", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"GPU: {SystemInfo.graphicsDeviceName}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"VRAM: {SystemInfo.graphicsMemorySize} MB", EditorStyles.miniLabel);

        // Check if running on ChromeOS
        bool isChromeOS = SystemInfo.operatingSystem.Contains("Chrome") ||
                          SystemInfo.operatingSystem.Contains("CrOS") ||
                          Environment.GetEnvironmentVariable("CROSTINI") != null;
        if (isChromeOS)
        {
            EditorGUILayout.HelpBox("✓ ChromeOS/Crostini detected - using Linux build path", MessageType.Info);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawDependencyStatus()
    {
        if (lastReport == null) return;

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Dependency Status", EditorStyles.boldLabel);

        // Required dependencies
        DrawDependencySection("Required", lastReport.Required, true);

        // Optional dependencies
        DrawDependencySection("Optional (Recommended)", lastReport.Optional, false);

        EditorGUILayout.EndVertical();
    }

    private void DrawDependencySection(string title, List<DependencyInfo> deps, bool required)
    {
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

        foreach (var dep in deps)
        {
            var color = dep.Found ? Color.green : (required ? Color.red : Color.yellow);
            var status = dep.Found ? "✅ FOUND" : (required ? "❌ MISSING" : "⚠ NOT FOUND");
            var style = new GUIStyle(EditorStyles.label) { normal = { textColor = color } };

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{dep.Name} {status}", style, GUILayout.Width(300));
            if (!string.IsNullOrEmpty(dep.Version))
            {
                EditorGUILayout.LabelField($"v{dep.Version}", EditorStyles.miniLabel, GUILayout.Width(80));
            }
            if (!dep.Found && GUILayout.Button("Guide", GUILayout.Width(60)))
            {
                ShowInstallGuide(dep);
            }
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(dep.Path))
            {
                EditorGUILayout.LabelField($"  Path: {dep.Path}", EditorStyles.miniLabel);
            }
        }
    }

    private void DrawInstallationGuides()
    {
        if (lastReport == null) return;

        var missingRequired = lastReport.Required.Where(d => !d.Found).ToList();
        var missingOptional = lastReport.Optional.Where(d => !d.Found).ToList();

        if (missingRequired.Count == 0 && missingOptional.Count == 0)
        {
            EditorGUILayout.HelpBox("✅ All dependencies satisfied!", MessageType.Info);
            return;
        }

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Installation Guides", EditorStyles.boldLabel);

        foreach (var dep in missingRequired.Concat(missingOptional))
        {
            if (GUILayout.Button($"📖 {dep.Name} Installation Guide"))
            {
                ShowInstallGuide(dep);
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawRemoteBuildOptions()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Remote Build Options (for Chromebook/Low-spec)", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("If local build is not feasible, use:", EditorStyles.wordWrappedMiniLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("GitHub Actions"))
        {
            OpenURL("https://github.com/features/actions");
        }
        if (GUILayout.Button("GitLab CI"))
        {
            OpenURL("https://docs.gitlab.com/ee/ci/");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Unity Cloud Build"))
        {
            OpenURL("https://build.cloud.unity3d.com/");
        }
        if (GUILayout.Button("Bitrise"))
        {
            OpenURL("https://www.bitrise.io/");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Required secrets for CI/CD:", EditorStyles.boldLabel);
        EditorGUILayout.SelectableLabel("UNITY_LICENSE: Your Unity license (Personal/Pro)", EditorStyles.textField, GUILayout.Height(18));
        EditorGUILayout.SelectableLabel("ANDROID_KEYSTORE: Base64 encoded keystore", EditorStyles.textField, GUILayout.Height(18));
        EditorGUILayout.SelectableLabel("KEYSTORE_PASS / KEY_ALIAS / KEY_PASS: Keystore credentials", EditorStyles.textField, GUILayout.Height(18));

        EditorGUILayout.EndVertical();
    }

    // ========================================================================
    // DEPENDENCY CHECKING
    // ========================================================================

    private void RunDependencyCheck()
    {
        lastReport = new DependencyReport
        {
            Timestamp = DateTime.Now,
            UnityVersion = Application.unityVersion,
            Platform = Application.platform.ToString()
        };

        // Check Unity (already running)
        lastReport.Required.Add(new DependencyInfo
        {
            Name = "Unity Editor",
            Required = true,
            Found = true,
            Version = Application.unityVersion,
            Path = EditorApplication.applicationPath,
            InstallGuide = "Already running!"
        });

        // Check .NET SDK
        CheckDotnet();

        // Check Android SDK
        CheckAndroidSDK();

        // Check Git
        CheckGit();

        // Check Zip
        CheckZip();

        // Check ADB
        CheckADB();

        // Check ChromeOS specifics
        CheckChromeOS();

        Repaint();
    }

    private void CheckDotnet()
    {
        var dep = new DependencyInfo
        {
            Name = ".NET SDK 8.0+",
            Required = true,
            InstallGuide = GetDotnetInstallGuide()
        };

        // Try dotnet in PATH
        string output = RunCommand("dotnet", "--version");
        if (!string.IsNullOrEmpty(output))
        {
            dep.Found = true;
            dep.Version = output.Trim();
            dep.Path = RunCommand("which", "dotnet")?.Trim() ?? RunCommand("where", "dotnet")?.Trim();
        }
        else
        {
            // Check common locations
            string[] paths = {
                "/usr/share/dotnet/dotnet",
                "/opt/dotnet/dotnet",
                Environment.ExpandEnvironmentVariables("%ProgramFiles%\\dotnet\\dotnet.exe"),
                Environment.ExpandEnvironmentVariables("%USERPROFILE%\\.dotnet\\dotnet.exe"),
                "/snap/dotnet-sdk/current/dotnet"
            };

            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    dep.Found = true;
                    dep.Path = path;
                    dep.Version = RunCommand(path, "--version")?.Trim();
                    break;
                }
            }
        }

        lastReport.Required.Add(dep);
    }

    private void CheckAndroidSDK()
    {
        var dep = new DependencyInfo
        {
            Name = "Android SDK",
            Required = false, // Optional - Unity has built-in
            InstallGuide = GetAndroidSDKInstallGuide()
        };

        string androidHome = Environment.GetEnvironmentVariable("ANDROID_HOME") ??
                            Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");

        if (!string.IsNullOrEmpty(androidHome) && Directory.Exists(androidHome))
        {
            dep.Found = true;
            dep.Path = androidHome;

            // Check for platform-tools
            string adbPath = Path.Combine(androidHome, "platform-tools", "adb");
            if (File.Exists(adbPath) || File.Exists(adbPath + ".exe"))
            {
                dep.Version = RunCommand(adbPath, "version")?.Split('\n')[0]?.Trim();
            }
        }
        else
        {
            // Check common locations
            string[] paths = {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Android", "Sdk"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Android", "sdk"),
                "/opt/android-sdk",
                "/usr/lib/android-sdk",
                "/android-sdk"
            };

            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    dep.Found = true;
                    dep.Path = path;
                    break;
                }
            }
        }

        lastReport.Optional.Add(dep);
    }

    private void CheckGit()
    {
        var dep = new DependencyInfo
        {
            Name = "Git",
            Required = false,
            InstallGuide = "Install via: apt install git / brew install git / winget install Git.Git"
        };

        string output = RunCommand("git", "--version");
        if (!string.IsNullOrEmpty(output))
        {
            dep.Found = true;
            dep.Version = output.Replace("git version ", "").Trim();
            dep.Path = RunCommand("which", "git")?.Trim() ?? RunCommand("where", "git")?.Trim();
        }

        lastReport.Optional.Add(dep);
    }

    private void CheckZip()
    {
        var dep = new DependencyInfo
        {
            Name = "zip (packaging)",
            Required = true,
            InstallGuide = "Install via: apt install zip / brew install zip / winget install zip"
        };

        string output = RunCommand("zip", "-v");
        if (!string.IsNullOrEmpty(output))
        {
            dep.Found = true;
            dep.Version = output.Split('\n')[0].Trim();
        }

        lastReport.Required.Add(dep);
    }

    private void CheckADB()
    {
        var dep = new DependencyInfo
        {
            Name = "ADB (device deployment)",
            Required = false,
            InstallGuide = "Part of Android SDK platform-tools. Or: apt install android-tools-adb"
        };

        string output = RunCommand("adb", "version");
        if (!string.IsNullOrEmpty(output))
        {
            dep.Found = true;
            dep.Version = output.Split('\n')[0].Trim();
        }

        lastReport.Optional.Add(dep);
    }

    private void CheckChromeOS()
    {
        bool isChromeOS = SystemInfo.operatingSystem.Contains("Chrome") ||
                          SystemInfo.operatingSystem.Contains("CrOS") ||
                          Environment.GetEnvironmentVariable("CROSTINI") != null ||
                          File.Exists("/etc/os-release") && File.ReadAllText("/etc/os-release").Contains("chromiumos");

        var dep = new DependencyInfo
        {
            Name = "ChromeOS Linux (Crostini)",
            Required = false,
            Found = isChromeOS,
            InstallGuide = "Enable Linux development environment in ChromeOS Settings > Advanced > Developers"
        };

        if (isChromeOS)
        {
            dep.Version = "Crostini";
            dep.Path = "/mnt/chromeos";

            // Check for storage constraints
            try
            {
                var drive = new DriveInfo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                double freeGB = drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
                if (freeGB < 10)
                {
                    lastReport.Warnings.Add($"Low disk space: {freeGB:F1} GB free. Consider cleaning or using remote build.");
                }
            }
            catch { }
        }

        lastReport.Optional.Add(dep);
    }

    private string RunCommand(string command, string args)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = command,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                process.WaitForExit(5000);
                if (process.ExitCode == 0)
                    return process.StandardOutput.ReadToEnd();
            }
        }
        catch { }
        return null;
    }

    private void ShowInstallGuide(DependencyInfo dep)
    {
        var content = new StringBuilder();
        content.AppendLine($"INSTALLATION GUIDE: {dep.Name}");
        content.AppendLine(new string('=', 50));
        content.AppendLine("");
        content.AppendLine(dep.InstallGuide);
        content.AppendLine("");

        if (dep.Name.Contains(".NET"))
        {
            content.AppendLine("LINUX (ChromeOS/Ubuntu/Debian):");
            content.AppendLine("  wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb");
            content.AppendLine("  sudo dpkg -i packages-microsoft-prod.deb");
            content.AppendLine("  sudo apt update && sudo apt install -y dotnet-sdk-8.0");
            content.AppendLine("");
            content.AppendLine("MACOS:");
            content.AppendLine("  brew install dotnet-sdk");
            content.AppendLine("");
            content.AppendLine("WINDOWS:");
            content.AppendLine("  winget install Microsoft.DotNet.SDK.8");
        }
        else if (dep.Name.Contains("Android"))
        {
            content.AppendLine("OPTION 1: Android Studio (recommended)");
            content.AppendLine("  Download: https://developer.android.com/studio");
            content.AppendLine("  Install with default settings (includes SDK, platform-tools, emulator)");
            content.AppendLine("");
            content.AppendLine("OPTION 2: Command Line Tools Only");
            content.AppendLine("  Download: https://developer.android.com/studio#command-tools");
            content.AppendLine("  Extract to ~/Android/Sdk");
            content.AppendLine("  Run: ~/Android/Sdk/cmdline-tools/latest/bin/sdkmanager \"platform-tools\" \"platforms;android-33\" \"build-tools;33.0.0\"");
            content.AppendLine("");
            content.AppendLine("SET ENVIRONMENT VARIABLES:");
            content.AppendLine("  export ANDROID_HOME=$HOME/Android/Sdk");
            content.AppendLine("  export PATH=$PATH:$ANDROID_HOME/platform-tools:$ANDROID_HOME/cmdline-tools/latest/bin");
            content.AppendLine("  Add to ~/.bashrc or ~/.zshrc");
        }
        else if (dep.Name.Contains("Git"))
        {
            content.AppendLine("LINUX: sudo apt install git");
            content.AppendLine("MACOS: brew install git");
            content.AppendLine("WINDOWS: winget install Git.Git");
        }
        else if (dep.Name.Contains("zip"))
        {
            content.AppendLine("LINUX: sudo apt install zip");
            content.AppendLine("MACOS: brew install zip");
            content.AppendLine("WINDOWS: winget install zip");
        }

        EditorUtility.DisplayDialog($"{dep.Name} Installation Guide", content.ToString(), "OK");
    }

    private string GetDotnetInstallGuide()
    {
        return @".NET SDK 8.0+ Installation:

LINUX (ChromeOS Crostini / Ubuntu / Debian):
  wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
  sudo dpkg -i packages-microsoft-prod.deb
  sudo apt update && sudo apt install -y dotnet-sdk-8.0

  # Verify: dotnet --version

MACOS:
  brew install dotnet-sdk

WINDOWS:
  winget install Microsoft.DotNet.SDK.8
  # Or download from: https://dotnet.microsoft.com/download/dotnet/8.0

CHROMEOS SPECIFIC:
  1. Enable Linux development environment: Settings > Advanced > Developers > Linux development environment
  2. Open Terminal app
  3. Run the Linux commands above

NOTE: Unity 2022.3 requires .NET Standard 2.1, which needs .NET SDK 6.0+.
      .NET 8.0 LTS is recommended for long-term support.";
    }

    private string GetAndroidSDKInstallGuide()
    {
        return @"Android SDK Installation:

OPTION 1: Android Studio (Recommended - includes everything)
  1. Download: https://developer.android.com/studio
  2. Run installer with default options
  3. SDK will be at: ~/Android/Sdk (Linux/Mac) or %USERPROFILE%\AppData\Local\Android\Sdk (Windows)

OPTION 2: Command Line Tools Only (Minimal)
  1. Download: https://developer.android.com/studio#command-tools
  2. Extract to ~/Android/Sdk/cmdline-tools/latest/
  3. Run:
     ~/Android/Sdk/cmdline-tools/latest/bin/sdkmanager --licenses
     ~/Android/Sdk/cmdline-tools/latest/bin/sdkmanager \"platform-tools\" \"platforms;android-33\" \"build-tools;33.0.0\"

REQUIRED PACKAGES:
  - platform-tools (adb, fastboot)
  - platforms;android-33 (or 29 for Quest 1/2 compatibility)
  - build-tools;33.0.0
  - ndk;25.1.8937393 (for IL2CPP)

ENVIRONMENT VARIABLES (add to ~/.bashrc, ~/.zshrc, or Windows Environment Variables):
  export ANDROID_HOME=$HOME/Android/Sdk
  export ANDROID_SDK_ROOT=$HOME/Android/Sdk
  export PATH=$PATH:$ANDROID_HOME/platform-tools:$ANDROID_HOME/cmdline-tools/latest/bin

UNITY INTEGRATION:
  Unity can use its own Android SDK (Preferences > External Tools > Android SDK).
  Point it to your Android SDK path for consistency.

CHROMEOS NOTE:
  Android Studio runs well on Crostini but needs >4GB RAM allocated.
  Consider Command Line Tools only if RAM is limited.";
    }

    private void OpenURL(string url)
    {
        Application.OpenURL(url);
    }

    // ========================================================================
    // DATA CLASSES
    // ========================================================================

    private class DependencyReport
    {
        public DateTime Timestamp;
        public string UnityVersion;
        public string Platform;
        public List<DependencyInfo> Required = new List<DependencyInfo>();
        public List<DependencyInfo> Optional = new List<DependencyInfo>();
        public List<string> Warnings = new List<string>();
    }

    private class DependencyInfo
    {
        public string Name;
        public bool Required;
        public bool Found;
        public string Version;
        public string Path;
        public string InstallGuide;
    }
}