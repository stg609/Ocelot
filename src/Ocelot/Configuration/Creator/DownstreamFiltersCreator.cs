using Ocelot.Configuration.File;
using System.Collections.Generic;
using System.Linq;

namespace Ocelot.Configuration.Creator
{
    public class DownstreamFiltersCreator : IDownstreamFiltersCreator
    {
        public List<DownstreamFilter> Create(FileReRoute reRoute)
        {
            return reRoute.DownstreamFilters.Select(itm => new DownstreamFilter(itm.Type, itm.Params)).ToList();
        }
    }
}
