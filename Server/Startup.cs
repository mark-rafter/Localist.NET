using AspNetCore.Identity.Mongo;
using AspNetCore.Identity.Mongo.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Localist.Server.Clients;
using Localist.Server.Config;
using Localist.Server.Middleware;
using Localist.Server.Services;

namespace Localist.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // config
            var dbOptions = GetOptions<DatabaseOptions>();
            services.AddSingleton<IDatabaseOptions>(dbOptions);

            var jwtOptions = GetOptions<JwtOptions>();
            services.AddSingleton<IJwtOptions>(jwtOptions);

            services.AddSingleton<IImageUploadApiOptions>(GetOptions<ImageUploadApiOptions>());
            services.AddSingleton<IVapidOptions>(GetOptions<VapidOptions>());

            MongoDbContext.RegisterEntityMaps();

            // services
            services.AddSingleton<IDbContext, MongoDbContext>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IInviteService, InviteService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<IProfileService, ProfileService>();

            services.AddHostedService<QueuedNotificationsService>();
            services.AddSingleton<INotificationsQueue, NotificationsQueue>();

            // clients
            services.AddHttpClient<IImageUploadApi, ImageUploadApi>();

            // identity
            services.AddIdentityMongoDbProvider<LocalistUser, MongoRole>(
                identityOptions =>
                {
                    identityOptions.Password.RequiredLength = 8;
                    identityOptions.Password.RequireLowercase = false;
                    identityOptions.Password.RequireUppercase = false;
                    identityOptions.Password.RequireNonAlphanumeric = false;
                    identityOptions.Password.RequireDigit = false;
                },
                mongoIdentityOptions =>
                {
                    mongoIdentityOptions.ConnectionString = dbOptions.ConnectionString;
                });

            services.AddAuthentication(o =>
                {
                    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtOptions.SecurityKey))
                    };
                });

            services.AddResponseCompression();
            services.AddControllersWithViews(config => config.Filters.Add(typeof(GlobalExceptionFilter)));
            services.AddRazorPages();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Localist.Server", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Localist.Server v1"));
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.UseResponseCompression();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }

        TOptions GetOptions<TOptions>() =>
            Configuration.GetSection(typeof(TOptions).Name).Get<TOptions>()
                ?? throw new System.InvalidOperationException($"appsettings.json missing {typeof(TOptions).Name} section");
    }
}
