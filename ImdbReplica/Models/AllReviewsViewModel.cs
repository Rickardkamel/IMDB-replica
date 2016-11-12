using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ImdbReplica.Context;

namespace ImdbReplica.Models
{
    public class AllReviewsViewModel
    {
        public AllReviewsViewModel()
        {
            CommentToReviewList = new List<CommentToReview>();
        }
        public Guid ReviewId { get; set; }
        public Guid CreatorUserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CreatedDate { get; set; }
        public int UserRating { get; set; }
        public string Type { get; set; }
        public string CreatedBy { get; set; }
        public int Likes { get; set; }
        public int DisLikes { get; set; }
        public string ReviewImagePath { get; set; }
        public bool HasProfilePicture { get; set; }
        public decimal TotalRating { get; set; }
        public IEnumerable<SelectListItem> RatingValues => new[]
        {
            new SelectListItem {Value = "1", Text = "1"},
            new SelectListItem {Value = "2", Text = "2"},
            new SelectListItem {Value = "3", Text = "3"},
            new SelectListItem {Value = "4", Text = "4"},
            new SelectListItem {Value = "5", Text = "5"}
        };

        [Required(ErrorMessage = "You need to write something in the comment field!")]
        [StringLength(maximumLength: 150, MinimumLength = 3)]
        public string CommentToAdd { get; set; }
        public List<CommentToReview> CommentToReviewList { get; set; }
    }
}