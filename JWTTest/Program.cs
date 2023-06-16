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
                        Title = "����",
                        Version = "1.0",
                        Description = "����",
                    });
                    var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{nameof(JWTTest)}.xml");
                    options.IncludeXmlComments(file, true);
                    #region JWT��֤
                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey,
                        Description = "���� JWT",
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

            //ע�����
            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true, //�Ƿ���֤Issuer
                        ValidIssuer = jwtOption.Issuer, //������Issuer
                        ValidateAudience = true, //�Ƿ���֤Audience
                        ValidAudience = jwtOption.Audience, //������Audience   
                        ValidateIssuerSigningKey = true, //�Ƿ���֤SecurityKey
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOption.SecretKey)), //SecurityKey
                        ValidateLifetime = true, //�Ƿ���֤ʧЧʱ��
                        ClockSkew = TimeSpan.FromSeconds(30), //����ʱ���ݴ�ֵ�������������ʱ�䲻ͬ�����⣨�룩
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


            //�����м����UseAuthentication����֤����������������Ҫ�����֤���м��ǰ���ã����� UseAuthorization����Ȩ����
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}