using ODataWebserver.Global;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ODataWebserver.Webserver
{
    public static class EfContextRegisterer
    {
        public static void Initialize(IServiceCollection services)
        {
            // Just extend a new case for the project context here!
            switch (ConfigHelper.Instance.Project)
            {
                case "Dummy": RegisterContext<DummyContext>(services); break;
                default: throw new Exception($"Registerer {ConfigHelper.Instance.Project} not defined");
            }
        }

        private static void RegisterContext<TContext>(IServiceCollection services)
            where TContext: DbContext
        {
            var cfg = ConfigHelper.Instance;
            var dbConnString = $"Server={cfg.DbHost};" +
                                $"Database={cfg.DbName};" +
                                $"user={cfg.DbUser};" +
                                $"password={cfg.DbPassword};";

            services.AddDbContextPool<TContext>(options => options.UseMySql(
                dbConnString,
                mysqlOptions =>
                    {
                        mysqlOptions.MigrationsAssembly("EfContext");
                    }
                ));
        }
    }
}