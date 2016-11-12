using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ImdbReplica.Context;
using ImdbReplica.Models;
using System.Data.Entity.Validation;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;

namespace ImdbReplica.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login()
        {
            return View("Login");
        }

        [HttpPost]
        public ActionResult UserLogin(LoginViewModel userToLogin)
        {
            if (!ModelState.IsValid)
            {
                return View("Login");
            }
            try
            {

                using (var context = new rkDBContext())
                {
                    // Get salt for current user
                    string userSalt =
                        context.Users.Where(x => x.Username == userToLogin.Username)
                            .Select(x => x.Salt)
                            .FirstOrDefault();

                    string hashedPassword;
                    using (var md5Hash = MD5.Create())
                    {
                        hashedPassword = GetMd5Hash(md5Hash, userToLogin.Password + userSalt);
                    }

                    if (context.Users.Any(x => x.Username == userToLogin.Username && x.Password == hashedPassword))
                    {
                        FormsAuthentication.SetAuthCookie(userToLogin.Username, false);
                        var userId = context.Users.Where(
                            x => x.Username == userToLogin.Username && x.Password == hashedPassword)
                            .Select(x => x.Id).FirstOrDefault();
                        Session["userId"] = userId;
                        Session["userName"] = userToLogin.Username;

                        return RedirectToAction("Reviews", "Review");
                    }
                    ModelState.AddModelError("CredentialError", "Wrong username or password.");
                    return View("Login");
                }
            }
            catch
            {
                return View("Error");
            }
        }

        public ActionResult UserLogOut()
        {
            FormsAuthentication.SignOut();
            Session["userId"] = null;
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Register()
        {
            return View("Register");
        }

        [HttpPost]
        public ActionResult RegisterUser(RegisterViewModel newUserToRegister)
        {
            if (!ModelState.IsValid)
            {
                return View("Register");
            }

            try
            {

                using (var context = new rkDBContext())
                {
                    if (
                        context.Users.Any(
                            x => x.Username == newUserToRegister.Username || x.Email == newUserToRegister.Email))
                    {
                        ModelState.AddModelError("ExistsError",
                            "Username or Email already exist, please choose something else.");
                        return View("Register");
                    }

                    // Hash pw and generate salt for it.
                    var newUserId = Guid.NewGuid();
                    string hashedAndSaltedPassword;
                    var salt = GenerateSalt();
                    using (var md5Hash = MD5.Create())
                    {
                        hashedAndSaltedPassword = GetMd5Hash(md5Hash, newUserToRegister.Password + salt);
                    }

                    var newUser = new User
                    {
                        Id = newUserId,
                        FirstName = newUserToRegister.Firstname,
                        LastName = newUserToRegister.Lastname,
                        Username = newUserToRegister.Username,
                        Password = hashedAndSaltedPassword,
                        Email = newUserToRegister.Email,
                        Salt = salt
                    };

                    context.Users.Add(newUser);
                    context.SaveChanges();

                    ModelState.Clear();
                }
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }

            return RedirectToAction("Login", "Account");
        }

        private string GenerateSalt()
        {
            // Här genereras salt:en.
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string salt = new string(
                Enumerable.Repeat(chars, 8)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());

            return salt;
        }

        public string GetMd5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }
    }
}