using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImdbReplica.Models
{
    public class LoginViewModel
    {

        [Required(ErrorMessage = "You must submit a username.")]
        [StringLength(maximumLength: 100, MinimumLength = 6, ErrorMessage = "The field Username must be between 6 - 100 characters")]
        public string Username { get; set; }
        [Required(ErrorMessage = "You must submit a password!")]
        [StringLength(maximumLength: 100, MinimumLength = 6, ErrorMessage = "The field password must be between 6 - 100 characters")]
        public string Password { get; set; }
    }
}
