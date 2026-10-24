namespace ContosoUniversity
{
    using System.Threading.Tasks;
    using ContosoUniversity.DAL;
    using ContosoUniversity.Infrastructure;
    using ContosoUniversity.Infrastructure.DataAccess;
    using ContosoUniversity.Infrastructure.Tags;
    using ContosoUniversity.Logging;
    using FluentValidation;
    using FluentValidation.AspNetCore;
    using HtmlTags.Conventions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Replaces Global.asax/Global.asax.cs (Application_Start), App_Start/* and the
    /// StructureMap DependencyResolution/* bootstrapper. Everything the old
    /// DefaultRegistry/CommandProcessingRegistry scanned is registered here explicitly
    /// (or supplied by the ASP.NET Core container).
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ---- MVC + global filters + model binders (was FilterConfig / DefaultRegistry scan) ----
            builder.Services.AddControllersWithViews(options =>
            {
                // Was Infrastructure/ControllerFactory.cs (DefaultControllerFactory.GetControllerType).
                options.Conventions.Add(new FeatureControllerNameConvention());

                // Was FilterConfig.RegisterGlobalFilters. HandleErrorAttribute is replaced by
                // UseExceptionHandler("/Home/Error") below.
                options.Filters.Add<ValidatorActionFilter>();
                options.Filters.Add<MvcTransactionFilter>();

                // Was scan.AddAllTypesOf(typeof(IModelBinderProvider)).
                options.ModelBinderProviders.Insert(0, new DepartmentModelBinderProvider());
                options.ModelBinderProviders.Insert(0, new InstructorModelBinderProvider());
            });

            // MvcTransactionFilter takes a SchoolContext, so Filters.Add<T>() needs it in DI.
            builder.Services.AddScoped<MvcTransactionFilter>();

            // ---- Feature-folder view locations (was ViewEngines.Engines.Clear()/Add(...)) ----
            builder.Services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Clear();
                options.ViewLocationExpanders.Add(new FeatureViewLocationExpander());
            });

            // ---- Data access (was For<SchoolContext>().Use(() => new SchoolContext("SchoolContext"))
            //      plus DbInterception.Add(...) in Application_Start) ----
            // The interceptors are SINGLETONS so their state (the transient-error counter, the
            // command stopwatch) is process-wide, matching EF6's DbInterception.Add(new ...) which
            // registered one instance for the lifetime of the application.
            builder.Services.AddSingleton<SchoolInterceptorLogging>();
            builder.Services.AddSingleton<SchoolInterceptorTransientErrors>();

            builder.Services.AddDbContext<SchoolContext>((sp, o) =>
            {
                // NO EnableRetryOnFailure: the deleted DAL/SchoolConfiguration.cs explicitly
                // installed EF6's DefaultExecutionStrategy (retry OFF), because the global
                // MvcTransactionFilter calls Database.BeginTransaction() on every action and a
                // retrying execution strategy rejects user-initiated transactions. EF Core's
                // default non-retrying strategy is the faithful equivalent.
                o.UseSqlServer(builder.Configuration.GetConnectionString("SchoolContext"));
                o.AddInterceptors(
                    sp.GetRequiredService<SchoolInterceptorLogging>(),
                    sp.GetRequiredService<SchoolInterceptorTransientErrors>());
            });

            // ---- Mapping (was AutoMapperInitializer.Initialize()/static Mapper) ----
            builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));

            // ---- Messaging (was CommandProcessingRegistry) ----
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

            // ---- Validation (was scan.AddAllTypesOf(typeof(IValidator<>)) +
            //      FluentValidationModelValidatorProvider + StructureMapValidatorFactory) ----
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddFluentValidationClientsideAdapters();
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();

            // ---- HtmlTags (was Infrastructure/Tags/HtmlTagRegistry) ----
            builder.Services.AddSingleton<HtmlConventionLibrary>(HtmlConventionLibraryFactory.Create());
            builder.Services.AddHttpContextAccessor();

            // ---- Application logging facade (was ContosoUniversity.Logging.Logger over Trace) ----
            builder.Services.AddSingleton<IAppLogger, Logger>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                // Replaces the global HandleErrorAttribute + Features/Shared/Error.cshtml.
                app.UseExceptionHandler("/Home/Error");
            }

            // No UseHttpsRedirection/UseHsts: the original application had no RequireHttpsAttribute
            // and no <httpRedirect>, so forcing HTTPS (and browser-pinning it via HSTS) would be a
            // new behavior rather than a migration of an existing one.
            app.UseStaticFiles();
            app.UseRouting();

            // No authentication or authorization middleware is registered, and none should be:
            // the original application had no auth whatsoever (no FormsAuthentication, no
            // [Authorize], no Membership/Roles, no global AuthorizeAttribute in FilterConfig),
            // so there is no scheme or policy to enforce and the middleware would be dead.
            // The only security filter to preserve is [ValidateAntiForgeryToken], which MVC
            // applies per-action on the 12 POST endpoints that carry it.

            // Replaces RouteConfig.RegisterRoutes (IgnoreRoute("{resource}.axd") has no Core equivalent).
            app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

            // Replaces the EF6 migrations seeder (Migrations/Configuration.Seed). In the original,
            // migrating and seeding was developer-initiated (AutomaticMigrationsEnabled = false,
            // run via Update-Database) and never happened in Application_Start, so it is gated to
            // Development here — or to an explicit "Seed:Enabled" configuration flag.
            if (app.Environment.IsDevelopment() ||
                builder.Configuration.GetValue<bool>("Seed:Enabled"))
            {
                using (var scope = app.Services.CreateScope())
                {
                    await DbSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<SchoolContext>());
                }
            }

            await app.RunAsync();
        }
    }
}
