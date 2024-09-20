using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace Genie.Common.Performance;

public class ElasticGeo
{
    public string Id { get; set; }
    public string testshape { get; set; }
    public string StateCode { get; set; }
    public string StateName { get; set; }
    public string CountyCode { get; set; }
    public string CountyName { get; set; }
    public string Zcta5Code { get; set; }
    public string Zcta5Name { get; set; }
    public string Zcta5Type { get; set; }
    public string Zcta5AreaCode { get; set; }
    public int? Zcta5NameLong { get; set; }

}




