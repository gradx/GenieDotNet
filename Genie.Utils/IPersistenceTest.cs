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
        public void Write(int i);

        public void Read(int i);
    }
}
