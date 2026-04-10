using System.Globalization;

namespace SeedPlan.Shared.Helpers;

public static class SeedInventoryHelper
{
    public static IReadOnlyList<string> ParseTags(string? tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
        {
            return Array.Empty<string>();
        }

        return tags
            .Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(tag => tag.Trim())
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static string NormalizeTags(string? tags)
    {
        var parsedTags = ParseTags(tags);
        return string.Join(", ", parsedTags);
    }

    public static bool HasTag(string? tags, string selectedTag)
    {
        if (string.IsNullOrWhiteSpace(selectedTag))
        {
            return true;
        }

        return ParseTags(tags).Any(tag => string.Equals(tag, selectedTag, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsExpired(DateTime? expiryDate, DateTime? referenceDate = null)
    {
        if (!expiryDate.HasValue)
        {
            return false;
        }

        var today = (referenceDate ?? DateTime.Now).Date;
        return expiryDate.Value.Date < today;
    }

    public static bool IsExpiringSoon(DateTime? expiryDate, int quantity, DateTime? referenceDate = null)
    {
        if (!expiryDate.HasValue || quantity <= 0)
        {
            return false;
        }

        var today = (referenceDate ?? DateTime.Now).Date;
        var threshold = today.AddMonths(6);
        var expiry = expiryDate.Value.Date;

        return expiry >= today && expiry <= threshold;
    }

    public static bool IsOutOfStock(int quantity) => quantity <= 0;
}