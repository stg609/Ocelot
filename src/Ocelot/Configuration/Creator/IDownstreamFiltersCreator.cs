using Ocelot.Configuration.File;
using System.Collections.Generic;

namespace Ocelot.Configuration.Creator
{
    public interface IDownstreamFiltersCreator
    {
        List<DownstreamFilter> Create(FileReRoute reRoute);
    }
}
