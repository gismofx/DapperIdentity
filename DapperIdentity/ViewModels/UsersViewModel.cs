using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IdentityUser = DapperIdentity.Models.CustomIdentityUser;
using System.Text.RegularExpressions;
using DapperIdentity.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Components;
using CPEIdentity.ViewModels;

namespace DapperIdentity.ViewModels
{
    [Obsolete("Use UsersEditPageViewModel",true)]
    public class UsersViewModel : BaseViewModel //PageModel
    {
        
        public List<IdentityUser> Users;
        /*public List<IdentityUser> Users
        {
            get => _Users;
            set
            {
                SetValue(ref _Users, value);
            }
        }*/


        private UserManager<IdentityUser> _UserManager;
        private IEmailSender _emailSender;

        private readonly IHttpContextAccessor _accessor;
        private readonly LinkGenerator _generator;
        private NavigationManager NavManager;

        public UsersViewModel(UserManager<IdentityUser> userManager, IEmailSender emailSender,
                              IHttpContextAccessor accessor, LinkGenerator generator, NavigationManager navmanager)
            
        {
            _UserManager = userManager;
            _emailSender = emailSender;
            //Url = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext); 
            _accessor = accessor;
            _generator = generator;
            NavManager = navmanager;
            
        }

        public async Task Load()
        {
            Users = _UserManager.Users.ToList();
        }

        public IEnumerable<string> PasswordStrength(string pw)
        {
            if (string.IsNullOrWhiteSpace(pw))
            {
                yield return "Password is required!";
                yield break;
            }
            if (pw.Length < 8)
                yield return "Password must be at least of length 8";
            if (!Regex.IsMatch(pw, @"[A-Z]"))
                yield return "Password must contain at least one capital letter";
            if (!Regex.IsMatch(pw, @"[a-z]"))
                yield return "Password must contain at least one lowercase letter";
            if (!Regex.IsMatch(pw, @"[0-9]"))
                yield return "Password must contain at least one digit";
        }

        /*public string PasswordMatch(string arg)
        {
            /*if (pwField1.Value != arg)
                return "Passwords don't match";
            return null;

        }*/
        public async Task ResendConfirmation(IdentityUser user)
        {
            await SendEmailConfirmation(user);
            ShowUserMessage(new() { Text = "Confirmation Resent!", Type = eMessageType.Exclamation });
        }

        private async Task SendEmailConfirmation(IdentityUser user)
        {
            IsBusy = true;
            await Task.Delay(1);
            string returnUrl = NavManager.BaseUri; // @"/";
            var code = await _UserManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = _generator.GetUriByPage(
                _accessor.HttpContext,
                page: "/Account/ConfirmEmail",
                handler: null,
                values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl }
                );
            await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
            ShowUserMessage(new() { Text = $"User {user.Email} registered! Email confirmation sent!", Type = eMessageType.Exclamation });
            IsBusy = false;
        }

        //public async Task OnValidSubmitRegister(RegisterModel newUser)
        //{
        //    //_generator.
        //    string returnUrl = NavManager.BaseUri;// @"/"; //Url.Content("~/");
        //    var user = new IdentityUser { UserName = newUser.Email, Email = newUser.Email, FirstName=newUser.FirstName, LastName = newUser.LastName };
        //    var result = await _UserManager.CreateAsync(user, newUser.Password );
        //    if (result.Succeeded)
        //    {
        //        //_logger.LogInformation("User created a new account with password.");

        //        var code = await _UserManager.GenerateEmailConfirmationTokenAsync(user);
        //        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        //        var callbackUrl = _generator.GetUriByPage(
        //            _accessor.HttpContext,
        //            page: "/Account/ConfirmEmail",
        //            handler: null,
        //            values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl }
        //            );

        //        await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
        //            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

        //        if (_UserManager.Options.SignIn.RequireConfirmedAccount)
        //        {
        //            //return RedirectToPage("RegisterConfirmation", new { email = user.Email, returnUrl = returnUrl });
        //        }
        //        /*else
        //        {
        //            await _signInManager.SignInAsync(user, isPersistent: false);
        //            return LocalRedirect(returnUrl);
        //        }*/
        //    }
        //    //foreach (var error in result.Errors)
        //    //{
        //    //    ModelState.AddModelError(string.Empty, error.Description);
        //    //}
        //}

    }

}
