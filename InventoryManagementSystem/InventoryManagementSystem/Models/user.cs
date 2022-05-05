//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace InventoryManagementSystem.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web;

    public partial class user
    {
        //Database Constructors
        public byte user_id { get; set; }
        [Required(ErrorMessage = "Please Enter Username")]
        public string user_username { get; set; }
        [Required(ErrorMessage = "Please Enter Password")]
        public string user_pass { get; set; }
        [Required(ErrorMessage = "Please Enter Role")]
        public string user_role { get; set; }
        [Required(ErrorMessage = "Please Enter Name")]
        public string user_name { get; set; }
        public string user_address { get; set; }
        [Required(ErrorMessage = "Please Enter Email")]
        public string user_email { get; set; }
        [Required(ErrorMessage = "Please Enter 11 Digit Contact Number")]
        [StringLength(11, MinimumLength = 10, ErrorMessage = "The Number Entered is either too long or too short")]
        [MaxLength(11)]
        public string user_contact { get; set; }
        public string user_timestamp { get; set; }
        public byte[] user_img { get; set; }
        public string user_nickname { get; set; }

        //Local Constructors
        public HttpPostedFileBase ImageFile { get; set; }

        [Required(ErrorMessage = "Please Enter Nickname")]
        public string LoginErrorMessage { get; set; }
    }
}