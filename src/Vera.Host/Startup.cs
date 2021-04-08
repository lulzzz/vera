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
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var jwtIssuer = _configuration["VERA:JWT:ISSUER"];
            var jwtKey = new SymmetricSecurityKey(Convert.FromBase64String(_configuration["VERA:JWT:KEY"]));
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtIssuer,
                        IssuerSigningKey = jwtKey
                    };
                });

            services.AddControllers();
            
            services.AddGrpc(o =>
            {
                o.EnableDetailedErrors = _env.IsDevelopment();
            });

            services.AddSingleton<ISecurityTokenGenerator>(new JwtSecurityTokenGenerator(jwtIssuer, jwtIssuer, jwtKey));
            
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddHostedService<QueueHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            // Used for the ESC POS encoding. See EscPosVisitor
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (_env.IsDevelopment())
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

                endpoints.MapGrpcService<UserRegisterService>();
                endpoints.MapGrpcService<LoginService>();
                endpoints.MapGrpcService<AccountService>();
                endpoints.MapGrpcService<TokenService>();
                endpoints.MapGrpcService<InvoiceService>();
                endpoints.MapGrpcService<ReceiptService>();
                endpoints.MapGrpcService<AuditService>();
                endpoints.MapGrpcService<SupplierService>();
                endpoints.MapGrpcService<PeriodService>();
                endpoints.MapGrpcService<RegisterService>();
                endpoints.MapGrpcService<ReportService>();
            });
        }
    }
}
