using System;
using System.Linq;
using ODataWebserver.Global;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ODataWebserver.Models;
using Microsoft.AspNetCore.Http;
using ODataWebserver.EfContext;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Builder;

namespace ODataWebserver.Webserver.Controllers
{
    /* Follow the comments in .Initialize
     */
    public static class ODataControllerRegisterer
    {
        public static Dictionary<Type, Dictionary<string, ApiAccess>> AccessRights { get; set; }

        public static void Initialize(ODataConventionModelBuilder b)
        {
            AccessRights = new Dictionary<Type, Dictionary<string, ApiAccess>>();
            var prj = ConfigHelper.Instance.Project;

            // If you add a new project: Add a new case here and define the method to call.
            // Further examples and comments in ODataControllerRegisterer.Dummy
            switch (prj)
            {
                case "Dummy": Dummy(b); break;
                default: throw new Exception($"Registerer {prj} not defined");
            }

            // Dont forget to create project specific controllers in Webserver.Controllers
            // as you can see in DummyODataControllers.cs
        }


        private static void Dummy(ODataConventionModelBuilder b)
        {
            // Get the specific api keys for the consumers of this project here.
            // An api keys refers a consumer application, wich consumes this odata service
            var dummyApiKey = ConfigHelper.Instance.ApiConsumers["dummy"];

            // add all entities with access rights now...

            // add the entity to the builder in this format
            b.EntitySet<Job>(nameof(DummyContext.Jobs));

            //then defince access rights for each api key
            AccessRights.Add(typeof(Job), new Dictionary<string, ApiAccess>
            {
                { dummyApiKey, ApiAccess.InsertRead }
            });


            b.EntitySet<HyperParameter>(nameof(DummyContext.HyperParameters));
            AccessRights.Add(typeof(HyperParameter), new Dictionary<string, ApiAccess>
            {
                { dummyApiKey, ApiAccess.Full }

            });

            b.EntitySet<JobResult>(nameof(DummyContext.JobResults));
            AccessRights.Add(typeof(JobResult), new Dictionary<string, ApiAccess>
            {
                { dummyApiKey, ApiAccess.Read }
            });

            b.EntitySet<ValueOverride>(nameof(DummyContext.ValuesToOverwrite));
            AccessRights.Add(typeof(ValueOverride), new Dictionary<string, ApiAccess>
            {
                { dummyApiKey, ApiAccess.Full }
            });
        }
    }
}