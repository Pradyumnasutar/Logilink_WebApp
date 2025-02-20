using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using LeS_LogiLink_WebApp.Controllers;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using LeS_LogiLink_WebApp.Data;
using LeSDataMain;

//public class CustomAuthorizationAttribute : TypeFilterAttribute
//{
//    public CustomAuthorizationAttribute() : base(typeof(CustomAuthorizationFilter))
//    {


//    }
//}

//public class CustomAuthorizationFilter : IAuthorizationFilter
//{ 
//    private readonly IHttpContextAccessor _httpContextAccessor;

//    public CustomAuthorizationFilter(IHttpContextAccessor httpContextAccessor)
//    {
//        _httpContextAccessor = httpContextAccessor;
//    }
//    public void OnAuthorization(AuthorizationFilterContext context)
//    {


//        var session = _httpContextAccessor.HttpContext.Session;
//        if (!session.IsAvailable)
//        {

//        }
//        var userid = _httpContextAccessor.HttpContext.Session.GetString("UserID");
//        // Check if the user is authenticated or has the required role, claims, etc.


//        if (userid == null || userid.Length == 0)
//        {
//            session.Remove("LOGIN_FAIL_MESSAGE");
//            session.Remove("UserName");
//            session.Remove("UserID");
//            session.Clear();
//            context.Result = new RedirectToActionResult("Logout", "Authenticate", new { msg = "Your session has expired!" });
//        }

//    }

//}

public class CustomActionFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Code executed before the action method

        var session = context.HttpContext.Session;

        string ControllerName= context.ActionDescriptor.RouteValues["Controller"].ToString();
        if ( ControllerName.ToLower() != "authenticate"&&ControllerName.ToLower()!="guestepod")
        {
            var userid = context.HttpContext.Session.GetString("UserID");
            // Check if the user is authenticated or has the required role, claims, etc.
            var epoduserid = context.HttpContext.Session.GetString("EPOD_UserID");

            if ((userid == null || userid.Length == 0||userid=="0")&&(epoduserid==null|| epoduserid.Length==0|| epoduserid=="0"))
            {
                session.Remove("LOGIN_FAIL_MESSAGE");
                session.Remove("UserName");
                session.Remove("UserID");
                session.Clear();
                context.Result = new  RedirectToActionResult("Logout", "Authenticate", new { msg = "Your session has expired!" });
            }
        }
        

    }

}

