﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;

namespace Infrastructure.Membership
{
    [Alias("UserLogins")]
    public class UserLogin
    {
        /// <summary>
        /// The primary key
        /// </summary>
        public virtual Guid Id { get; set; }

        /// <summary>
        /// The user this is associated to
        /// </summary>
        public virtual Guid UserId { get; set; }

        /// <summary>
        /// The login provider name
        /// </summary>
        public virtual string LoginProvider { get; set; }

        /// <summary>
        /// The login key for the provider
        /// </summary>
        public virtual string LoginKey { get; set; }
    }
}
