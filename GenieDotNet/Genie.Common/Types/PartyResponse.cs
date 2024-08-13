using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genie.Common.Types
{
    public record PartyResponse : BaseResponse
    {
        public Party? Party { get; set; }
    }
}
