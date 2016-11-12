using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ImdbReplica.Models
{
    public class ReviewViewModel
    {
        public Guid ReviewId { get; set; }

        public Guid CreatorUserId { get; set; }

        [Required(ErrorMessage = "Your review requires a title!")]
        [StringLength(maximumLength: 50, MinimumLength = 1)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Your review requires a description!")]
        public string Description { get; set; }

        public string CreatedDate { get; set; }

        [Required(ErrorMessage = "You forgot to give your review a rating!")]
        public int UserRating { get; set; }

        public IEnumerable<SelectListItem> RatingValues => new[]
        {
            new SelectListItem {Value = "1", Text = "1"},
            new SelectListItem {Value = "2", Text = "2"},
            new SelectListItem {Value = "3", Text = "3"},
            new SelectListItem {Value = "4", Text = "4"},
            new SelectListItem {Value = "5", Text = "5"}
        };

        [Required(ErrorMessage = "You need to choose type!")]
        public string Type { get; set; }

        public IEnumerable<SelectListItem> Values => new[]
        {
            new SelectListItem {Value = "Book", Text = "Book"},
            new SelectListItem {Value = "Movie", Text = "Movie"},
            new SelectListItem {Value = "Game", Text = "Game"},
        };

        public decimal TotalRating { get; set; }
        public string ReviewImagePath { get; set; }
    }
}
