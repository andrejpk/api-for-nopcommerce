#!/usr/bin/env dotnet-script
#r "nuget: System.IO.Compression, 4.3.0"

using System.IO.Compression;

// Configuration
var pluginName = "Nop.Plugin.Api";
var pluginProjectPath = Path.Combine("Nop.Plugin.Api");
var nopCommerceRoot = Environment.GetEnvironmentVariable("NopCommerceRoot")
    ?? Path.Combine("..", "nopCommerce", "src");
var pluginOutputPath = Path.Combine(nopCommerceRoot, "Presentation", "Nop.Web", "Plugins", pluginName);
var version = Environment.GetEnvironmentVariable("VERSION") ?? "0.0.0-dev";
var packageRootPath = "package";
var packageOutputPath = Path.Combine(packageRootPath, pluginName);
var zipFileName = $"Nop.Plugin.Api_v{version}.zip";

Console.WriteLine("📦 API Plugin Packager");
Console.WriteLine("===========================\n");

// Check if nopCommerce build output exists
if (!Directory.Exists(pluginOutputPath))
{
    Console.Error.WriteLine($"❌ Error: Plugin build output not found at {pluginOutputPath}");
    Console.Error.WriteLine("Please build the plugin first with: dotnet build");
    Environment.Exit(1);
}

// Clean and create package directory
Console.WriteLine("📦 Creating package...");
if (Directory.Exists(packageRootPath))
{
    Directory.Delete(packageRootPath, true);
}
Directory.CreateDirectory(packageOutputPath);

// Copy plugin DLL and PDB
Console.WriteLine("   Copying binaries...");
CopyIfExists(Path.Combine(pluginOutputPath, "Nop.Plugin.Api.dll"), packageOutputPath);
CopyIfExists(Path.Combine(pluginOutputPath, "Nop.Plugin.Api.pdb"), packageOutputPath);

// Copy all dependency DLLs (excluding nopCommerce core assemblies and framework assemblies)
Console.WriteLine("   Copying dependencies...");
// Exclude nopCommerce assemblies and .NET framework assemblies, but keep third-party packages
var excludedPrefixes = new[] 
{ 
    "Nop.Core", "Nop.Data", "Nop.Services", "Nop.Web",
    "System.", "netstandard", "mscorlib"
};

// More selective Microsoft exclusions - only exclude framework assemblies, not third-party packages
var excludedMicrosoftPrefixes = new[]
{
    "Microsoft.AspNetCore.App.Runtime",
    "Microsoft.CSharp",
    "Microsoft.Extensions.",
    "Microsoft.NETCore.",
    "Microsoft.TestPlatform",
    "Microsoft.VisualBasic",
    "Microsoft.Win32",
    "Microsoft.WindowsDesktop"
};

foreach (var file in Directory.GetFiles(pluginOutputPath, "*.dll"))
{
    var fileName = Path.GetFileName(file);
    
    // Skip the main plugin DLL
    if (fileName == "Nop.Plugin.Api.dll")
        continue;
        
    // Check general exclusions
    if (excludedPrefixes.Any(prefix => fileName.StartsWith(prefix)))
        continue;
        
    // Check Microsoft-specific exclusions (more selective)
    if (fileName.StartsWith("Microsoft.") && excludedMicrosoftPrefixes.Any(prefix => fileName.StartsWith(prefix)))
        continue;
    
    // Copy the dependency
    File.Copy(file, Path.Combine(packageOutputPath, fileName), true);
}

// Copy Areas (contains Admin views)
Console.WriteLine("   Copying Areas...");
var areasSource = Path.Combine(pluginProjectPath, "Areas");
var areasTarget = Path.Combine(packageOutputPath, "Areas");
if (Directory.Exists(areasSource))
{
    CopyDirectory(areasSource, areasTarget);
}

// Copy Views folder if it exists (for front-end views)
Console.WriteLine("   Copying Views...");
var viewsSource = Path.Combine(pluginProjectPath, "Views");
var viewsTarget = Path.Combine(packageOutputPath, "Views");
if (Directory.Exists(viewsSource))
{
    CopyDirectory(viewsSource, viewsTarget);
}

// Copy metadata files
Console.WriteLine("   Copying metadata...");
var pluginJsonSource = Path.Combine(pluginProjectPath, "plugin.json");
var pluginJsonTarget = Path.Combine(packageOutputPath, "plugin.json");
var pluginJsonContent = File.ReadAllText(pluginJsonSource);
pluginJsonContent = System.Text.RegularExpressions.Regex.Replace(
    pluginJsonContent,
    @"""Version""\s*:\s*""0\.0\.0""",
    $@"""Version"": ""{version}""");
File.WriteAllText(pluginJsonTarget, pluginJsonContent);
CopyIfExists(Path.Combine(pluginProjectPath, "logo.jpg"), packageOutputPath);
CopyIfExists(Path.Combine(pluginProjectPath, "logo.png"), packageOutputPath);

// Create uploadedItems.json
Console.WriteLine("   Creating uploadedItems.json...");
var uploadedItemsPath = Path.Combine(packageRootPath, "uploadedItems.json");
var uploadedItems = $$"""
[
  {
    "Type": "Plugin",
    "SupportedVersion": "4.90",
    "DirectoryPath": "{{pluginName}}/",
    "SystemName": "{{pluginName}}",
    "SourceDirectoryPath": "{{pluginName}}/"
  }
]
""";
File.WriteAllText(uploadedItemsPath, uploadedItems);

// Create ZIP
Console.WriteLine("\n📦 Creating ZIP file...");
if (File.Exists(zipFileName))
{
    File.Delete(zipFileName);
}

ZipFile.CreateFromDirectory(packageRootPath, zipFileName, CompressionLevel.Optimal, includeBaseDirectory: false);

var zipInfo = new FileInfo(zipFileName);
Console.WriteLine($"✅ Package created successfully!");
Console.WriteLine($"   File: {zipFileName}");
Console.WriteLine($"   Size: {zipInfo.Length:N0} bytes");
Console.WriteLine($"   Version: {version}");

Console.WriteLine("\nTo install:");
Console.WriteLine("  1. Go to Admin → Configuration → Local plugins");
Console.WriteLine("  2. Click 'Upload plugin or theme'");
Console.WriteLine("  3. Select Nop.Plugin.Api.zip");
Console.WriteLine("  4. Upload and restart the application");

// Helper functions
void CopyIfExists(string source, string destination)
{
    if (File.Exists(source))
    {
        var fileName = Path.GetFileName(source);
        var destPath = Directory.Exists(destination) ? Path.Combine(destination, fileName) : destination;
        File.Copy(source, destPath, true);
    }
}

void CopyDirectory(string sourceDir, string destDir)
{
    Directory.CreateDirectory(destDir);

    foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
    {
        var relativePath = Path.GetRelativePath(sourceDir, file);
        var destFile = Path.Combine(destDir, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
        File.Copy(file, destFile, true);
    }
}
