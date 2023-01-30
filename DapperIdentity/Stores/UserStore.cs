﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Threading;

using Dapper.Contrib.Extensions;
using Dapper;
using DapperRepository;
using IdentityUser = DapperIdentity.Models.CustomIdentityUser;
using IdentityRole = DapperIdentity.Models.CustomIdentityRole;


namespace DapperIdentity.Stores
{
    public class UserStore : IUserStore<IdentityUser>,
                             IUserEmailStore<IdentityUser>,
                             IUserLoginStore<IdentityUser>,
                             IUserPasswordStore<IdentityUser>,
                             IUserPhoneNumberStore<IdentityUser>,
                             IUserRoleStore<IdentityUser>,
                             IQueryableUserStore<IdentityUser>,
                             IUserSecurityStampStore<IdentityUser>
                             //IUserClaimStore
                             //IUserTwoFactorStore<>,
                             //IUserLockoutStore<>
                            
    {
        private IRepository<IdentityUser> repository;

        public UserStore(IRepository<IdentityUser> userRepository)
        {
            repository = userRepository;
        }

        IQueryable<IdentityUser> IQueryableUserStore<IdentityUser>.Users => this.GetUsers().Result; //repository.FindAllAsync().Result.AsQueryable();

        private async Task<IQueryable<IdentityUser>> GetUsers()
        {
            var userDictionary = new Dictionary<string, IdentityUser>();

            using (var connection = repository.DbConnection)
            {
                var sql = @"SELECT * FROM IdentityUser as U LEFT JOIN IdentityRole as R ON U.Id=R.Id";
                var list = await connection.QueryAsync<IdentityUser, IdentityRole, IdentityUser>(
                    sql,
                    (user, role) =>
                    {
                        //IdentityUser user;

                        if (!userDictionary.TryGetValue(user.Id, out var foundUser))
                        {
                            foundUser = user;
                            foundUser.Roles = new List<IdentityRole>();
                            userDictionary.Add(user.Id, foundUser);
                        }
                        foundUser.Roles.Add(role);
                        return foundUser;
                    },
                    splitOn: "Id");

                //.ToList();
                return list.Distinct().AsQueryable();
            }
        }

        public Task AddLoginAsync(IdentityUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user.Id is null)
            {
                user.Id = Guid.NewGuid().ToString();
            }
            var something = await repository.InsertAsync(user); // InsertAsync(user);
            
            return IdentityResult.Success;
        }

        //using (var connection = new SqlConnection(_connectionString))
        //{
        //    await connection.OpenAsync(cancellationToken);
        //    user.Id = await connection.QuerySingleAsync<int>($@"INSERT INTO [IdentityUser] ([UserName], [NormalizedUserName], [Email],
        //        [NormalizedEmail], [EmailConfirmed], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled])
        //        VALUES (@{nameof(IdentityUser.UserName)}, @{nameof(IdentityUser.NormalizedUserName)}, @{nameof(IdentityUser.Email)},
        //        @{nameof(IdentityUser.NormalizedEmail)}, @{nameof(IdentityUser.EmailConfirmed)}, @{nameof(IdentityUser.PasswordHash)},
        //        @{nameof(IdentityUser.PhoneNumber)}, @{nameof(IdentityUser.PhoneNumberConfirmed)}, @{nameof(IdentityUser.TwoFactorEnabled)});
        //        SELECT CAST(SCOPE_IDENTITY() as int)", user);
        //}


        public async Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            await repository.DeleteAsync(user);
            return IdentityResult.Success;

        }

        public void Dispose()
        {
            //nothing to dispose
            //throw new NotImplementedException();
        }

        public async Task<IdentityUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            
            var dynamicParams = new DynamicParameters();
            dynamicParams.Add("NormalizedEmail", normalizedEmail, System.Data.DbType.String);
            string sql = @"SELECT * FROM IdentityUser WHERE NormalizedEmail = @NormalizedEmail";
                        return await repository.SelectFirstOrDefaultAsync(sql, dynamicParams);
        }

        public async Task<IdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await repository.FindByIDAsync(userId);
        }

        public Task<IdentityUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IdentityUser foundUser;
            using (var conn = repository.DbConnection)
            { 
                var dynamicParams = new DynamicParameters();
                dynamicParams.Add("UserName", normalizedUserName, System.Data.DbType.String);
                foundUser = await conn.QueryFirstOrDefaultAsync<IdentityUser>("SELECT * FROM IdentityUser WHERE NormalizedUserName = @UserName", dynamicParams);
            }
            return foundUser;            
        }

        public Task<string> GetEmailAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult<bool>(user.EmailConfirmed);
            //throw new NotImplementedException();
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedEmailAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPasswordHashAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult<string>(user.PasswordHash);
        }

        public Task<string> GetPhoneNumberAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult<string>(user.PhoneNumber);
            //throw new NotImplementedException();
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult<string>(user.Id);  //@chad
            //throw new NotImplementedException();
        }

        //public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}

        public Task<string> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult<string>(user.UserName);
            //throw new NotImplementedException();
        }

        public Task<bool> HasPasswordAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult<bool>(user.PasswordHash != null);
        }

        public Task RemoveLoginAsync(IdentityUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetEmailAsync(IdentityUser user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.FromResult(0);
        }

        public Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task SetNormalizedEmailAsync(IdentityUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.FromResult(0);
        }

        public Task SetNormalizedUserNameAsync(IdentityUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.FromResult(0);
        }

        public Task SetPasswordHashAsync(IdentityUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
            //throw new NotImplementedException();
        }

        public Task SetPhoneNumberAsync(IdentityUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            user.PhoneNumber = phoneNumber;
            return Task.FromResult(0);
        }

        public Task SetPhoneNumberConfirmedAsync(IdentityUser user, bool confirmed, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetUserNameAsync(IdentityUser user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.FromResult(0);
        }

        public async Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await repository.UpdateAsync(user);
            return IdentityResult.Success;
        }

        Task IUserRoleStore<IdentityUser>.AddToRoleAsync(IdentityUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        async Task<IList<string>> IUserRoleStore<IdentityUser>.GetRolesAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            string sql = "SELECT Name FROM IdentityRole WHERE id = @id";
            var dynamicParams = new DynamicParameters();
            dynamicParams.Add("id", user.Id, System.Data.DbType.String);
            var result = await repository.DbConnection.QueryAsync<string>(sql, dynamicParams);
            return result.ToList<string>();
        }

        Task<IList<IdentityUser>> IUserRoleStore<IdentityUser>.GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task<bool> IUserRoleStore<IdentityUser>.IsInRoleAsync(IdentityUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Task IUserRoleStore<IdentityUser>.RemoveFromRoleAsync(IdentityUser user, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Set the security stamp for the user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="stamp"></param>
        /// <returns></returns>
        public Task SetSecurityStampAsync(IdentityUser user, string stamp, CancellationToken cancellationToken)
        {
            //ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            user.SecurityStamp = stamp;
            //UpdateAsync(user, cancellationToken);
            return Task.FromResult(0);
        }
        
        /// <summary>
        ///     Get the security stamp for a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<string> GetSecurityStampAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            //ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }
            return Task.FromResult(user.SecurityStamp);
        }


        //public async Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    cancellationToken.ThrowIfCancellationRequested();

        //    using (var connection = new SqlConnection(_connectionString))
        //    {
        //        await connection.OpenAsync(cancellationToken);
        //        user.Id = await connection.QuerySingleAsync<int>($@"INSERT INTO [IdentityUser] ([UserName], [NormalizedUserName], [Email],
        //        [NormalizedEmail], [EmailConfirmed], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled])
        //        VALUES (@{nameof(IdentityUser.UserName)}, @{nameof(IdentityUser.NormalizedUserName)}, @{nameof(IdentityUser.Email)},
        //        @{nameof(IdentityUser.NormalizedEmail)}, @{nameof(IdentityUser.EmailConfirmed)}, @{nameof(IdentityUser.PasswordHash)},
        //        @{nameof(IdentityUser.PhoneNumber)}, @{nameof(IdentityUser.PhoneNumberConfirmed)}, @{nameof(IdentityUser.TwoFactorEnabled)});
        //        SELECT CAST(SCOPE_IDENTITY() as int)", user);
        //    }

        //    return IdentityResult.Success;
        //}

        //public async Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    cancellationToken.ThrowIfCancellationRequested();

        //    using (var connection = new SqlConnection(_connectionString))
        //    {
        //        await connection.OpenAsync(cancellationToken);
        //        await connection.ExecuteAsync($"DELETE FROM [IdentityUser] WHERE [Id] = @{nameof(IdentityUser.Id)}", user);
        //    }

        //    return IdentityResult.Success;
        //}

        //public async Task<IdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        //{
        //    cancellationToken.ThrowIfCancellationRequested();

        //    using (var connection = new SqlConnection(_connectionString))
        //    {
        //        await connection.OpenAsync(cancellationToken);
        //        return await connection.QuerySingleOrDefaultAsync<IdentityUser>($@"SELECT * FROM [IdentityUser]
        //        WHERE [Id] = @{nameof(userId)}", new { userId });
        //    }
        //}

        //public async Task<IdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        //{
        //    cancellationToken.ThrowIfCancellationRequested();

        //    using (var connection = new SqlConnection(_connectionString))
        //    {
        //        await connection.OpenAsync(cancellationToken);
        //        return await connection.QuerySingleOrDefaultAsync<IdentityUser>($@"SELECT * FROM [IdentityUser]
        //        WHERE [NormalizedUserName] = @{nameof(normalizedUserName)}", new { normalizedUserName });
        //    }
        //}

        //public Task<string> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    return Task.FromResult(user.NormalizedUserName);
        //}

        //public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    return Task.FromResult(user.Id.ToString());
        //}

        //public Task<string> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    return Task.FromResult(user.UserName);
        //}

        //public Task SetNormalizedUserNameAsync(IdentityUser user, string normalizedName, CancellationToken cancellationToken)
        //{
        //    user.NormalizedUserName = normalizedName;
        //    return Task.FromResult(0);
        //}

        //public Task SetUserNameAsync(IdentityUser user, string userName, CancellationToken cancellationToken)
        //{
        //    user.UserName = userName;
        //    return Task.FromResult(0);
        //}
        /*
        public async Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                await connection.ExecuteAsync($@"UPDATE [IdentityUser] SET
                [UserName] = @{nameof(IdentityUser.UserName)},
                [NormalizedUserName] = @{nameof(IdentityUser.NormalizedUserName)},
                [Email] = @{nameof(IdentityUser.Email)},
                [NormalizedEmail] = @{nameof(IdentityUser.NormalizedEmail)},
                [EmailConfirmed] = @{nameof(IdentityUser.EmailConfirmed)},
                [PasswordHash] = @{nameof(IdentityUser.PasswordHash)},
                [PhoneNumber] = @{nameof(IdentityUser.PhoneNumber)},
                [PhoneNumberConfirmed] = @{nameof(IdentityUser.PhoneNumberConfirmed)},
                [TwoFactorEnabled] = @{nameof(IdentityUser.TwoFactorEnabled)}
                WHERE [Id] = @{nameof(IdentityUser.Id)}", user);
            }

            return IdentityResult.Success;
        }
        */


        //public Task<string> GetEmailAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    return Task.FromResult(user.Email);
        //}

        //public Task<bool> GetEmailConfirmedAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    return Task.FromResult(user.EmailConfirmed);
        //}

        //public Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed, CancellationToken cancellationToken)
        //{
        //    user.EmailConfirmed = confirmed;
        //    return Task.FromResult(0);
        //}
        /*
        public async Task<IdentityUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync(cancellationToken);
                return await connection.QuerySingleOrDefaultAsync<IdentityUser>($@"SELECT * FROM [IdentityUser]
                WHERE [NormalizedEmail] = @{nameof(normalizedEmail)}", new { normalizedEmail });
            }
        }
        */
        //public Task<string> GetNormalizedEmailAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    return Task.FromResult(user.NormalizedEmail);
        //}

        //Task<string> IUserEmailStore<IdentityUser>.GetNormalizedEmailAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}


        //public Task SetNormalizedEmailAsync(IdentityUser user, string normalizedEmail, CancellationToken cancellationToken)
        //{
        //    user.NormalizedEmail = normalizedEmail;
        //    return Task.FromResult(0);
        //}

        //public Task SetPhoneNumberAsync(IdentityUser user, string phoneNumber, CancellationToken cancellationToken)
        //{
        //    user.PhoneNumber = phoneNumber;
        //    return Task.FromResult(0);
        //}

        //public Task<string> GetPhoneNumberAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    return Task.FromResult(user.PhoneNumber);
        //}

        //public Task<bool> GetPhoneNumberConfirmedAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    return Task.FromResult(user.PhoneNumberConfirmed);
        //}

        //public Task SetPhoneNumberConfirmedAsync(IdentityUser user, bool confirmed, CancellationToken cancellationToken)
        //{
        //    user.PhoneNumberConfirmed = confirmed;
        //    return Task.FromResult(0);
        //}

        //public Task SetTwoFactorEnabledAsync(IdentityUser user, bool enabled, CancellationToken cancellationToken)
        //{
        //    user.TwoFactorEnabled = enabled;
        //    return Task.FromResult(0);
        //}

        //public Task<bool> GetTwoFactorEnabledAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    return Task.FromResult(user.TwoFactorEnabled);
        //}

        //public Task SetPasswordHashAsync(IdentityUser user, string passwordHash, CancellationToken cancellationToken)
        //{
        //    user.PasswordHash = passwordHash;
        //    return Task.FromResult(0);
        //}

        //public Task<string> GetPasswordHashAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    return Task.FromResult(user.PasswordHash);
        //}

        //public Task<bool> HasPasswordAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    return Task.FromResult(user.PasswordHash != null);
        //}

        //public void Dispose()
        //{
        //    // Nothing to dispose.
        //}

        //Task<IdentityUser> IUserEmailStore<IdentityUser>.FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<string> IUserEmailStore<IdentityUser>.GetEmailAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<bool> IUserEmailStore<IdentityUser>.GetEmailConfirmedAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}



        //Task IUserEmailStore<IdentityUser>.SetEmailAsync(IdentityUser user, string email, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}

        //Task IUserEmailStore<IdentityUser>.SetEmailConfirmedAsync(IdentityUser user, bool confirmed, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}

        //Task IUserEmailStore<IdentityUser>.SetNormalizedEmailAsync(IdentityUser user, string normalizedEmail, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<IdentityResult> IUserStore<IdentityUser>.CreateAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<IdentityResult> IUserStore<IdentityUser>.DeleteAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<IdentityUser> IUserStore<IdentityUser>.FindByIdAsync(string userId, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<IdentityUser> IUserStore<IdentityUser>.FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<string> IUserStore<IdentityUser>.GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<string> IUserStore<IdentityUser>.GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<string> IUserStore<IdentityUser>.GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}

        //Task IUserStore<IdentityUser>.SetNormalizedUserNameAsync(IdentityUser user, string normalizedName, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}

        //Task IUserStore<IdentityUser>.SetUserNameAsync(IdentityUser user, string userName, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}

        //Task<IdentityResult> IUserStore<IdentityUser>.UpdateAsync(IdentityUser user, CancellationToken cancellationToken)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
