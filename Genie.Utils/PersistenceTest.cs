using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genie.Utils
{
    public class PersistenceTest : IPersistenceTestModel
    {
        public string Id { get; set; }
        public string Info { get; set; }
    }

    public interface IPersistenceTestModel
    {
        string Id { get; set; }
        string Info { get; set; }
    }
}
