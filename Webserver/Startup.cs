using System.Linq;
using ODataWebserver.Global;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using ODataWebserver.Models;
using System;
using Microsoft.AspNetCore.Identity;
using ODataWebserver.Webserver.Controllers;

namespace ODataWebserver.Webserver
{
    /* INSTRUCTIONS FOR ADDING A NEW PROJECT (Consumer context)
     * --------------------------------------------------------
     * - Add a new project specific context in EfContext.Contexts
     * - Declare the neccessary models there.
     * - Add a new case-Entry for the context in EfContext.EfContextRegisterer.Initialize
     * - Go to Webserver.Controllers and add a new project specific controller, which derives from ODataControllerBase
     * - Add a new case-Entry in Webserver.Controllers.ODataControllerRegisterer
     * - Therefore you must define a new Method to call there.
     * - In this method, register all accessable endpoints and configure the access rights per Entity and Consumer
     * - Go to Webserver/web.config and set the database connection and the project name!
     * 
     * 
     * 
     * CONFIG USAGE
     * ------------
     * - You must set the specific project in config.json
     * - You must also note there all possible api consumers ["Name:ApiKey",...]
     * - The API key can be generated as a GUID
     * 
     * 
     * 
     * INSTRUCTIONS FOR EXTENDING THE MODEL
     * ------------------------------------
     * - Go to the Models project and add a new class
     * - For the new class: 
     *      - Implement the interface "IModel"
     *      - Define all properties you need
     *      - Important: Value types (also foreign keys) must be nullable (e.g. int? instead of int)
     *      - Foreign keys are defined as integers (e.g. FooId) and also as a object reference (e.g. Foo), which is virtual
     *      - The foreign key id property needs an attribute as follows: [ForeignKey(nameof(Foo))]
     *      - Properties can be marked as [Required]
     *      - Child-objects are collections of the type "ICollection<Bar>" and must be declared as virtual, too.
     * - To update the database structure, follow the instructions for scheme update!
     * - Special definitions for model behaviours (e.g. on delete cascade constraints) can be defined in EfContext.Contexts (OnModelCreating method)
     * 
     * 
     * 
     * INSTRUCTIONS FOR SCHEME UPDATE
     * ----------------------------------
     * - Set Webserver as Startup Project (solution explorer => right click on the project => set as startup project)
     * - Check if the db config in Webserver/web.config is correct
     * - Open Package Manager Console in Visual Studio (menustrip => tools/nuget package manager)
     * - Command: "Add-Migration SchemeUpdate -Project EfContext"
     * - Then: "Update-Database"
     * - Delete the SchemeUpdate File in EfContext/Migrations. Not the ModelSnapshot!
     * 
     * 
     * 
     * INSTRUCTIONS FOR ADDING A CONTROLLER
     * ------------------------------------
     * - Extend the builder: Webserver.Controllers.ODataControllerRegisterer.Initialize
     * - Go to Webserver.Controllers and add a new File <Project>ODataControllers.cs
     * - Then: Define all controllers in this file <EntityName>Controller.
     * - After starting the Webserver application (e.g. via debugging), you can use the controller
     * - How to use the controller in detail, c.f. instructions in Webserver.ODataControllerBase
     */

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOData();
            services.AddMvc();
            services.AddMvcCore().AddJsonFormatters();
            EfContextRegisterer.Initialize(services);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMvc(b =>
            {
                b.Select().Expand().Filter().OrderBy().MaxTop(100).Count();
                b.MapODataServiceRoute("odata", "odata", GetEdmModel());
            });
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            ODataControllerRegisterer.Initialize(builder);
            return builder.GetEdmModel();
        }
    }
}
