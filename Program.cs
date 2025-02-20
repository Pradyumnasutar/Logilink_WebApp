
//using Microsoft.AspNetCore.Authentication.AzureAD.UI;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LeS_LogiLink_WebApp.Models;
using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using LeSDataMain;
using LeS_LogiLink_WebApp.Interface;
using LeS_LogiLink_WebApp.Repo;
using System.Net.Http;
using System.Configuration;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IUserDefaultData, UserDefaultData>();
builder.Services.AddScoped<IEpodUserDefaultData, EpodUserDefaultData>();

builder.Services.AddScoped<CustomActionFilterAttribute>();

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddControllersWithViews(options =>
{
    // Add the custom authorization filter as a global filter
    options.Filters.Add(typeof(CustomActionFilterAttribute));

    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

});
builder.Services.AddHttpClient<ApiCallRoutine>();
builder.Services.AddScoped<ApiCallRoutine>();
builder.Services.AddMvc();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = "LogisticManagementCookie"+ DateTime.Now.ToString("ss.fff");
   // options.IdleTimeout = TimeSpan.FromMinutes(20); // to be uncommented after completing development
});

//builder.Services.AddControllers(options =>
//{
//    //options.Filters.Add(new CustomActionFilterAttribute());
//});
//builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddHttpContextAccessor();
//builder.Services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();
#region IOrders
builder.Services.AddScoped<IOrders>(provider =>
{
    var routine = provider.GetRequiredService<ApiCallRoutine>();
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    var session = httpContextAccessor.HttpContext?.Session;
    var userType = session?.GetString("AddressType");
    if (userType != null && userType != "")
    {
        return userType switch
        {
            "buyer" => new BuyerOrders(routine),

            _ => new SupplierOrders(routine)
        };
    }
    else
    {
        return new SupplierOrders(routine);
    }
});
#endregion IOrders

#region IOutbound
builder.Services.AddScoped<IOutbound>(provider =>
{
    var routine = provider.GetRequiredService<ApiCallRoutine>();
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    var session = httpContextAccessor.HttpContext?.Session;
    var userType = session?.GetString("AddressType");
    if (userType != null && userType != "")
    {
        return userType switch
        {
            "buyer" => new BuyerOutbound(routine),

            _ => new SupplierOutbound(routine)
        };
    }
    else
    {
        return new SupplierOutbound(routine);
    }
});
#endregion IOutbound

#region IePOD
builder.Services.AddScoped<IePOD>(provider =>
{
    var routine = provider.GetRequiredService<ApiCallRoutine>();
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    var session = httpContextAccessor.HttpContext?.Session;
    var userType = session?.GetString("AddressType");
    if (userType != null && userType != "")
    {
        return userType switch
        {
            "buyer" => new BuyerEPOD(routine),

            _ => new SupplierEPOD(routine)
        };
    }
    else
    {
        return new SupplierEPOD(routine);
    }
});
#endregion IePOD

#region IInbound
builder.Services.AddScoped<IInbound>(provider =>
{
    var routine = provider.GetRequiredService<ApiCallRoutine>();
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    var session = httpContextAccessor.HttpContext?.Session;
    var userType = session?.GetString("AddressType");
    if (userType != null && userType != "")
    {
        return userType switch
        {
            "buyer" => new BuyerInbound(routine),

            _ => new SupplierInbound(routine)
        };
    }
    else
    {
        return new SupplierInbound(routine);
    }
});
#endregion IInbound

#region IMasters
builder.Services.AddScoped<IMasters>(provider =>
{
    var routine = provider.GetRequiredService<ApiCallRoutine>();
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    var session = httpContextAccessor.HttpContext?.Session;
    var userType = session?.GetString("AddressType");
    if (userType != null && userType != "")
    {
        return userType switch
        {
            "buyer" => new BuyerMasters(routine),

            _ => new SupplierMasters(routine)
        };
    }
    else
    {
        return new SupplierMasters(routine);
    }
});
#endregion IMasters

builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();
builder.Services.AddControllersWithViews();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    LeSDM.AddConsoleLog("Development environment started!");
    app.UseDeveloperExceptionPage();
}
else
{
    LeSDM.AddConsoleLog("Production environment started!");
    app.UseExceptionHandler(new ExceptionHandlerOptions()
    {
        AllowStatusCode404Response = true,
        ExceptionHandlingPath = "/Authenticate/Logout?msg=Something went wrong!"
    });
   
   // app.UseExceptionHandler("/Home/Error?statuscode=501");
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/Authenticate/NotFound"); 
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseRouting();

app.UseAuthorization();
//app.UseAuthentication();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Authenticate}/{action=Login}/{id?}");
app.MapRazorPages();

app.Run();
