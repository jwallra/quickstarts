﻿using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using QuickStart.UWP.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace QuickStart.UWP.Data
{
    public class DataStore
    {
        #region Static Settings
        private static string clouduri = "https://ahall-samples-push-on-insert.azurewebsites.net";
        private static string localcache = "localtaskstore.db";
        #endregion

        #region Private Instance Variables
        private bool _initialized = false;
        #endregion

        /// <summary>
        /// Initialize the Data Store asynchronously
        /// </summary>
        /// <returns></returns>
        private async Task InitializeAsync()
        {
            // Create the two sides of the service
            CloudService = new MobileServiceClient(DataStore.clouduri);
            LocalCacheService = new MobileServiceSQLiteStore(localcache);

            // Define the table structure
            ((MobileServiceSQLiteStore)LocalCacheService).DefineTable<TodoItem>();

            // Initialize the local store
            await CloudService.SyncContext.InitializeAsync(LocalCacheService);
        }

        /// <summary>
        /// Provide an async authentication mechanism
        /// </summary>
        /// <returns>Task (async)</returns>
        public async Task AuthenticateAsync()
        {
            if (!IsAuthenticated)
            {
                try
                {
                    User = await CloudService.LoginAsync(MobileServiceAuthenticationProvider.MicrosoftAccount);
                    Debug.WriteLine("AUTH:{0}:{1}", User.UserId, User.MobileServiceAuthenticationToken);
                }
                catch (MobileServiceInvalidOperationException ex)
                {
                    Debug.WriteLine("EXCEPTION:{0}:{1}:{2}:{3}:{4}", ex.GetType(),
                        ex.HResult, ex.Message, ex.Request.RequestUri, ex.Response.ReasonPhrase);
                    throw new LoginDeniedException();
                }
            }
        }

        #region Properties
        /// <summary>
        /// True if the stores are initialized
        /// </summary>
        public bool IsInitialized
        {
            get { return _initialized; }
        }

        /// <summary>
        /// True if the cloud service is authenticated
        /// </summary>
        public bool IsAuthenticated
        {
            get { return (User != null);  }
        }

        public MobileServiceClient CloudService { get; private set; }

        public MobileServiceLocalStore LocalCacheService { get; private set; }

        public MobileServiceUser User { get; private set; }
        #endregion

        #region Singleton
        // Singleton private storage
        private static DataStore _instance = null;

        /// <summary>
        /// Singleton Pattern - returns the current singleton instance of the DataStore.
        /// If one does not exist, it is created and cached.
        /// </summary>
        /// <returns>The current Singleton instance of the data store</returns>
        public static async Task<DataStore> GetInstance()
        {
            if (_instance == null)
            {
                _instance = new DataStore();
                await _instance.InitializeAsync();

            }
            return _instance;
        }
        #endregion
    }
}
