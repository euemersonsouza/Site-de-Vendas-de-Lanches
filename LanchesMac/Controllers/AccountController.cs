﻿using LanchesMac.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace LanchesMac.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login(string returUrl)
        {
            return View(new LoginViewModel()
            {
                ReturnUrl = returUrl
            });
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View(loginVM);
            //}
            //var user = await _userManager.FindByNameAsync(loginVM.UserName);
            //if (user == null)
            //{
            //    var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, false, false);
            //    if (result.Succeeded)
            //    {
            //        if (string.IsNullOrEmpty(loginVM.ReturnUrl))
            //        {
            //            return RedirectToAction("Home", "Index");
            //        }
            //        return View(loginVM.ReturnUrl);
            //    }
            //}
            //ModelState.AddModelError("", "Falha ao realizar login!");
            //return View(loginVM);
            if (!ModelState.IsValid)
                return View(loginVM);

            var user = await _userManager.FindByNameAsync(loginVM.UserName);

            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user,
                    loginVM.Password, false, false);

                if (result.Succeeded)
                {
                    if (string.IsNullOrEmpty(loginVM.ReturnUrl))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    return Redirect(loginVM.ReturnUrl);
                }
            }
            ModelState.AddModelError("", "Falha ao realizar o login!!");
            return View(loginVM);
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async  Task<IActionResult> Register(LoginViewModel registroVM)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser() { UserName = registroVM.UserName };
                var result = await _userManager.CreateAsync(user, registroVM.Password);

                if (result.Succeeded)
                {
                   // await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    ModelState.AddModelError("Register", "Falha ao registrar usuário!");
                }
            }
            return View(registroVM);    
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.User = null;
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}