using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genie.Utils
{
    public interface IPersistenceTest
    {
        public int Payload { get; set; }
        public bool Write(long i);

        public bool Read(long i);

        public Task<bool> WritePostal(CountryPostalCode message);
        public Task<bool> ReadPostal(CountryPostalCode message);

        public Task<bool> QueryPostal(CountryPostalCode message);
        public Task<bool> SelfJoinPostal(CountryPostalCode message);
    }
}
