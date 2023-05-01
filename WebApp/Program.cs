using DataLayer;
using DataLayer.Models;

namespace WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Database.CreateDatabase();
            Database.CreateTable<Room>();
            Database.CreateTable<Reservation>();
            Database.CreateTable<User>();
            Database.CreateTable<Review>();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            LoggedInSingleton.Instance.LoggedInUser = null;

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}