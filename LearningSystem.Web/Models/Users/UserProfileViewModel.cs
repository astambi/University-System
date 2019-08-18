﻿namespace LearningSystem.Web.Models.Users
{
    using System.Collections.Generic;
    using LearningSystem.Services.Models.Users;

    public class UserProfileViewModel
    {
        public UserProfileServiceModel User { get; set; }

        public IEnumerable<string> Roles { get; set; }
    }
}
