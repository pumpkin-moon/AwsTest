using System;
using UnityEngine;

namespace Helpers
{
    public static class Prefs
    {
        public static string Hash
        {
            get
            {
                const string key = nameof(Hash);
                
                if (!PlayerPrefs.HasKey(key))
                {
                    PlayerPrefs.SetString(key, Guid.NewGuid().ToString());
                }

                return PlayerPrefs.GetString(key);
            }
        }
    }
}