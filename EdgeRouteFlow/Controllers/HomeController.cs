using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Devices.Shared;

namespace EdgeRouteFlow.Controllers
{
    public class Route
    {
        public string Id { get; set; }
        public string ModuleFrom { get; set; }
        public string ModuleTo { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }
    }

    public class Module
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int Top { get; set; }
        public int Left { get; set; }
        public List<string> Inputs { get; private set; } = new List<string>();
        public List<string> Outputs { get; private set; } = new List<string>();
    }

    public class JsonObject
    {
        public Dictionary<string, Operator> operators { get; private set; } = new Dictionary<string, Operator>();
        public Dictionary<string, Link> links { get; private set; } = new Dictionary<string, Link>();
    }

    public class Operator
    {
        public Properties properties { get; private set; } = new Properties();
        public int top { get; set; }
        public int left { get; set; }
    }

    public class Properties
    {
        public string title { get; set; }

        public Dictionary<string, InputOutput> inputs { get; private set; } = new Dictionary<string, InputOutput>();

        public Dictionary<string, InputOutput> outputs { get; private set; } = new Dictionary<string, InputOutput>();
    }

    public class InputOutput
    {
        public string label { get; set; }
    }

    public class Link
    {
        public string fromOperator { get; set; }
        public string toOperator { get; set; }
        public string fromConnector { get; set; }
        public string toConnector { get; set; }
    }

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View("Index", null);
        }

        [HttpPost]
        public IActionResult FlowRoutes(FlowData a)
        {
            try
            {
                var jsonRoutes = JObject.Parse(a.routes);

                var jsonRoutesList = jsonRoutes["routes"].ToList<JToken>();

                var newJObject = new JObject();

                foreach (var attribute in jsonRoutesList)
                {
                    newJObject.Add(attribute);
                }

                var twinCollection = new TwinCollection(newJObject, new JObject());

                dynamic routes = twinCollection;

                return CreatedAtRoute(routes);
            }
            catch (Exception)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return new JsonResult(null);
            }
        }

        [HttpPost]
        public IActionResult Flow(FlowData a)
        {
            try
            {
                var connectionString = a.cs;

                var registryManager = RegistryManager.CreateFromConnectionString(connectionString);

                var twin = registryManager.GetTwinAsync(a.dn, "$edgeHub").Result;

                var desired = twin.Properties.Desired;

                var routes = desired["routes"];

                return CreatedAtRoute(routes);
            }
            catch (Exception)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return new JsonResult(null);
            }
        }

        private static JsonResult CreatedAtRoute(dynamic routes)
        {
            List<Route> routeList = ExtractEdgeHubRoutes(routes);

            List<Module> moduleList = ExtractRoutes(routeList);

            var jsonObject = ConstructFlowChart(routeList, moduleList);

            return new JsonResult(jsonObject);
        }

        private static List<Route> ExtractEdgeHubRoutes(dynamic routes)
        {
            var routeList = new List<Route>();

            foreach (var r in routes)
            {
                var key = r.Key;
                string value = r.Value;
                var regex1 = new Regex(@"modules/(\w+)[/outputs]{0,}/(.+?|\*)\b.*INTO.*modules/(\w+)/inputs/(.+?|\*)""", RegexOptions.IgnoreCase);

                var match1 = regex1.Match(value);

                if (match1.Success
                        && match1.Groups.Count > 4)
                {
                    routeList.Add(
                        new Route
                        {
                            Id = r.Key,
                            ModuleFrom = match1.Groups[1].Value,
                            Output = match1.Groups[2].Value,
                            ModuleTo = match1.Groups[3].Value,
                            Input = match1.Groups[4].Value,
                        });
                }

                var regex2 = new Regex(@"modules/(\w+)[/outputs]{0,}/(.+?|\*)\b.*INTO.*(\$upstream)", RegexOptions.IgnoreCase);

                var match2 = regex2.Match(value);

                if (match2.Success
                        && match2.Groups.Count > 3)
                {
                    routeList.Add(
                        new Route
                        {
                            Id = r.Key,
                            ModuleFrom = match2.Groups[1].Value,
                            Output = match2.Groups[2].Value,
                            ModuleTo = match2.Groups[3].Value,
                            Input = "upstream",
                        });
                }
            }

            return routeList;
        }

        private static List<Module> ExtractRoutes(List<Route> routeList)
        {
            var modules = new List<Module>();

            var topIndex = 0;
            var leftIndex = 0;

            foreach (var route in routeList)
            {
                var moduleFrom = modules.FirstOrDefault(x => x.Title == route.ModuleFrom);
                if (moduleFrom == null)
                {
                    moduleFrom =
                        new Module
                        {
                            Id = Convert.ToString(route.ModuleFrom),
                            Title = Convert.ToString(route.ModuleFrom),
                            Top = topIndex,
                            Left = leftIndex,
                        };

                    modules.Add(moduleFrom);

                    topIndex = topIndex + 100;
                    leftIndex = leftIndex + 100;
                }

                var moduleTo = modules.FirstOrDefault(x => x.Title == route.ModuleTo);
                if (moduleTo == null)
                {
                    moduleTo =
                        new Module
                        {
                            Id = Convert.ToString(route.ModuleTo),
                            Title = Convert.ToString(route.ModuleTo),
                            Top = topIndex,
                            Left = leftIndex,
                        };

                    modules.Add(moduleTo);

                    topIndex = topIndex + 100;
                    leftIndex = leftIndex + 100;
                }

                if (!moduleFrom.Outputs.Any(x => x == Convert.ToString(route.Output)))
                {
                    moduleFrom.Outputs.Add(Convert.ToString(route.Output));
                }

                if (!moduleTo.Inputs.Any(x => x == Convert.ToString(route.Input)))
                {
                    moduleTo.Inputs.Add(Convert.ToString(route.Input));
                }
            }

            return modules;
        }

        private static JsonObject ConstructFlowChart(List<Route> routeList, List<Module> moduleList)
        {
            var jsonObject = new JsonObject();

            foreach (var route in routeList)
            {
                jsonObject.links.Add(
                    route.Id,
                    new Link
                    {
                        fromOperator = route.ModuleFrom,
                        fromConnector = route.Output,
                        toOperator = route.ModuleTo,
                        toConnector = route.Input,
                    });
            }

            foreach (var module in moduleList)
            {
                var o = new Operator();

                jsonObject.operators.Add(module.Title, o);

                o.top = module.Top;
                o.left = module.Left;

                o.properties.title = module.Title;

                foreach (var output in module.Outputs)
                {
                    o.properties.outputs.Add(output, new InputOutput { label = output });
                }

                foreach (var input in module.Inputs)
                {
                    o.properties.inputs.Add(input, new InputOutput { label = input });
                }
            }

            return jsonObject;
        }
    }

    public class FlowData
    {
        public string cs { get; set; }
        public string dn { get; set; }
        public string routes { get; set; }
    }
}