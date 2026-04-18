using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.SqlServer.Persistence
{
    public class ApplicationRoles : IdentityRole<Guid>
    {
        public string? FullName { get; set; }

    }
}
