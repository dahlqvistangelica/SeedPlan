using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace SeedPlan.Client.UnitTests.Flows;

[TestClass]
public class Prio1StaticQaChecksTests
{
    [TestMethod]
    public void VersionChain_IsConsistentAcrossMainLayout_Settings_AndAppSettings()
    {
        var root = GetRepoRoot();
        var mainLayoutPath = Path.Combine(root, "SeedPlan.Client", "Layout", "MainLayout.razor");
        var settingsPath = Path.Combine(root, "SeedPlan.Client", "Pages", "Settings.razor");
        var appSettingsPath = Path.Combine(root, "SeedPlan.Client", "wwwroot", "appsettings.json");

        var mainLayout = File.ReadAllText(mainLayoutPath);
        var settings = File.ReadAllText(settingsPath);
        var appSettings = File.ReadAllText(appSettingsPath);

        Assert.IsTrue(mainLayout.Contains("CurrentAppVersion = \"1.5.1\""), "MainLayout app version should be 1.5.1.");
        Assert.IsTrue(settings.Contains("SeedPlan v.1.5.1"), "Settings page should show version 1.5.1.");

        using var json = JsonDocument.Parse(appSettings);
        var appVersion = json.RootElement.GetProperty("AppVersion").GetString();
        Assert.AreEqual("1.5.1", appVersion, "appsettings.json AppVersion should be 1.5.1.");
    }

    [TestMethod]
    public void ModeSwitch_AndModeSpecificBottomNav_AreConfigured()
    {
        var root = GetRepoRoot();
        var mainLayoutPath = Path.Combine(root, "SeedPlan.Client", "Layout", "MainLayout.razor");
        var bottomNavPath = Path.Combine(root, "SeedPlan.Client", "Layout", "BottomNav.razor");

        var mainLayout = File.ReadAllText(mainLayoutPath);
        var bottomNav = File.ReadAllText(bottomNavPath);

        Assert.IsTrue(mainLayout.Contains("localStorage.getItem", StringComparison.Ordinal), "MainLayout should read persisted mode from localStorage.");
        Assert.IsTrue(mainLayout.Contains("localStorage.setItem", StringComparison.Ordinal), "MainLayout should persist mode to localStorage.");
        Assert.IsTrue(mainLayout.Contains("appMode", StringComparison.Ordinal), "MainLayout should use the appMode key.");
        Assert.IsTrue(mainLayout.Contains("/dahliabox-home", StringComparison.Ordinal), "MainLayout should navigate to DahliaBox home for Dahlia mode.");

        Assert.IsTrue(bottomNav.Contains("AppMode.SeedPlan", StringComparison.Ordinal), "BottomNav should have a SeedPlan mode branch.");
        Assert.IsTrue(bottomNav.Contains("AppMode.DahliaBox", StringComparison.Ordinal), "BottomNav should have a DahliaBox mode branch.");
        Assert.IsTrue(bottomNav.Contains("href=\"seeds\"", StringComparison.Ordinal), "SeedPlan mode should include seeds navigation.");
        Assert.IsTrue(bottomNav.Contains("href=\"sowings\"", StringComparison.Ordinal), "SeedPlan mode should include sowings navigation.");
        Assert.IsTrue(bottomNav.Contains("href=\"guide\"", StringComparison.Ordinal), "SeedPlan mode should include guide navigation.");
        Assert.IsTrue(bottomNav.Contains("href=\"dahliabox-home\"", StringComparison.Ordinal), "Dahlia mode should include dashboard navigation.");
        Assert.IsTrue(bottomNav.Contains("href=\"tubers\"", StringComparison.Ordinal), "Dahlia mode should include tubers navigation.");
        Assert.IsTrue(bottomNav.Contains("href=\"dahlias\"", StringComparison.Ordinal), "Dahlia mode should include varieties navigation.");
    }

    private static string GetRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "SeedPlan.slnx")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test execution directory.");
    }
}
