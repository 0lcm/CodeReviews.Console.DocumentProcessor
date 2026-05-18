namespace ECommerce.API.Models.Import_Models;

public class MissingSeedDataException : Exception
{
    public List<string> MissingDataCategories { get; }

    public MissingSeedDataException(List<string> missingDataCategories)
    : base($"Missing seed data for {string.Join(", ", missingDataCategories)}")
    {
        MissingDataCategories = missingDataCategories;
    }
}