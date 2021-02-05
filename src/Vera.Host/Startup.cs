using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Vera.Host.Security;
using Vera.Host.Services;

namespace Vera.Host
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["VERA:JWT:ISSUER"],
                        ValidAudience = Configuration["VERA:JWT:ISSUER"],
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Convert.FromBase64String(Configuration["VERA:JWT:KEY"]))
                    };
                });

            services.AddControllers();
            services.AddGrpc(o =>
            {
                // TODO(kevin): only in development mode
                o.EnableDetailedErrors = true;
            });

            services.AddTransient<ISecurityTokenGenerator>(p => new JwtSecurityTokenGenerator(Configuration));

            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddHostedService<QueueHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Used for the ESC POS encoding. See EscPosVisitor
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapGrpcService<RegisterService>();
                endpoints.MapGrpcService<LoginService>();
                endpoints.MapGrpcService<AccountService>();
                endpoints.MapGrpcService<TokenService>();
                endpoints.MapGrpcService<InvoiceService>();
                endpoints.MapGrpcService<ReceiptService>();
                endpoints.MapGrpcService<AuditService>();
            });
        }
    }
}