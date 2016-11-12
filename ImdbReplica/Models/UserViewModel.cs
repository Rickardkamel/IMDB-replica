using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImdbReplica.Models
{
    public class UserViewModel
    {
        public UserViewModel()
        {
            ReviewViewModelList = new List<ReviewViewModel>();
        }
        public Guid Id { get; set; }

        public List<ReviewViewModel> ReviewViewModelList { get; set; }

        [Required(ErrorMessage = "You need to fill in Username(more than 5 letters)!")]
        [StringLength(maximumLength: 50, MinimumLength = 5)]
        public string Username { get; set; }

        //public string Password { get; set; }
        //public string Salt { get; set; }

        [Required(ErrorMessage = "You need to fill in Email!")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required(ErrorMessage = "You need to fill in Firstname!")]
        [StringLength(maximumLength: 50, MinimumLength = 2)]
        public string Firstname { get; set; }

        [Required(ErrorMessage = "You need to fill in Lastname!")]
        [StringLength(maximumLength: 50, MinimumLength = 2)]
        public string Lastname { get; set; }

        public string ProfileImagePath { get; set; }
    }
}
