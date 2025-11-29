using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.ComponentModel.DataAnnotations;

namespace Chatup.Models
{
    public class User
    {
        [Key]
        private readonly Guid Id = Guid.NewGuid();

        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string About { get; set; }

        [Required]
        [Phone]
        [StringLength(20, MinimumLength =10)]
        public string PhoneNumber { get; set; }

        [Required, ]
        [StringLength(50, ErrorMessage = "Username cannot be longer than 50 characters.")]
        public string Username { get; set; }

        [Required]
         public string Password { get; set; }

        
        public UserStatus Status { get; set; }
        public DateTime LastSeen { get; set; }


        public bool IsVerified { get; set; } = false;
        public string? ProfilePictureUrl { get; set; }


        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }



        // Privacy Settings
        public PrivacySetting LastSeenPrivacy { get; set; } = PrivacySetting.Everyone;
        public PrivacySetting ProfilePicturePrivacy { get; set; } = PrivacySetting.Everyone;
        public PrivacySetting AboutPrivacy { get; set; } = PrivacySetting.Everyone;


        // Account Status
        public bool IsActive { get; set; } = true;
        public bool IsDisabled { get; set; } = false;
        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public enum UserStatus
        {
            Online = 0,
            Offline = 1
        }

        public enum PrivacySetting
        {
            Everyone = 0,
            MyContacts = 1,
            Nobody = 2
        }





    }
}
