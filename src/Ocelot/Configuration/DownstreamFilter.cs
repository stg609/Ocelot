namespace Ocelot.Configuration
{
    public class DownstreamFilter
    {
        public DownstreamFilter(string type, object[] parameters)
        {
            Type = type;
            Params = parameters;
        }

        public string Type { get; private set; }
        public object[] Params { get; private set; }
    }
}
