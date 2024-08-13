
namespace Genie.Common.Utils.ChangeFeed;

public record ChangeLog
{
    public List<Difference> Common { get; set; } = [];

    public List<BasicDifference> Basic { get; set; } = [];
    public List<SetDifference> Set { get; set; } = [];
    public List<MissingEntryDifference> Missing { get; set; } = [];

    public void Add(Difference d)
    {
        switch(d)
        {
            case BasicDifference b:
                Basic.Add(b);
                break;
            case SetDifference s:
                Set.Add(s);
                break;
            case MissingEntryDifference m:
                Missing.Add(m);
                break;
            case Difference:
                Common.Add(d);
                break;
        }
    }
} 

public record Difference
{
    public string? Breadcrumb { get; set; }
}

public record BasicDifference :  Difference
{
    public string? Value1 { get; set; }
    public string? Value2 { get; set; }
    public string? ChildProperty { get; set; }
}

public record SetDifference : Difference
{
    public List<string> Expected { get; set; } = [];
    public List<string> Extra { get; set; } = [];
}


public record MissingEntryDifference : Difference
{
    public MissingSide Side { get; set; }
    public List<string> Expected { get; set; } = [];
    public List<string> Extra { get; set; } = [];
    public string? Key { get; set; }
    public string? Value { get; set; }
}


public enum MissingSide
{
    Actual = 0,
    Expected = 1
}