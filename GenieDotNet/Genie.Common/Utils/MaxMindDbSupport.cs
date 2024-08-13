using DuckDB.NET.Data;
using MaxMind.Db;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genie.Common.Utils;

public class MaxMindDbSupport
{
    private static readonly Lazy<Reader> lazy = new(() => Create());

    private MaxMindDbSupport() { }

    public static Reader Instance => lazy.Value;

    private static Reader Create()
    {
        return new Reader(@"C:\Users\gradx\repos\GenieDotNet\SharedFiles\GeoLite2-City.mmdb");
    }

}