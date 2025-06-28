namespace DRMA.Shared.Core.Models;

public abstract class LogDocument : CosmosDocument
{
    protected LogDocument(string id) : base(id)
    {
    }
}