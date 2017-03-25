using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebUI.Models
{
    public class RegisterViewModel
    {
        public RegisterViewModel()
        {
            StorageQueueName = "seat-events";
        }

        [Required]
        [Display(Name = "Room Name")]
        public string RoomName { get; set; }

        [Required]
        [Display(Name = "Storage Queue Connection String")]
        public string StorageQueueConnectionString { get; set; }

        [Required]
        [Display(Name = "Storage Queue Name")]
        public string StorageQueueName { get; set; }
    }
}