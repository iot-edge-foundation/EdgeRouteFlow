using System.Collections.Generic;

namespace EdgeRouteFlow.Controllers
{
    public class Properties
    {
        public string title { get; set; }

        public Dictionary<string, InputOutput> inputs { get; private set; } = new Dictionary<string, InputOutput>();

        public Dictionary<string, InputOutput> outputs { get; private set; } = new Dictionary<string, InputOutput>();
    }
}