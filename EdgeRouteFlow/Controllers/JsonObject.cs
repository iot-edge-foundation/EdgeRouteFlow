using System.Collections.Generic;

namespace EdgeRouteFlow.Controllers
{
    public class JsonObject
    {
        public Dictionary<string, Operator> operators { get; private set; } = new Dictionary<string, Operator>();
        public Dictionary<string, Link> links { get; private set; } = new Dictionary<string, Link>();
    }
}