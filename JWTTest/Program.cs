using System.Text;
using JWTTest.Helpers;
using JWTTest.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace JWTTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(
                options =>
                {
                    options.SwaggerDoc("V1", new OpenApiInfo()
                    {
                        Title = "测试",
                        Version = "1.0",
                        Description = "测试",
                    });
                    var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{nameof(JWTTest)}.xml");
                    options.IncludeXmlComments(file, true);
                    #region JWT验证
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Description = "输入 JWT",
                        Name = "Authorization",
                        BearerFormat = "JWT",
                        Scheme = "Bearer"
                    });
                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                        }
                    }); 
                    #endregion
                }
            );
            var jwtOption = builder.Configuration.GetSection("Jwt").Get<JwtOption>();
            builder.Services.AddSingleton(jwtOption);
            builder.Services.AddSingleton<JwtHelper>();

            //注册服务
            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true, //是否验证Issuer
                        ValidIssuer = jwtOption.Issuer, //发行人Issuer
                        ValidateAudience = true, //是否验证Audience
                        ValidAudience = jwtOption.Audience, //订阅人Audience   
                        ValidateIssuerSigningKey = true, //是否验证SecurityKey
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOption.SecretKey)), //SecurityKey
                        ValidateLifetime = true, //是否验证失效时间
                        ClockSkew = TimeSpan.FromSeconds(30), //过期时间容错值，解决服务器端时间不同步问题（秒）
                        RequireExpirationTime = true,
                    };
                });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(n => n.SwaggerEndpoint("/swagger/V1/swagger.json", "V1"));
            }

            app.UseHttpsRedirection();


            //调用中间件：UseAuthentication（认证），必须在所有需要身份认证的中间件前调用，比如 UseAuthorization（授权）。
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}