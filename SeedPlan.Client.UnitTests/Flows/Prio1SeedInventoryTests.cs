using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeedPlan.Shared.Helpers;

namespace SeedPlan.Client.UnitTests.Flows;

[TestClass]
public class Prio1SeedInventoryTests
{
    [TestMethod]
    public void SeedInventoryHelper_ParseTags_TrimsAndDeduplicates()
    {
        var tags = SeedInventoryHelper.ParseTags("favorit, ekologisk; Ny\nfavorit,   premium");

        CollectionAssert.AreEqual(new[] { "favorit", "ekologisk", "Ny", "premium" }, tags.ToArray());
    }

    [TestMethod]
    public void SeedInventoryHelper_NormalizeTags_ReturnsCleanCommaSeparatedList()
    {
        var normalized = SeedInventoryHelper.NormalizeTags("favorit, ekologisk; favorit\nny");

        Assert.AreEqual("favorit, ekologisk, ny", normalized.ToLowerInvariant());
    }

    [TestMethod]
    public void SeedInventoryHelper_ExpiryWarnings_WorkAsExpected()
    {
        var today = DateTime.Today;

        Assert.IsTrue(SeedInventoryHelper.IsExpired(today.AddDays(-1), today));
        Assert.IsFalse(SeedInventoryHelper.IsExpired(today.AddDays(1), today));
        Assert.IsTrue(SeedInventoryHelper.IsExpiringSoon(today.AddMonths(6).AddDays(-1), 10, today));
        Assert.IsFalse(SeedInventoryHelper.IsExpiringSoon(today.AddMonths(6).AddDays(1), 10, today));
        Assert.IsFalse(SeedInventoryHelper.IsExpiringSoon(today.AddMonths(3), 0, today));
    }

    [TestMethod]
    public void SeedInventoryMigration_ContainsExpandedFieldsAndViewColumns()
    {
        var root = GetRepoRoot();
        var migrationPath = Path.Combine(root, "supabase", "migrations", "20260409113000_expand_seed_inventory.sql");

        var migration = File.ReadAllText(migrationPath);

        Assert.IsTrue(migration.Contains("purchase_date", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(migration.Contains("purchase_location", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(migration.Contains("germination_rate", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(migration.Contains("tags text", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(migration.Contains("v_user_inventory", StringComparison.OrdinalIgnoreCase));
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
