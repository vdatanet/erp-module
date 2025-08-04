using System.Text;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.ExpressApp.MultiTenancy;
using DevExpress.ExpressApp.ReportsV2;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.Authentication.ClientServer;
using DevExpress.ExpressApp.WebApi.Services;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using erp.Blazor.Server.Services;
using erp.Module;
using erp.Module.BusinessObjects;
using erp.Module.BusinessObjects.Common;
using erp.Module.BusinessObjects.Contacts;
using erp.Module.BusinessObjects.Crm;
using erp.Module.BusinessObjects.Products;
using erp.Module.BusinessObjects.Sales;
using erp.WebApi.JWT;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData;
using Microsoft.OpenApi.Models;
using Country = erp.Module.BusinessObjects.Common.Country;
using State = erp.Module.BusinessObjects.Common.State;
using Task = erp.Module.BusinessObjects.Common.Task;

namespace erp.Blazor.Server;

public class Startup(IConfiguration configuration)
{
    private IConfiguration Configuration { get; } = configuration;

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(typeof(HubConnectionHandler<>), typeof(ProxyHubConnectionHandler<>));

        services.AddRazorPages();
        services.AddServerSideBlazor();
        services.AddHttpContextAccessor();
        services.AddScoped<IAuthenticationTokenProvider, JwtTokenProviderService>();
        services.AddScoped<CircuitHandler, CircuitHandlerProxy>();
        services.AddXaf(Configuration, builder =>
        {
            builder.UseApplication<erpBlazorApplication>();

            builder.AddXafWebApi(webApiBuilder =>
            {
                webApiBuilder.AddXpoServices();

                webApiBuilder.ConfigureOptions(options =>
                {
                    options.BusinessObject<Country>();
                    options.BusinessObject<State>();
                    options.BusinessObject<City>();
                    options.BusinessObject<Attachment>();
                    options.BusinessObject<Picture>();
                    options.BusinessObject<Task>();
                    
                    options.BusinessObject<Contact>();
                    options.BusinessObject<CompanyInfo>();
                    options.BusinessObject<Partner>();
                    options.BusinessObject<Customer>();
                    options.BusinessObject<Vendor>();
                    options.BusinessObject<Employee>();

                    options.BusinessObject<Lead>();

                    options.BusinessObject<Product>();
                    options.BusinessObject<Category>();

                    options.BusinessObject<SalesOrder>();
                    options.BusinessObject<OrderLine>();
                });
            });

            builder.Modules
                .AddCloning()
                .AddConditionalAppearance()
                .AddDashboards(options => { options.DashboardDataType = typeof(DashboardData); })
                .AddFileAttachments()
                .AddNotifications()
                .AddOffice()
                .AddReports(options =>
                {
                    options.EnableInplaceReports = true;
                    options.ReportDataType = typeof(ReportDataV2);
                    options.ReportStoreMode = ReportStoreModes.XML;
                })
                .AddScheduler()
                .AddValidation(options => { options.AllowValidationDetailsAccess = false; })
                .AddViewVariants()
                .Add<erpModule>()
                .Add<erpBlazorModule>();
            builder.AddMultiTenancy()
                .WithHostDatabaseConnectionString(Configuration.GetConnectionString("ConnectionString"))
#if EASYTEST
                    .WithHostDatabaseConnectionString(Configuration.GetConnectionString("EasyTestConnectionString"))
#endif
                .WithMultiTenancyModelDifferenceStore(options =>
                {
#if !RELEASE
                    options.UseTenantSpecificModel = false;
#endif
                })
                .WithTenantResolver<TenantByEmailResolver>();

            builder.ObjectSpaceProviders
                .AddSecuredXpo((serviceProvider, options) =>
                {
                    var connectionString = serviceProvider.GetRequiredService<IConnectionStringProvider>()
                        .GetConnectionString();
                    options.ConnectionString = connectionString;
                    options.ThreadSafe = true;
                    options.UseSharedDataStoreProvider = true;
                })
                .AddNonPersistent();
            builder.Security
                .UseIntegratedMode(options =>
                {
                    options.Lockout.Enabled = true;

                    options.RoleType = typeof(PermissionPolicyRole);
                    // ApplicationUser descends from PermissionPolicyUser and supports the OAuth authentication. For more information, refer to the following topic: https://docs.devexpress.com/eXpressAppFramework/402197
                    // If your application uses PermissionPolicyUser or a custom user type, set the UserType property as follows:
                    options.UserType = typeof(ApplicationUser);
                    // ApplicationUserLoginInfo is only necessary for applications that use the ApplicationUser user type.
                    // If you use PermissionPolicyUser or a custom user type, comment out the following line:
                    options.UserLoginInfoType = typeof(ApplicationUserLoginInfo);
                    options.UseXpoPermissionsCaching();
                    options.Events.OnSecurityStrategyCreated += securityStrategy =>
                    {
                        // Use the 'PermissionsReloadMode.NoCache' option to load the most recent permissions from the database once
                        // for every Session instance when secured data is accessed through this instance for the first time.
                        // Use the 'PermissionsReloadMode.CacheOnFirstAccess' option to reduce the number of database queries.
                        // In this case, permission requests are loaded and cached when secured data is accessed for the first time
                        // and used until the current user logs out.
                        // See the following article for more details: https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Security.SecurityStrategy.PermissionsReloadMode.
                        ((SecurityStrategy)securityStrategy).PermissionsReloadMode = PermissionsReloadMode.NoCache;
                    };
                })
                .AddPasswordAuthentication(options => { options.IsSupportChangePassword = true; });
        });
        var authentication = services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        });
        authentication.AddCookie(options => { options.LoginPath = "/LoginPage"; });
        authentication.AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                //ValidIssuer = Configuration["Authentication: Jwt:Issuer"],
                //ValidAudience = Configuration["Authentication: Jwt:Audience"],
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(Configuration["Authentication:Jwt:IssuerSigningKey"]!)),
                AuthenticationType = JwtBearerDefaults.AuthenticationScheme
            };
        });
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder(
                    JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .RequireXafAuthentication()
                .Build();
        });

        services
            .AddControllers()
            .AddOData((options, serviceProvider) =>
            {
                options
                    .AddRouteComponents("api/odata", new EdmModelBuilder(serviceProvider).GetEdmModel(),
                        ODataVersion.V401, routeServices => { routeServices.ConfigureXafWebApiServices(); })
                    .EnableQueryFeatures(100);
            });

        services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "erp API",
                Version = "v1",
                Description =
                    @"Use AddXafWebApi(options) in the erp.Blazor.Server\Startup.cs file to make Business Objects available in the Web API."
            });
            c.AddSecurityDefinition("JWT", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Name = "Bearer",
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "JWT"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        services.Configure<JsonOptions>(o =>
        {
            //The code below specifies that the naming of properties in an object serialized to JSON must always exactly match
            //the property names within the corresponding CLR type so that the property names are displayed correctly in the Swagger UI.
            //XPO is case-sensitive and requires this setting so that the example request data displayed by Swagger is always valid.
            //Comment this code out to revert to the default behavior.
            //See the following article for more information: https://learn.microsoft.com/en-us/dotnet/api/system.text.json.jsonserializeroptions.propertynamingpolicy
            o.JsonSerializerOptions.PropertyNamingPolicy = null;
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "erp WebApi v1"); });

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            //app.UseSwagger();
            //app.UseSwaggerUI(c =>
            //{
            //c.SwaggerEndpoint("/swagger/v1/swagger.json", "erp WebApi v1");
            //});
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. To change this for production scenarios, see: https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRequestLocalization();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();
        app.UseXaf();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapXafEndpoints();
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
            endpoints.MapControllers();
        });
    }
}