using ApartmentManagement.Repositories;
using ApartmentManagement.Repositories.Interfaces;
using ApartmentManagement.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

namespace ApartmentManagement
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSingleton<IMongoDBService, MongoDBService>();
            builder.Services.AddScoped<Jwt>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "MyCookieAuth";
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
            .AddCookie("MyCookieAuth", options =>
            {
                options.Cookie.Name = "MyCookieAuth";
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
            })
            .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.SignInScheme = "MyCookieAuth";
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

                options.Scope.Add("profile");
                options.Scope.Add("email");
                options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");

                options.Events.OnRedirectToAuthorizationEndpoint = context =>
                {
                    context.Response.Redirect(context.RedirectUri + "&prompt=select_account");
                    return Task.CompletedTask;
                };
            });




            builder.Services.AddScoped<EmailSender>();

            builder.Services.AddSingleton<CloudService>();
            builder.Services.AddScoped<IApartmentRepository, ApartmentRepository>();
            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<IRoomRepository, RoomRepository>();

            builder.Services.AddControllersWithViews();


            builder.Services.AddAuthorization();
            builder.Services.AddHttpClient(); 

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();


            app.UseAuthorization();
            app.UseAuthentication();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Room}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
