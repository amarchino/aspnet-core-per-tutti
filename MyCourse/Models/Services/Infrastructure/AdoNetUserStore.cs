using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MyCourse.Models.Entities;

namespace MyCourse.Models.Services.Infrastructure
{
    public class AdoNetUserStore : IUserStore<ApplicationUser>
    {
        private readonly IDatabaseAccessor db;
        public AdoNetUserStore(IDatabaseAccessor db)
        {
            this.db = db;
        }

        #region Implementation of IUserStore<ApplicationUser>
        public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            int affectedRows = await db.CommandAsync($"INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, FullName) VALUES ({user.Id}, {user.UserName}, {user.NormalizedUserName}, {user.Email}, {user.NormalizedEmail}, {user.EmailConfirmed}, {user.PasswordHash}, {user.SecurityStamp}, {user.ConcurrencyStamp}, {user.PhoneNumber}, {user.PhoneNumberConfirmed}, {user.TwoFactorEnabled}, {user.LockoutEnd}, {user.LockoutEnabled}, {user.AccessFailedCount}, {user.FullName})");
            if(affectedRows > 0)
            {
                return IdentityResult.Success;
            }
            var err = new IdentityError { Description = "Could not insert user" };
            return IdentityResult.Failed();
        }

        public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            int affectedRows = await db.CommandAsync($"DELETE FROM AspNetUsers WHERE Id={user.Id}");
            if (affectedRows > 0)
            {
                return IdentityResult.Success;
            }
            var error = new IdentityError { Description = "User could not be found" };
            return IdentityResult.Failed(error);
        }

        public void Dispose()
        {
            // Non implementato
        }

        public async Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            DataSet dataSet = await db.QueryAsync($"SELECT * FROM AspNetUsers WHERE Id={userId}");
            if (dataSet.Tables[0].Rows.Count == 0)
            {
                return null;
            }
            return ApplicationUser.FromDataRow(dataSet.Tables[0].Rows[0]);
        }

        public async Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            DataSet dataSet = await db.QueryAsync($"SELECT * FROM AspNetUsers WHERE NormalizedUserName={normalizedUserName}");
            if (dataSet.Tables[0].Rows.Count == 0)
            {
                return null;
            }
            return ApplicationUser.FromDataRow(dataSet.Tables[0].Rows[0]);
        }

        public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }

        public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
        {
            int affectedRows = await db.CommandAsync($"UPDATE AspNetUsers SET UserName={user.UserName}, NormalizedUserName={user.NormalizedUserName}, Email={user.Email}, NormalizedEmail={user.NormalizedEmail}, EmailConfirmed={user.EmailConfirmed}, PasswordHash={user.PasswordHash}, SecurityStamp={user.SecurityStamp}, ConcurrencyStamp={user.ConcurrencyStamp}, PhoneNumber={user.PhoneNumber}, PhoneNumberConfirmed={user.PhoneNumberConfirmed}, TwoFactorEnabled={user.TwoFactorEnabled}, LockoutEnd={user.LockoutEnd}, LockoutEnabled={user.LockoutEnabled}, AccessFailedCount={user.AccessFailedCount}, FullName={user.FullName} WHERE Id={user.Id}");
            if (affectedRows > 0)
            {
                return IdentityResult.Success;
            }
            var error = new IdentityError { Description = "Could not update user" };
            return IdentityResult.Failed(error);
        }
        #endregion
    }
}
