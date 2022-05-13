using System.Collections.Generic;

namespace Alza.LinkComposer
{
    public class Invocation
    {
        public string ProjectName { get; set; }
        public string ControllerName { get; set; }
        public string MethodName { get; set; }
        public string MethodTemplate { get; set; }
        public string ControllerTemplate { get; set; }
        public IDictionary<string, string> RouteParameterValues { get; set; }
        public IDictionary<string, string> ControllerRouteParameterValues { get; set; }
        public IDictionary<string, object> ParameterValues { get; set; }
    }
}
