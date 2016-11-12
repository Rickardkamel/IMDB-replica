using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ImdbReplica.Context;
using ImdbReplica.Models;

namespace ImdbReplica.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            if (Session["userId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var userId = (Guid)Session["userId"];

            try
            {

                using (var context = new rkDBContext())
                {
                    var thisUser = context.Users.SingleOrDefault(x => x.Id == userId);

                    var user = new UserViewModel
                    {
                        Firstname = thisUser?.FirstName,
                        Lastname = thisUser?.LastName,
                        Username = thisUser?.Username,
                        Email = thisUser?.Email,
                        ProfileImagePath = userId.ToString() + ".png"
                    };

                    return View(user);
                }
            }
            catch
            {
                return View("Error");
            }
        }

        public ActionResult EditUser()
        {
            if (Session["userId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            UserViewModel userViewModelToReturn = null;

            try
            {
                using (var context = new rkDBContext())
                {
                    Guid userId;
                    if (Session["userId"] != null)
                    {
                        userId = (Guid)Session["userId"];
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    var user = context.Users.SingleOrDefault(x => x.Id == userId);

                    if (user != null)
                        userViewModelToReturn = new UserViewModel
                        {
                            Firstname = user.FirstName,
                            Lastname = user.LastName,
                            Email = user.Email,
                            Username = user.Username,
                            ProfileImagePath = user.Id.ToString() + ".png",
                        };
                }
                return userViewModelToReturn == null ? View("Error") : View(userViewModelToReturn);
            }
            catch
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult UpdateUser(UserViewModel userViewModel)
        {
            if (Session["userId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (!ModelState.IsValid)
            {
                EditUser();
                return View("EditUser");
            }
            try
            {
                using (var context = new rkDBContext())
                {
                    var userId = (Guid)Session["userId"];

                    var userToEdit = context.Users.SingleOrDefault(x => x.Id == userId);

                    

                    if (
                        context.Users.Any(
                            x =>
                                x.Username == userViewModel.Username && x.Id != userId ||
                                x.Email == userViewModel.Email && x.Id != userId))
                    {
                        ModelState.AddModelError("AlreadyExistsError",
                            "Username or Email already exists, please choose something else");
                        //return View("EditUser");
                        return View("EditUser", userViewModel);
                    }

                    if (userToEdit == null) return RedirectToAction("Index", "User");

                    userToEdit.FirstName = userViewModel.Firstname;
                    userToEdit.LastName = userViewModel.Lastname;
                    userToEdit.Email = userViewModel.Email;
                    userToEdit.Username = userViewModel.Username;

                    Session["userName"] = userToEdit.Username;

                    context.Entry(userToEdit).State = System.Data.Entity.EntityState.Modified;

                    context.SaveChanges();
                }
                return RedirectToAction("Index", "User");
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        public ActionResult EditPassword()
        {
            if (Session["userId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View("EditPassword");
        }

        [HttpPost]
        public ActionResult ChangeUserPassword(PasswordViewModel passwordViewModel)
        {
            if (Session["userId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (!ModelState.IsValid)
            {
                return View("EditPassword");
            }

            try
            {
                using (var context = new rkDBContext())
                {
                    var userId = (Guid)Session["userId"];
                    var user = context.Users.SingleOrDefault(x => x.Id == userId);
                    if (user == null) return RedirectToAction("Login", "Account");

                    string hashedPassword;
                    var accController = new AccountController();

                    using (var md5Data = MD5.Create())
                    {
                        hashedPassword = accController.GetMd5Hash(md5Data, passwordViewModel.OldPassword + user.Salt);
                    }

                    if (context.Users.Any(x => x.Id == userId && x.Password == hashedPassword))
                    {
                        string newHashedPassword;

                        using (var md5Data = MD5.Create())
                        {
                            newHashedPassword = accController.GetMd5Hash(md5Data,
                                passwordViewModel.NewPassword + user.Salt);
                        }
                        user.Password = newHashedPassword;
                    }
                    else
                    {
                        ModelState.AddModelError("WrongCredentialsError", "You submitted the wrong password, try again.");
                        return View("EditPassword");
                    }

                    context.Entry(user).State = System.Data.Entity.EntityState.Modified;
                    context.SaveChanges();
                }
                return RedirectToAction("Index", "User");
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public ActionResult DeleteUser()
        {
            var userId = (Guid)Session["userId"];

            try
            {
                using (var context = new rkDBContext())
                {
                    var userToDelete = context.Users.SingleOrDefault(x => x.Id == userId);

                    var reviewsToDelete = context.Reviews.Where(x => x.CreatorUserId == userId).ToList();

                    // Go through all reviews where "userToDelete" is owner
                    foreach (var review in reviewsToDelete)
                    {
                        // Remove all comments from "userToDelete"'s reviews.
                        var commentToReviewToDelete = context.CommentToReviews.Where(x => x.UserId == userId).ToList();

                        foreach (var commentToReview in commentToReviewToDelete)
                        {
                            context.CommentToReviews.Remove(commentToReview);
                        }

                        // Remove HasLiked, Ratings etc.
                        var userToReviewToRemove = context.UserToReviews.Where(x => x.UserId == userId).ToList();

                        foreach (var userToReview in userToReviewToRemove)
                        {
                            context.UserToReviews.Remove(userToReview);
                        }

                        // Remove review.
                        context.Reviews.Remove(review);
                    }

                    // Remove "userId"'s comments
                    var commentsByUser = context.CommentToReviews.Where(x => x.UserId == userId).ToList();

                    foreach (var comment in commentsByUser)
                    {
                        context.CommentToReviews.Remove(comment);
                    }

                    // Remove "userId"'s ratings
                    var likesOrDislikesByUser = context.UserToReviews.Where(x => x.UserId == userId).ToList();

                    foreach (var likeOrDislikeByUser in likesOrDislikesByUser)
                    {
                        context.UserToReviews.Remove(likeOrDislikeByUser);
                    }

                    // Remove user
                    context.Users.Remove(userToDelete);
                    context.SaveChanges();
                }

                var fullPath = Request.MapPath("~/Images/ProfileImages/" + userId + ".png");
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }

                FormsAuthentication.SignOut();
                Session["userId"] = null;
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public ActionResult Upload()
        {
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Session["userId"].ToString() + ".png";

                    var path = Path.Combine(Server.MapPath("~/Images/ProfileImages/"), fileName);

                    file.SaveAs(path);
                }
            }

            return RedirectToAction("EditUser", "User");
        }

        public ActionResult DifferentUser(Guid? creatorUserId)
        {
            if (Session["userId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (creatorUserId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                using (var context = new rkDBContext())
                {
                    var user = context.Users.Where(x => x.Id == creatorUserId).Select(x => new UserViewModel
                    {
                        Username = x.Username,
                        Firstname = x.FirstName,
                        Lastname = x.LastName,
                        Email = x.Email,
                        ProfileImagePath = x.Id.ToString() + ".png"
                    }).SingleOrDefault();

                    var userReviews = context.Reviews.Where(x => x.CreatorUserId == creatorUserId).ToList();
                    foreach (var userReview in userReviews)
                    {
                        var reviewToAdd = new ReviewViewModel
                        {
                            ReviewId = userReview.Id,
                            Title = userReview.Title,
                            Description = userReview.Description,
                            CreatedDate = userReview.CreatedDate.ToShortDateString(),
                            Type = userReview.Type,
                            UserRating = userReview.UserRating,
                        };

                        user?.ReviewViewModelList.Add(reviewToAdd);
                    }

                    return user == null ? View("Error") : View(user);
                }
            }
            catch
            {
                return View("Error");
            }
        }

        [HttpPost]
        public JsonResult SearchUsers()
        {
            List<string> userList = null;
            int succeeded = 0;

            try
            {
                using (var context = new rkDBContext())
                {
                    userList = context.Users.Select(x => x.Username).ToList();
                }
            }
            catch
            {
                return Json(new { succeeded, userList });
            }
            succeeded = 1;
            return Json(new { succeeded, userList });
        }

        [HttpPost]
        public JsonResult GoToSearchedUser(string userName)
        {
            var currentUsername = Session["userName"].ToString();
            var searchedUserId = Guid.Empty;

            var succeeded = 0;

            try
            {
                using (var context = new rkDBContext())
                {
                    if (context.Users.Any(x => x.Username == userName))
                    {
                        succeeded = 1;
                        searchedUserId =
                            context.Users.Where(x => x.Username == userName).Select(x => x.Id).SingleOrDefault();
                    }
                }
            }
            catch
            {
                return Json(new { currentUsername, searchedUserId, succeeded });
            }
            return Json(new
            {
                currentUsername,
                searchedUserId,
                succeeded
            });
        }
    }
}