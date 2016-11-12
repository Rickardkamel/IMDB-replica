using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ImdbReplica.Models
{
    public class PasswordViewModel
    {
        [Required(ErrorMessage = "Write your new password.")]
        [StringLength(maximumLength: 100, MinimumLength = 6, ErrorMessage = "The field password must be between 6 - 100 characters")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Write your current password.")]
        [StringLength(maximumLength: 100, MinimumLength = 6, ErrorMessage = "The field password must be between 6 - 100 characters")]
        public string OldPassword { get; set; }
    }
}