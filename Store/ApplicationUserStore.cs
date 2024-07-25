using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using WorkFlowWeb.Data.DataAccess;
using WorkFlowWeb.Models;


namespace WorkFlowWeb.Store {
public class ApplicationUserStore : UserStore<ApplicationUser, IdentityRole, ApplicationDbContext>
    {
        private readonly ILogger<ApplicationUserStore> _logger;
        public ApplicationUserStore(ApplicationDbContext context, ILogger<ApplicationUserStore> logger)
        : base(context)
    {
            _logger = logger;
        }

    public override async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        user.ApplicationUserId = GenerateApplicationUserId();
        user.Created = DateTime.Now;
        user.Modified = DateTime.Now;
        return await base.CreateAsync(user, cancellationToken);
    }

    public override async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        user.Modified = DateTime.Now;
         _logger.LogInformation("User updated at {Time}", DateTime.Now);
            return await base.UpdateAsync(user, cancellationToken);
    }

    private string GenerateApplicationUserId()
    {
        var lastUser = Users.OrderByDescending(u => u.ApplicationUserId).FirstOrDefault();
        int lastUserId = 0;

        if (lastUser != null && !string.IsNullOrEmpty(lastUser.ApplicationUserId))
        {
            if (int.TryParse(lastUser.ApplicationUserId.Substring(1), out int numericId))
            {
                lastUserId = numericId;
            }
        }

        int newUserId = lastUserId + 1;
        return $"u{newUserId:D9}";
    }
}
}
