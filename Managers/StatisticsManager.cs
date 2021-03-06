﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using LittleLogBook.Data.Contracts;
using LittleLogBook.Data.Entities.Cloud;
using LittleLogBook.Data.SqlConnectivity;

namespace LittleLogBook.Data.Managers
{
    public class StatisticsManager : ManagerBase, IStatisticsManager
    {
        private readonly IDataHandler _dataHandler;

        internal StatisticsManager(IDataHandler dataHandler, IUser currentUser)
            : base(currentUser)
        {
            _dataHandler = dataHandler;
        }

        public async Task<IEnumerable<INameValueValueStatistic>> GetCloudBackupRestoreDataTrafficPerCountry()
        {
            var returnValues = new List<INameValueValueStatistic>();

            using (var command = _dataHandler.CreateCommand("GetCloudBackupRestoreDataTrafficPerCountry"))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        returnValues.Add(new NameValueValueStatistic(CurrentUser.CloudUserId, reader));
                    }
                }

                return returnValues;
            }
        }

        public async Task<IEnumerable<INameValueValueStatistic>> GetCloudBackupsRestoresPerCountry()
        {
            var returnValues = new List<INameValueValueStatistic>();

            using (var command = _dataHandler.CreateCommand("GetCloudBackupsRestoresPerCountry"))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        returnValues.Add(new NameValueValueStatistic(CurrentUser.CloudUserId, reader));
                    }
                }

                return returnValues;
            }
        }

        public async Task<IEnumerable<INameValueValueStatistic>> GetCloudCreditsPerCountry()
        {
            var returnValues = new List<INameValueValueStatistic>();

            using (var command = _dataHandler.CreateCommand("GetCloudCreditsPerCountry"))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        returnValues.Add(new NameValueValueStatistic(CurrentUser.CloudUserId, reader));
                    }
                }

                return returnValues;
            }
        }

        public async Task<IEnumerable<INameValueValueStatistic>> GetCloudSpendPerCountry()
        {
            var returnValues = new List<INameValueValueStatistic>();

            using (var command = _dataHandler.CreateCommand("GetCloudSpendPerCountry"))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        returnValues.Add(new NameValueValueStatistic(CurrentUser.CloudUserId, reader));
                    }
                }

                return returnValues;
            }
        }


        public async Task<IEnumerable<INameValueStatistic>> GetInstallationsPerCountry()
        {
            var returnValues = new List<INameValueStatistic>();

            using (var command = _dataHandler.CreateCommand("GetInstallationsPerCountry"))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        returnValues.Add(new NameValueStatistic(CurrentUser.CloudUserId, reader));
                    }
                }

                return returnValues;
            }
        }

        public async Task<IEnumerable<INameValueStatistic>> GetDevicesPerCountry()
        {
            var returnValues = new List<INameValueStatistic>();

            using (var command = _dataHandler.CreateCommand("GetDevicesPerCountry"))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        returnValues.Add(new NameValueStatistic(CurrentUser.CloudUserId, reader));
                    }
                }

                return returnValues;
            }
        }

        public async Task<IEnumerable<INameValueValueStatistic>> GetCloudUsersPerCountry()
        {
            var returnValues = new List<INameValueValueStatistic>();

            using (var command = _dataHandler.CreateCommand("GetCloudUsersPerCountry"))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        returnValues.Add(new NameValueValueStatistic(CurrentUser.CloudUserId, reader));
                    }
                }

                return returnValues;
            }
        }

        public async Task<IEnumerable<INameValueStatistic>> GetActiveDevicesPerCountry()
        {
            var returnValues = new List<INameValueStatistic>();

            using (var command = _dataHandler.CreateCommand("GetActiveDevicesPerCountry"))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        returnValues.Add(new NameValueStatistic(CurrentUser.CloudUserId, reader));
                    }
                }

                return returnValues;
            }
        }
        
        public async Task<IEnumerable<INameValueValueStatistic>> GetSqlConnections()
        {
            var returnValues = new List<INameValueValueStatistic>();

            using (var command = _dataHandler.CreateCommand("GetSqlConnections"))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        returnValues.Add(new NameValueValueStatistic(CurrentUser.CloudUserId, reader));
                    }
                }

                return returnValues;
            }
        }
        
        public async Task<IEnumerable<IStatistic>> GetStatisticsForUserAsync(Guid cloudUserId)
        {
            if (Guid.Empty.Equals(cloudUserId))
            {
                throw new ArgumentNullException("The specified cloud user ID may not be empty");
            }

            var returnValues = new List<Statistic>();

            using (var command = _dataHandler.CreateCommand("GetStatisticsForUser"))
            {
                command
                    .AddParameter("@CloudUserId", cloudUserId, DbType.Guid)
                    .AddParameter("@ViewedByUserId", CurrentUser.CloudUserId, DbType.Guid);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        returnValues.Add(new Statistic(CurrentUser.CloudUserId, reader));
                    }
                }

                return returnValues;
            }
        }

        public async Task<bool> UpdateBackupEntryAsync(IBackupEntry backupEntry)
        {
            using (var command = _dataHandler.CreateCommand("UpdateBackupEntry"))
            {
                command
                    .AddParameter("@BackupEntryId", backupEntry.BackupEntryId, DbType.Guid)
                    .AddParameter("@BackupName", backupEntry.BackupName, DbType.String)
                    .AddParameter("@BackupDescription", backupEntry.BackupDescription, DbType.String)
                    .AddParameter("@ModifiedByUserId", CurrentUser.CloudUserId, DbType.Guid);

                if (await command.ExecuteNonQueryAsync() > 0)
                {
                    ((BackupEntry) backupEntry).SetInternals(false, false, DateTime.UtcNow, CurrentUser.CloudUserId);

                    return true;
                }

                return false;
            }
        }

        public async Task<bool> FinishBackupEntryAsync(Guid backupEntryId, long backupSize)
        {
            using (var command = _dataHandler.CreateCommand("FinishBackupEntry"))
            {
                command
                    .AddParameter("@BackupEntryId", backupEntryId, DbType.Guid)
                    .AddParameter("@BackupSize", backupSize, DbType.Int64)
                    .AddParameter("@ModifiedByUserId", CurrentUser.CloudUserId, DbType.Guid);

                return await command.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> CancelBackupEntryAsync(Guid backupEntryId, long backupSize)
        {
            using (var command = _dataHandler.CreateCommand("CancelBackupEntry"))
            {
                command
                    .AddParameter("@BackupEntryId", backupEntryId, DbType.Guid)
                    .AddParameter("@BackupSize", backupSize, DbType.Int64)
                    .AddParameter("@ModifiedByUserId", CurrentUser.CloudUserId, DbType.Guid);

                return await command.ExecuteNonQueryAsync() > 0;
            }
        }

        public async Task<bool> InsertBackupEntryAsync(IBackupEntry backupEntry)
        {
            using (var command = _dataHandler.CreateCommand("InsertBackupEntry"))
            {
                command
                    .AddParameter("@BackupEntryId", backupEntry.BackupEntryId, DbType.Guid)
                    .AddParameter("@CloudUserId", backupEntry.CloudUserId, DbType.Guid)
                    .AddParameter("@BackupEntryTypeId", (int) backupEntry.BackupEntryType, DbType.Int32)
                    .AddParameter("@BackupFilename", backupEntry.BackupFilename, DbType.String)
                    .AddParameter("@CreatedByUserId", CurrentUser.CloudUserId, DbType.Guid);

                if (await command.ExecuteNonQueryAsync() > 0)
                {
                    ((BackupEntry) backupEntry).SetInternals(false, false, DateTime.UtcNow, CurrentUser.CloudUserId);

                    return true;
                }

                return false;
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