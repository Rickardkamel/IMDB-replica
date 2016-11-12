using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ImdbReplica.Models
{
    public class RegisterViewModel : LoginViewModel
    {
        [Required(ErrorMessage = "Need to fill in Firstname!")]
        [StringLength(150)]
        public string Firstname { get; set; }

        [Required(ErrorMessage = "Need to fill in Lastname!")]
        [StringLength(250)]
        public string Lastname { get; set; }

        [Required(ErrorMessage = "Need to fill in Email-address!")]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}