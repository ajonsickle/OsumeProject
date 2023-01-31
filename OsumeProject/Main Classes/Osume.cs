using System;
using System.Collections.Generic;
using System.Text;

namespace OsumeProject
{
    public class Osume
    {
        private apiClient apiClient;
        public databaseManager databaseManager;
        public Osume(apiClient apiClient, databaseManager databaseManager)
        {
            this.apiClient = apiClient;
            this.databaseManager = databaseManager;
        }
        public apiClient getApiClient()
        {
            return this.apiClient;
        }
        public databaseManager getDatabaseManager()
        {
            return this.databaseManager;
        }
    }
}
