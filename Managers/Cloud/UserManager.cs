﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Security;
using System.Threading.Tasks;

using LittleLogBook.Data.Contracts;
using LittleLogBook.Data.Entities.Cloud;
using LittleLogBook.Data.SqlConnectivity;

namespace LittleLogBook.Data.Managers.Cloud
{
    public class UserManager : ManagerBase, IUserManager
    {
        private readonly IDataHandler _dataHandler;

        internal UserManager(IDataHandler dataHandler, IUser user)
            : base(user)
        {
            _dataHandler = dataHandler;
        }

        public async Task<IUser> GetUserAsync(string emailAddress)
		{
            emailAddress = (emailAddress + "").Trim();

			if (emailAddress.Length == 0)
			{
				throw new ArgumentNullException("The specified email address may not be empty");
			}

            using (var command = _dataHandler.CreateCommand("GetCloudUserByEmailAddress"))
            {
                command.AddParameter("@EmailAddress", emailAddress, DbType.String);
                command.AddParameter("@ViewedByUserId", CurrentUser.CloudUserId, DbType.Guid);

                using (var reader = command.OpenReader())
                {
                    if (await reader.ReadAsync())
                    {
                        return new CloudUser(CurrentUser.CloudUserId, reader);
                    }
                }
            }

			return null;
		}

        public async Task<IEnumerable<IUser>> GetUsersByMemorableWordAsync(string memorableWord)
        {
            return await GetUsersByMemorableWordAsync(_dataHandler, memorableWord, CurrentUser.CloudUserId);
        }

        public static async Task<IEnumerable<IUser>> GetUsersByMemorableWordAsync(string connectionString, string memorableWord, Guid cloudUserId)
        {
            using (var dataHandler = new DataHandler(connectionString))
            {
                return await GetUsersByMemorableWordAsync(dataHandler, memorableWord, cloudUserId);
            }
        }

        private static async Task<IEnumerable<IUser>> GetUsersByMemorableWordAsync(IDataHandler dataHandler, string memorableWord, Guid cloudUserId)
        {
	        memorableWord = (memorableWord + "").Trim();

	        if (memorableWord.Length == 0)
	        {
		        throw new ArgumentNullException("The specified memorable word may not be empty");
	        }

	        using (var command = dataHandler.CreateCommand("GetCloudUsersByMemorableWord"))
	        {
		        command.AddParameter("@MemorableWord", memorableWord, DbType.String);
		        command.AddParameter("@ViewedByUserId", cloudUserId, DbType.Guid);

		        var users = new List<IUser>();
		        
		        using (var reader = command.OpenReader())
		        {
			        while (await reader.ReadAsync())
			        {
				        users.Add(new CloudUser(cloudUserId, reader));
			        }
		        }

		        return users;
	        }
        }

        internal static async Task<IUser> LogInAsync(IDataHandler dataHandler, string username, string password)
        {
            username = (username + "").Trim();
            password = (password + "").Trim();

            if (username.Length == 0 || password.Length == 0)
            {
                throw new ArgumentNullException("Username and password may not be empty");
            }

            using (var command = dataHandler.CreateCommand("GetCloudUserByLogin"))
            {
                command.AddParameter("@EmailAddress", username, DbType.String);
                command.AddParameter("@Password", password, DbType.String);

                using (var reader = command.OpenReader())
                {
                    if (await reader.ReadAsync())
                    {
                        return new CloudUser(reader);
                    }
                }
            }

            throw new SecurityException("Invalid username or password");
        }

        public async Task<IUser> GetUserAsync(Guid userId)
        {
            using (var command = _dataHandler.CreateCommand("GetCloudUserByUserId"))
            {
                command.AddParameter("@CloudUserId", userId, DbType.Guid);
                command.AddParameter("@ViewedByUserId", CurrentUser.CloudUserId, DbType.Guid);

                using (var reader = command.OpenReader())
                {
                    if (await reader.ReadAsync())
                    {
                        return new CloudUser(CurrentUser.CloudUserId, reader);
                    }
                }
            }

            return null;
        }

        public async Task<bool> IsEmailAddressAvailableAsync(string emailAddress)
        {
	        return await IsEmailAddressAvailableAsync(_dataHandler, emailAddress);
        }

        public async Task<bool> IsMemorableWordAvailableAsync(string memorableWord)
        {
	        return await IsMemorableWordAvailableAsync(_dataHandler, memorableWord);
        }

        public async Task<IEnumerable<IUserAudit>> GetUserAuditsAsync(Guid cloudUserId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var returnValues = new List<IUserAudit>();

            using (var command = _dataHandler.CreateCommand("GetCloudUserAudits"))
            {
                command.AddParameter("@CloudUserId", cloudUserId, DbType.Guid);
                command.AddParameter("@DateFrom", dateFrom, DbType.DateTime);
                command.AddParameter("@DateTo", dateTo, DbType.DateTime);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        returnValues.Add(new CloudUserAudit(reader));
                    }
                }
            }

            return returnValues;
        }

        public static async Task<bool> IsEmailAddressAvailableAsync(string connectionString, string emailAddress)
        {
	        using (var dataHandler = new DataHandler(connectionString))
	        {
		        return await IsEmailAddressAvailableAsync(dataHandler, emailAddress);
	        }
        }

        private static async Task<bool> IsEmailAddressAvailableAsync(IDataHandler dataHandler, string emailAddress)
		{
			emailAddress = (emailAddress + "").Trim();

			if (emailAddress.Length == 0)
			{
				throw new ArgumentNullException("The specified email address may not be empty");
			}

            using (var command = dataHandler.CreateCommand("IsCloudEmailAddressAvailable"))
            {
                command.AddParameter("@EmailAddress", emailAddress, DbType.String);

                var result = (await command.ExecuteScalarAsync()) + "";

                return "1".Equals(result) || "True".Equals(result, StringComparison.OrdinalIgnoreCase);
            }
		}

        public static async Task<bool> IsMemorableWordAvailableAsync(string connectionString, string memorableWord)
        {
	        using (var dataHandler = new DataHandler(connectionString))
	        {
		        return await IsMemorableWordAvailableAsync(dataHandler, memorableWord);
	        }
        }
        
        private static async Task<bool> IsMemorableWordAvailableAsync(IDataHandler dataHandler, string memorableWord)
        {
	        memorableWord = (memorableWord + "").Trim();

	        if (memorableWord.Length == 0)
	        {
		        throw new ArgumentNullException("The specified memorable word may not be empty");
	        }

	        using (var command = dataHandler.CreateCommand("IsCloudMemorableWordAvailable"))
	        {
		        command.AddParameter("@MemorableWord", memorableWord, DbType.String);

		        var result = (await command.ExecuteScalarAsync()) + "";

		        return "1".Equals(result) || "True".Equals(result, StringComparison.OrdinalIgnoreCase);
	        }
        }

		public async Task<bool> UpdateUserAsync(IUser user)
		{
            using (var command = _dataHandler.CreateCommand("UpdateCloudUser"))
            {
                command.AddParameter("@CloudUserId", user.CloudUserId, DbType.Guid);
                command.AddParameter("@FirstName", user.FirstName, DbType.String);
                command.AddParameter("@LastName", user.LastName, DbType.String);
                command.AddParameter("@TitleId", (int)user.Title, DbType.Int32);
                command.AddParameter("@ContactNumber", user.ContactNumber, DbType.String);
                command.AddParameter("@PreferredCurrencyIso", user.PreferredCurrencyIso, DbType.String);
                command.AddParameter("@TimezoneId", user.TimezoneId, DbType.String);
                command.AddParameter("@MemorableWord", user.MemorableWord, DbType.String);
                command.AddParameter("@SecurityQuestion1", user.SecurityQuestion1, DbType.String);
                command.AddParameter("@SecurityAnswer1", user.SecurityAnswer1, DbType.String);
                command.AddParameter("@SecurityQuestion2", user.SecurityQuestion2, DbType.String);
                command.AddParameter("@SecurityAnswer2", user.SecurityAnswer2, DbType.String);
                command.AddParameter("@SecurityQuestion3", user.SecurityQuestion3, DbType.String);
                command.AddParameter("@SecurityAnswer3", user.SecurityAnswer3, DbType.String);
                command.AddParameter("@ModifiedByUserId", user.ViewedByUserId, DbType.Guid);

                if ((await command.ExecuteNonQueryAsync()) > 0)
                {
                    ((CloudUser)user).SetInternals(false, false, DateTime.UtcNow, user.ViewedByUserId);

                    return true;
                }
            }

			return false;
		}

		public async Task<bool> RegisterUserAsync(IUser user, string password)
		{
			user.EmailAddress = (user.EmailAddress + "").Trim();

			if (user.EmailAddress.Length == 0 || (password + "").Trim().Length == 0)
			{
				throw new ArgumentNullException("Email address and password may not be empty");
			}

			if ((password + "").Length > 50)
			{
				throw new ArgumentException("The specified new password is too long, it may not be more than 50 characters in length");
			}

            using (var command = _dataHandler.CreateCommand("RegisterCloudUser"))
            {
                command.AddParameter("@CloudUserId", user.CloudUserId, DbType.Guid);
                command.AddParameter("@CreatedByUserId", user.CreatedByUserId, DbType.Guid);
                command.AddParameter("@EmailAddress", user.EmailAddress, DbType.String);
                command.AddParameter("@Password", password, DbType.String);
                command.AddParameter("@FirstName", user.FirstName, DbType.String);
                command.AddParameter("@LastName", user.LastName, DbType.String);
                command.AddParameter("@TitleId", (int)user.Title, DbType.String);
                command.AddParameter("@ContactNumber", user.ContactNumber, DbType.String);
                command.AddParameter("@PreferredCurrencyIso", user.PreferredCurrencyIso, DbType.String);
                command.AddParameter("@TimezoneId", user.TimezoneId, DbType.String);
                command.AddParameter("@MemorableWord", user.MemorableWord, DbType.String);
                command.AddParameter("@SecurityQuestion1", user.SecurityQuestion1, DbType.String);
                command.AddParameter("@SecurityAnswer1", user.SecurityAnswer1, DbType.String);
                command.AddParameter("@SecurityQuestion2", user.SecurityQuestion2, DbType.String);
                command.AddParameter("@SecurityAnswer2", user.SecurityAnswer2, DbType.String);
                command.AddParameter("@SecurityQuestion3", user.SecurityQuestion3, DbType.String);
                command.AddParameter("@SecurityAnswer3", user.SecurityAnswer3, DbType.String);

                if ((await command.ExecuteNonQueryAsync()) > 0)
                {
                    user.CloudUserStatus = EnumCloudUserStatus.Unverified;

                    ((CloudUser)user).SetInternals(false, false, null, null);

                    //var verificationCode = VerificationCodeManager.GenerateVerificationCode(Constants.SystemUserId, user.EmailAddress,
                    //    EnumVerificationType.ForgotPassword, EnumVerificationCodeType.Complex, Constants.SystemConnectionId);

                    //SmtpManager.SendCloudUserEmailVerification(User, verificationCode);

                    return true;
                }
            }

			return false;
		}

		public async Task<bool> VerifyUserEmailAddressAsync(string emailAddress, string emailAddressVerificationCode)
		{
			emailAddress = (emailAddress + "").Trim();
            emailAddressVerificationCode = (emailAddressVerificationCode + "").Trim();

            if (emailAddress.Length == 0 || emailAddressVerificationCode.Length == 0)
			{
				throw new ArgumentNullException("User email address and verification code may not be empty");
			}

            using (var command = _dataHandler.CreateCommand("VerifyCloudUserEmailAddress"))
            {
                command.AddParameter("@EmailAddressVerificationCode", emailAddressVerificationCode, DbType.String);
                command.AddParameter("@EmailAddress", emailAddress, DbType.String);

                var result = (await command.ExecuteScalarAsync()) + "";

                return "1".Equals(result) || "TRUE".Equals(result, StringComparison.OrdinalIgnoreCase);
            }
		}

		public async Task<bool> ChangeUserPasswordAsync(Guid cloudUserId, string oldPassword, string newPassword)
		{
			if ((newPassword + "").Trim().Length == 0 || Guid.Empty.Equals(cloudUserId))
			{
				throw new ArgumentNullException("User ID and new password may not be empty");
			}

			if ((newPassword + "").Length > 50)
			{
				throw new ArgumentException("The specified new password is too long, it may not be more than 50 characters in length");
			}

            using (var command = _dataHandler.CreateCommand("ChangeUserPassword"))
            {
                command.AddParameter("@CloudUserId", cloudUserId, DbType.Guid);
                command.AddParameter("@OldPassword", oldPassword, DbType.String);
                command.AddParameter("@NewPassword", newPassword, DbType.String);

                return (await command.ExecuteNonQueryAsync()) > 0;
            }
		}

		public async Task<bool> ChangeUserPasswordAsync(string verificationCodeValue, string emailAddress, string newPassword)
		{
			if ((verificationCodeValue + "").Trim().Length == 0
                || (emailAddress + "").Trim().Length == 0
                || (newPassword + "").Trim().Length == 0)
			{
				throw new ArgumentNullException("Verification code, email address and password must be specified");
			}

			if ((newPassword + "").Length > 50)
			{
				throw new ArgumentException("The specified new password is too long, it may not be more than 50 characters in length");
			}

            using (var command = _dataHandler.CreateCommand("ChangeCloudUserForgottenPassword"))
            {
                command.AddParameter("@VerificationCodeValue", verificationCodeValue, DbType.String);
                command.AddParameter("@EmailAddress", emailAddress, DbType.String);
                command.AddParameter("@NewPassword", newPassword, DbType.String);

                return (await command.ExecuteNonQueryAsync()) > 0;
            }
		}

		public async Task<bool> ChangeUserEmailAddressAsync(IUser user, string emailAddress)
		{
			if (user == null)
			{
				throw new ArgumentException("The specified user does not exist");
			}

			if (user.EmailAddress == emailAddress)
			{
				throw new ArgumentException("The specified new email address is the same as the old email address");
			}

            using (var command = _dataHandler.CreateCommand("ChangeCloudUserEmailAddress"))
            {
                command.AddParameter("@CloudUserId", user.CloudUserId, DbType.Guid);
                command.AddParameter("@EmailAddress", emailAddress, DbType.String);
                command.AddParameter("@CloudUserStatus", EnumCloudUserStatus.Unverified, DbType.Int32);
                command.AddParameter("@DateModified", DateTime.UtcNow, DbType.DateTime);
                command.AddParameter("@ModifiedByUserId", user.ViewedByUserId, DbType.Guid);

                if ((await command.ExecuteNonQueryAsync()) > 0)
                {
                    user.EmailAddress = emailAddress;
                    user.CloudUserStatus = EnumCloudUserStatus.Unverified;

                    ((CloudUser)user).SetInternals(false, false, DateTime.UtcNow, user.ViewedByUserId);
                }

                return true;
            }
		}
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dataHandler?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}