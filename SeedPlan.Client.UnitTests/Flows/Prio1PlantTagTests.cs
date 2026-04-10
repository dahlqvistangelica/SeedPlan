using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeedPlan.Shared.Models;
using SeedPlan.Shared.Models.ViewModels;

namespace SeedPlan.Client.UnitTests.Flows;

[TestClass]
public class Prio1PlantTagTests
{
    [TestMethod]
    public void PlantModel_ExposesTagCollection()
    {
        var tagProperty = typeof(Plant).GetProperty(nameof(Plant.Tags));

        Assert.IsNotNull(tagProperty);
        Assert.AreEqual(typeof(List<PlantTag>), tagProperty!.PropertyType);
    }

    [TestMethod]
    public void SeedView_ExposesPlantTagsColumn()
    {
        var tagProperty = typeof(SeedView).GetProperty(nameof(SeedView.PlantTags));

        Assert.IsNotNull(tagProperty);
        Assert.AreEqual(typeof(string), tagProperty!.PropertyType);
    }

    [TestMethod]
    public void PlantTagsMigration_ContainsGlobalTagTablesAndAggregatedInventoryView()
    {
        var root = GetRepoRoot();
        var migrationPath = Path.Combine(root, "supabase", "migrations", "20260409153000_plant_tags.sql");

        var migration = File.ReadAllText(migrationPath);

        Assert.IsTrue(migration.Contains("create table if not exists public.tags", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(migration.Contains("create table if not exists public.plant_tags", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(migration.Contains("plant_tags", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(migration.Contains("string_agg(tg.display_name", StringComparison.OrdinalIgnoreCase));
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