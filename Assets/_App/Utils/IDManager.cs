using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace _App
{
    public class IDManager 
    {
        public static string GetID()
        {
            Social.localUser.Authenticate(success => {
                if (success )
                {
                    Debug.Log("Authenticated with Game Center");
                    PlayerPrefs.SetString(PrefsKeys.GAME_ID, Social.localUser.id);
                }
            });

            if (Social.localUser.authenticated && Social.localUser.state != UserState.Offline)
            {
                return Social.localUser.id;
            }

            var deviceID = SystemInfo.deviceUniqueIdentifier;
            if (!string.IsNullOrEmpty(deviceID))
            {
                PlayerPrefs.SetString(PrefsKeys.GAME_ID, deviceID);
                return deviceID;
            }
            
            return "";
        }
        
        public static string GenerateGameID()
        {
            var userID = PlayerPrefs.GetString(PrefsKeys.USER_NAME, "");
            if (!string.IsNullOrEmpty(userID)) return userID;
            userID = GenerateUniqueID();
            PlayerPrefs.SetString(PrefsKeys.USER_NAME, userID);
            return userID;
        }
        
        private static string GenerateUniqueID()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}