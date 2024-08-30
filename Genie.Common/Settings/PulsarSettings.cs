using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genie.Common.Settings;

public class PulsarSettings(string connectionString)
{
    public string ConnectionString { get; set; } = connectionString;
}