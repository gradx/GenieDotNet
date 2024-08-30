
namespace Genie.Common.Types;

public record Schedule
{
    public bool Sunday { get; set; }
    public bool Monday { get; set; }
    public bool Tuesday { get; set; }
    public bool Wednesday { get; set; }
    public bool Thursday { get; set; }
    public bool Friday { get; set; }
    public bool Saturday { get; set; }

    public DateTime? BeginTime { get; set; }
    public DateTime? EndTime { get; set; }

}