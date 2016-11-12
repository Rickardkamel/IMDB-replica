using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ImdbReplica.Context;
using ImdbReplica.Models;

namespace ImdbReplica.Controllers
{
    public class ReviewController : Controller
    {
        // GET: Review
        public ActionResult Index()
        {
            if (Session["userId"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = (Guid)Session["userId"];
            List<Review> allUserReviews;

            try
            {
                using (var context = new rkDBContext())
                {
                    allUserReviews = context.Reviews.Where(x => x.CreatorUserId == userId).ToList();
                }


                var reviewList = allUserReviews.Select(review => new ReviewViewModel
                {
                    ReviewId = review.Id,
                    Title = review.Title,
                    Description = review.Description,
                    CreatedDate = review.CreatedDate.ToShortDateString(),
                    UserRating = review.UserRating,
                    Type = review.Type,
                }).ToList();


                return View(reviewList);
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        public ActionResult Reviews()
        {
            if (Session["userId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var currentUserId = (Guid)Session["userId"];
            try
            {
                using (var context = new rkDBContext())
                {
                    var allReviews = context.Reviews.ToList();

                    if (allReviews.Count <= 0) return View();
                    var allReviewsViewModelList = new List<AllReviewsViewModel>();
                    foreach (var review in allReviews)
                    {
                        // Fetch userinfo
                        var reviewAuthor = context.Users.FirstOrDefault(x => x.Id == review.CreatorUserId);

                        // Calculate totalRating
                        var userToReviewList = context.UserToReviews.Where(x => x.ReviewId == review.Id).ToList();

                        var calculateReviewRating = CalculateReviewRating(userToReviewList);
                        if (calculateReviewRating == null) continue;
                        var totalRating = (decimal)calculateReviewRating;

                        // Fill AllReviewsViewModel with reviewValue to send it to the view
                        if (reviewAuthor == null) return View(allReviewsViewModelList);
                        var reviewToAdd = new AllReviewsViewModel
                        {
                            ReviewId = review.Id,
                            CreatorUserId = reviewAuthor.Id,
                            CreatedBy = reviewAuthor.Username,
                            Title = review.Title,
                            CreatedDate = review.CreatedDate.ToShortDateString(),
                            Description = review.Description,
                            DisLikes = review.DislikeCount,
                            Likes = review.LikeCount,
                            TotalRating = totalRating,
                            UserRating = review.UserRating,
                            HasProfilePicture = ConfirmProfilePicture(reviewAuthor.Id),
                            Type = review.Type
                        };

                        allReviewsViewModelList.Add(reviewToAdd);
                    }
                    return View(allReviewsViewModelList);

                }
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        public ActionResult ShowReview(Guid? reviewId)
        {
            if (Session["userId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (reviewId == null)
            {
                return RedirectToAction("Reviews", "Review");
            }
            try
            {
                using (var context = new rkDBContext())
                {
                    var reviewRating =
                        CalculateReviewRating(context.UserToReviews.Where(x => x.ReviewId == reviewId).ToList());

                    var reviewToShow = context.Reviews.Where(x => x.Id == reviewId).Select(x => new AllReviewsViewModel
                    {
                        Title = x.Title,
                        CreatorUserId = x.CreatorUserId,
                        CreatedBy = x.User.Username,
                        UserRating = x.UserRating,
                        ReviewId = x.Id,
                        Description = x.Description,
                        DisLikes = x.DislikeCount,
                        Likes = x.LikeCount,
                        TotalRating = (decimal)reviewRating,
                        Type = x.Type,
                        ReviewImagePath = reviewId.ToString() + ".png"
                    }).FirstOrDefault();

                    var reviewComments =
                        context.CommentToReviews.OrderByDescending(x => x.CreatedDate)
                            .Where(x => x.ReviewId == reviewId)
                            .ToList();
                    foreach (var item in reviewComments)
                    {
                        var commentToAdd = new CommentToReview
                        {
                            Id = item.Id,
                            ReviewId = item.ReviewId,
                            Comment = item.Comment,
                            CreatedDate = item.CreatedDate,
                            User = context.Users.SingleOrDefault(x => x.Id == item.UserId)
                        };
                        reviewToShow?.CommentToReviewList.Add(commentToAdd);
                    }
                    return reviewToShow == null ? View("Error") : View(reviewToShow);
                }
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        public ActionResult NewReview()
        {
            if (Session["userId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var reviewViewModel = new ReviewViewModel();

            return View(reviewViewModel);
        }

        [HttpPost]
        public ActionResult CreateNewReview(ReviewViewModel reviewViewModel)
        {
            if (Session["userId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (!ModelState.IsValid || reviewViewModel.UserRating == 0)
            {
                ModelState.AddModelError("RatingError", "You have to set a rating.");
                var returnModel = new ReviewViewModel();

                return View("NewReview", returnModel);
            }

            if (Session["userId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var currentUserId = (Guid)Session["userId"];

            try
            {
                using (var context = new rkDBContext())
                {
                    var newReview = new Review
                    {
                        Id = Guid.NewGuid(),
                        CreatorUserId = currentUserId,
                        Title = reviewViewModel.Title,
                        Description = reviewViewModel.Description,
                        Type = reviewViewModel.Type,
                        UserRating = reviewViewModel.UserRating,
                        CreatedDate = DateTime.Now
                    };

                    UploadPicture(newReview.Id);

                    context.Reviews.Add(newReview);
                    context.SaveChanges();
                }
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
            return RedirectToAction("Index", "Review");
        }

        [HttpGet]
        public ActionResult EditReview(Guid? reviewId)
        {
            if (Session["userId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (reviewId == null)
            {
                return RedirectToAction("Index", "Review");
            }
            var userId = (Guid)Session["userId"];
            try
            {
                using (var context = new rkDBContext())
                {
                    var review = context.Reviews.FirstOrDefault(c => c.Id == reviewId);
                    if (userId != review?.CreatorUserId)
                    {
                        return View("Error");
                    } 

                    var reviewToEdit = context.Reviews.Where(x => x.Id == reviewId).Select(x => new ReviewViewModel
                    {
                        Title = x.Title,
                        Description = x.Description,
                        Type = x.Type,
                        UserRating = x.UserRating,
                        ReviewId = x.Id,
                        ReviewImagePath = reviewId.ToString() + ".png"
                    }).SingleOrDefault();

                    return reviewToEdit == null ? View("Error") : View(reviewToEdit);
                }
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public ActionResult EditSelectedReview(ReviewViewModel reviewViewModel)
        {
            if (Session["userId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            if (!ModelState.IsValid)
            {
                return View("Index", "Reviews");
            }
            try
            {
                using (var context = new rkDBContext())
                {
                    var reviewToUpdate = context.Reviews.SingleOrDefault(x => x.Id == reviewViewModel.ReviewId);

                    if (reviewToUpdate != null)
                    {
                        reviewToUpdate.Title = reviewViewModel.Title;
                        reviewToUpdate.Description = reviewViewModel.Description;
                        reviewToUpdate.Type = reviewViewModel.Type;
                        reviewToUpdate.UserRating = reviewViewModel.UserRating;
                    }
                    else
                    {
                        return RedirectToAction("Error", "Home");
                    }

                    context.SaveChanges();
                }
            }
            catch
            {
                return RedirectToAction("Error", "Home");
            }
            return RedirectToAction("Index", "Review");

        }

        public JsonResult DeleteSelectedReviews(List<Guid> selectedReviewsIds)
        {
            if (selectedReviewsIds == null) return Json(false);
            try
            {
                using (var context = new rkDBContext())
                {
                    foreach (var id in selectedReviewsIds)
                    {
                        var reviewToDelete = context.Reviews.SingleOrDefault(x => x.Id == id);

                        context.Reviews.Remove(reviewToDelete);
                        context.SaveChanges();

                        var fullImagePath = Request.MapPath("~/Images/ReviewImages/" + id + ".png");
                        if (System.IO.File.Exists(fullImagePath))
                        {
                            System.IO.File.Delete(fullImagePath);
                        }
                    }
                }
            }
            catch
            {
                return Json(false);

            }
            return Json(true);
        }

        public JsonResult CreateCommentToReview(AllReviewsViewModel allReviewsViewModel)
        {
            var succeeded = false;

            if (!ModelState.IsValid)
            {
                return Json(allReviewsViewModel);
            }

            try
            {
                using (var context = new rkDBContext())
                {
                    var userId = (Guid)Session["userId"];

                    if (context.Users.Any(x => x.Id == userId))
                    {
                        var user = context.Users.SingleOrDefault(x => x.Id == userId);

                        var newCommentToReview = new CommentToReview
                        {
                            Id = Guid.NewGuid(),
                            ReviewId = allReviewsViewModel.ReviewId,
                            UserId = (Guid)userId,
                            Comment = allReviewsViewModel.CommentToAdd,
                            CreatedDate = DateTime.Now
                        };
                        allReviewsViewModel.CreatedBy = user?.Username;
                        allReviewsViewModel.CreatedDate = DateTime.Now.ToShortDateString();

                        context.CommentToReviews.Add(newCommentToReview);
                        succeeded = true;
                        context.SaveChanges();
                    }
                    else
                    {
                        return Json(new { succeeded });
                    }
                }
            }
            catch
            {
                return Json(new { succeeded });
            }
            return Json(new { succeeded, allReviewsViewModel });
        }

        [HttpPost]
        public JsonResult LikeOrDislikeReview(int likeOrDislike, Guid reviewId)
        {
            var succeeded = 0;
            var userId = (Guid)Session["userId"];

            if (likeOrDislike == 1)
            {
                try
                {
                    using (var context = new rkDBContext())
                    {
                        if (!context.Users.Any(x => x.Id == userId))
                        {
                            succeeded = 3;
                            return Json(succeeded);
                        }

                        var isValid = context.UserToReviews.Any(x => x.UserId == userId && x.ReviewId == reviewId);
                        var userToReview =
                            context.UserToReviews.SingleOrDefault(x => x.UserId == userId && x.ReviewId == reviewId);

                        if (!isValid)
                        {
                            succeeded = 1;
                            var userHasLiked = new UserToReview
                            {
                                Id = Guid.NewGuid(),
                                UserId = userId,
                                ReviewId = reviewId,
                                HasLiked = true
                            };

                            var reviewToLike = context.Reviews.SingleOrDefault(x => x.Id == reviewId);

                            if (reviewToLike != null) reviewToLike.LikeCount++;

                            context.UserToReviews.Add(userHasLiked);
                            context.SaveChanges();

                        }
                        else if (userToReview != null && userToReview.HasLiked != true)
                        {
                            succeeded = 1;
                            var reviewToLike = context.Reviews.SingleOrDefault(x => x.Id == reviewId);

                            if (reviewToLike != null) reviewToLike.LikeCount++;
                            userToReview.HasLiked = true;

                            context.SaveChanges();
                        }
                        else
                        {
                            return Json(succeeded);
                        }

                    }
                }
                catch
                {
                    return Json(succeeded);
                }
                return Json(succeeded);
            }

            try
            {
                using (var context = new rkDBContext())
                {
                    if (!context.Users.Any(x => x.Id == userId))
                    {
                        succeeded = 3;
                        return Json(succeeded);
                    }

                    var isValid = context.UserToReviews.Any(x => x.UserId == userId && x.ReviewId == reviewId);
                    var userToReview =
                        context.UserToReviews.SingleOrDefault(x => x.UserId == userId && x.ReviewId == reviewId);

                    if (userToReview != null && !isValid)
                    {
                        succeeded = 2;

                        var userHasLiked = new UserToReview
                        {
                            Id = Guid.NewGuid(),
                            UserId = userId,
                            ReviewId = reviewId,
                            HasLiked = true
                        };
                        var reviewToDislike = context.Reviews.SingleOrDefault(x => x.Id == reviewId);

                        if (reviewToDislike != null) reviewToDislike.DislikeCount++;
                        userToReview.HasLiked = true;

                        context.SaveChanges();
                    }
                    else if (userToReview != null && userToReview.HasLiked != true)
                    {
                        succeeded = 2;

                        var reviewToDislike = context.Reviews.SingleOrDefault(x => x.Id == reviewId);

                        if (reviewToDislike != null) reviewToDislike.DislikeCount++;
                        userToReview.HasLiked = true;

                        context.SaveChanges();
                    }
                    else
                    {
                        return Json(succeeded);
                    }
                }
            }
            catch
            {
                return Json(succeeded);
            }
            return Json(succeeded);
        }

        [HttpPost]
        public JsonResult RateReview(int checkedValue, Guid reviewId)
        {
            var succeeded = checkedValue;
            var userId = (Guid)Session["userId"];

            try
            {
                using (var context = new rkDBContext())
                {
                    if (!context.Users.Any(x => x.Id == userId))
                    {
                        succeeded = 6;
                        return Json(new { succeeded });
                    }

                    var isValid = context.UserToReviews.Any(x => x.UserId == userId && x.ReviewId == reviewId);
                    var userToReview =
                        context.UserToReviews.SingleOrDefault(x => x.UserId == userId && x.ReviewId == reviewId);

                    if (isValid && userToReview != null)
                    {
                        userToReview.Rating = checkedValue;

                        context.SaveChanges();
                    }
                    else if (!isValid)
                    {
                        var newUserToReview = new UserToReview
                        {
                            Id = Guid.NewGuid(),
                            UserId = userId,
                            ReviewId = reviewId,
                            HasLiked = false,
                            Rating = checkedValue
                        };

                        context.UserToReviews.Add(newUserToReview);
                        context.SaveChanges();
                    }
                    else
                    {
                        succeeded = 0;
                    }
                    return Json(new
                    {
                        succeeded,
                        rating = CalculateReviewRating(context.UserToReviews.Where(x => x.ReviewId == reviewId).ToList())
                    });

                }
            }
            catch
            {
                succeeded = 6;
                return Json(new { succeeded });
            }
        }

        public decimal? CalculateReviewRating(List<UserToReview> userToReviewList)
        {
            var reviewCount = userToReviewList.Count;

            var totalRating = reviewCount != 0 ? userToReviewList.Sum(x => x.Rating) / reviewCount : 0;

            return totalRating;
        }

        [HttpPost]
        public JsonResult SortReviews(int sortValue)
        {
            var succeeded = false;

            var reviews = new List<AllReviewsViewModel>();

            try
            {
                using (var context = new rkDBContext())
                {
                    succeeded = true;

                    var reviewsToShow = context.Reviews.ToList();

                    reviews.AddRange(from review in reviewsToShow
                                     let user = context.Users.SingleOrDefault(x => x.Id == review.CreatorUserId)
                                     select new AllReviewsViewModel
                                     {
                                         Title = review.Title,
                                         Type = review.Type,
                                         ReviewId = review.Id,
                                         Likes = review.LikeCount,
                                         UserRating = review.UserRating,
                                         DisLikes = review.DislikeCount,
                                         Description = review.Description,
                                         CreatedBy = review.User.Username,
                                         TotalRating = review.ReviewRating,
                                         CreatorUserId = review.CreatorUserId,
                                         CreatedDate = review.CreatedDate.ToShortDateString(),
                                         HasProfilePicture = ConfirmProfilePicture(user.Id)
                                     });
                }
            }
            catch
            {
                return Json(new { succeeded, reviews });
            }
            switch (sortValue)
            {
                case 1:
                    reviews = reviews.OrderBy(x => x.Title).ToList();
                    break;
                case 2:
                    reviews = reviews.OrderBy(x => x.UserRating).ToList();
                    break;
                case 3:
                    reviews = reviews.OrderBy(x => x.Likes).ToList();
                    break;
                case 4:
                    reviews = reviews.OrderBy(x => x.DisLikes).ToList();
                    break;
                case 5:
                    reviews = reviews.OrderBy(x => x.CreatedDate).ToList();
                    break;
                case 6:
                    reviews = reviews.OrderBy(x => x.Type).ToList();
                    break;
            }
            return Json(new { succeeded, reviews });
        }

        [HttpPost]
        public JsonResult SearchReviews(string searchValue)
        {
            var succeeded = false;

            var reviews = new List<AllReviewsViewModel>();

            try
            {
                using (var context = new rkDBContext())
                {
                    if (!context.Reviews.Any(x => x.Title == searchValue)) return Json(new { reviews, succeeded });

                    succeeded = true;

                    var reviewsToShow = context.Reviews.Where(x => x.Title == searchValue).ToList();

                    reviews.AddRange(from review in reviewsToShow
                                     let user = context.Users.SingleOrDefault(x => x.Id == review.CreatorUserId)
                                     select new AllReviewsViewModel
                                     {
                                         Title = review.Title,
                                         Type = review.Type,
                                         ReviewId = review.Id,
                                         Likes = review.LikeCount,
                                         UserRating = review.UserRating,
                                         DisLikes = review.DislikeCount,
                                         Description = review.Description,
                                         CreatedBy = review.User.Username,
                                         TotalRating = review.ReviewRating,
                                         CreatorUserId = review.CreatorUserId,
                                         CreatedDate = review.CreatedDate.ToShortDateString(),
                                         HasProfilePicture = ConfirmProfilePicture(user.Id)
                                     });

                }
            }
            catch
            {
                return Json(new { reviews, succeeded });
            }
            return Json(new { reviews, succeeded });
        }

        public JsonResult RefreshReviews(string value)
        {
            if (!string.IsNullOrEmpty(value)) return Json(null);
            const bool succeeded = true;

            var reviews = new List<AllReviewsViewModel>();

            try
            {
                using (var context = new rkDBContext())
                {
                    var reviewsToShow = context.Reviews.ToList();

                    reviews.AddRange(from review in reviewsToShow
                                     let user = context.Users.SingleOrDefault(x => x.Id == review.CreatorUserId)
                                     select new AllReviewsViewModel
                                     {
                                         Title = review.Title,
                                         Type = review.Type,
                                         ReviewId = review.Id,
                                         Likes = review.LikeCount,
                                         UserRating = review.UserRating,
                                         DisLikes = review.DislikeCount,
                                         Description = review.Description,
                                         CreatedBy = review.User.Username,
                                         TotalRating = review.ReviewRating,
                                         CreatorUserId = review.CreatorUserId,
                                         CreatedDate = review.CreatedDate.ToShortDateString(),
                                         HasProfilePicture = ConfirmProfilePicture(user.Id)
                                     });
                }
            }
            catch
            {
                return Json(new { succeeded, reviews });
            }
            return Json(new { succeeded, reviews });
        }

        [HttpPost]
        public JsonResult GetReviewRatings(Guid reviewId)
        {
            int succeeded = 0;

            List<RatingViewModel> ratingsToReview = null;

            try
            {
                using (var context = new rkDBContext())
                {
                    var userToReviewList = context.UserToReviews.Where(x => x.ReviewId == reviewId).ToList();

                    ratingsToReview = userToReviewList.Select(userToReview => new RatingViewModel
                    {
                        Username = userToReview.User.Username,
                        Rating = userToReview.Rating
                    }).ToList();
                }
            }
            catch
            {
                succeeded = 2;
                return Json(new { succeeded, ratingsToReview });
            }

            if (ratingsToReview?.Count > 0)
            {
                succeeded = 1;
            }


            return Json(new { succeeded, ratingsToReview });

        }

        [HttpPost]
        public ActionResult UploadPicture(Guid id)
        {

            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                    var fileName = id.ToString() + ".png";

                    var filePath = Path.Combine(Server.MapPath("~/Images/ReviewImages/"), fileName);

                    file.SaveAs(filePath);
                }
            }

            return RedirectToAction("Index", "Review");
        }

        public bool ConfirmProfilePicture(Guid userId)
        {
            var hasProfilePic = System.IO.File.Exists(Server.MapPath("~/Images/ProfileImages/" + userId + ".png"));

            return hasProfilePic;
        }

    }
}