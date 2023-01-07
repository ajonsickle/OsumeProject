using System;
using System.Collections.Generic;
using System.Text;

namespace OsumeProject
{
    public static class factory
    {
        private static user instance = null;
        public static void deleteSingleton()
        {
            instance = null;
        }

        public static user getSingleton()
        {
            return instance;
        }

        public static void createSingleton(bool admin)
        {
            if (instance == null)
            {
                if (admin)
                {
                    instance = new adminUser();
                } else
                {
                    instance = new user();
                }
            }
            else throw new Exception("User already exists!");
        }

    }
}
