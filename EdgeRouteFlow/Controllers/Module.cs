using System.Collections.Generic;

namespace EdgeRouteFlow.Controllers
{
    public class Module
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int Top { get; set; }
        public int Left { get; set; }
        public List<string> Inputs { get; private set; } = new List<string>();
        public List<string> Outputs { get; private set; } = new List<string>();
    }
}