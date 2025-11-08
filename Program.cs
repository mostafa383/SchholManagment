using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartSchool.Data;
using SmartSchool.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddDbContext<SchoolContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentity<Teacher, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.User.RequireUniqueEmail = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(3);
})
    .AddEntityFrameworkStores<SchoolContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    // تعيين مخطط JWT ليكون المخطط الافتراضي للاستجابات (Challenger) والتحقق (Authenticate)
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
    {
        // هنا التحكم في معايير التحقق من صحة التوكن
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // 1. مفتاح التشفير (الذي وقعنا به التوكن)
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"])),
            
            // 2. التحكم في الجهة المصدرة (Issuer)
            ValidateIssuer = true, 
            ValidIssuer = builder.Configuration["JWT:Issuer"], 
            
            // 3. التحكم في الجمهور المستهدف (Audience)
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT:Audience"],
            
            // 4. التحكم في تاريخ انتهاء الصلاحية
            ValidateLifetime = true,
            
            // 5. السماح بفترة سماح للتاريخ (مهم لمنع مشاكل فروق التوقيت)
            ClockSkew = TimeSpan.Zero 
        };
});;

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();