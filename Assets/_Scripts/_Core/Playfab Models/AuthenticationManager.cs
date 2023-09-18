using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using StarWriter.Utility.Singleton;
using System.Security;
using JetBrains.Annotations;
using PlayFab.SharedModels;

namespace _Scripts._Core.Playfab_Models
{
    /// <summary>
    /// Authentication methods
    /// Authentication methods references: https://api.playfab.com/documentation/client#Authentication
    /// </summary>
    public enum AuthMethods
    {
        Default,
        Anonymous,
        PlayFabLogin,
        EmailLogin,
        Register
    }

    public class AuthenticationManager : SingletonPersistent<AuthenticationManager>
    {
        public static PlayerAccount PlayerAccount;
        public static PlayerProfile PlayerProfile;
        
        // public delegate void LoginSuccessEvent();
        // public static event LoginSuccessEvent OnLoginSuccess;
        public static EventHandler<PlayFabResultCommon> OnLoginSuccess;

        // public delegate void LoginErrorEvent();
        // public static event LoginErrorEvent OnLoginError;
        public static EventHandler<PlayFabResultCommon> OnLoginError;

        // public delegate void ProfileLoaded();
        // public static event ProfileLoaded OnProfileLoaded;
        public static EventHandler<PlayFabResultCommon> OnProfileLoaded;

        public static List<string> Adjectives;
        public static List<string> Nouns;

        void Start()
        {
            AuthenticationManager.Instance.AnonymousLogin();
            OnLoginSuccess += LoadProfileAfterLogin;
        }

        /// <summary>
        /// Set Player Display Name
        /// Update player display name, we can assume the account is already created here
        /// </summary>
        public void SetPlayerDisplayName(string displayName, Action callback = null)
        {
            PlayFabClientAPI.UpdateUserTitleDisplayName(
                new UpdateUserTitleDisplayNameRequest()
                {
                    DisplayName = displayName
                },
                (result) =>
                {
                    Debug.Log($"AuthenticationManager - Successful updated player display name: {PlayerAccount.PlayerDisplayName}");

                    PlayerAccount.PlayerDisplayName = result.DisplayName;
                    PlayerProfile.DisplayName = result.DisplayName;
                    callback?.Invoke();
                }, 
                (error) =>
                {
                    Debug.LogError(error.GenerateErrorReport());
                }
            );
        }

        void LoadProfileAfterLogin(object sender,  PlayFabResultCommon result) 
        {
            LoadPlayerProfile(result);
        }

        /// <summary>
        /// Load Player Profile
        /// Load player profile using Playfab Id and return player display name
        /// </summary>
        public void LoadPlayerProfile(PlayFabResultCommon result)
        {
            PlayFabClientAPI.GetPlayerProfile(
                new GetPlayerProfileRequest()
                {
                    PlayFabId = PlayerAccount.PlayFabId
                }, 
                (result) =>
                {
                    // The result will get publisher id, title id, player id (also called playfab id in other requests) and display name
                    PlayerProfile ??= new PlayerProfile();
                    PlayerProfile.DisplayName = result.PlayerProfile.DisplayName;
                    
                    // TODO: It might be good to retrieve player avatar url here 
                    
                    Debug.Log("AuthenticationManager - Successfully retrieved player profile");
                    Debug.Log($"AuthenticationManager - Player id: {PlayerProfile.DisplayName}");

                    OnProfileLoaded?.Invoke(this,result);
                },
                (error) =>
                {
                    Debug.LogError(error.GenerateErrorReport());
                }
            );
        }
        
        /// <summary>
        /// Load Default Adjectives and Nouns
        /// Get a list of adjectives and nouns stored in Playfab title data for random name generation
        /// </summary>
        public void LoadRandomNameList()
        {
            if (Adjectives != null && Nouns != null)
            {
                Debug.Log("AuthenticationManager - Names are already retrieved.");
                return;
            }
            
            PlayFabClientAPI.GetTitleData(
                new GetTitleDataRequest()
                {
                    AuthenticationContext = PlayerAccount.AuthContext
                }, 
                (result) =>
                {
                    if (result.Data != null)
                    {
                        Adjectives = new(JsonConvert.DeserializeObject<string[]>(result.Data["DefaultDisplayNameAdjectives"]));
                        Nouns = new(JsonConvert.DeserializeObject<string[]>(result.Data["DefaultDisplayNameNouns"]));
                        
                        Debug.Log("AuthenticationManager - Default name list loaded.");
                        Debug.Log($"AuthenticationManager - Default adjectives: {Adjectives}");
                        Debug.Log($"AuthenticationManager - Default nouns: {Nouns}");
                    }
                            
                }, 
                (error) =>
                {
                    Debug.LogError(error.GenerateErrorReport());
                }
            );
        }

        
        /// <summary>
        /// Anonymous Login
        /// If successful, populate player account with playfab id, auth context and newly created flag, no custom id for now
        /// </summary>
        public void AnonymousLogin()
        {
        #if UNITY_ANDROID && !UNITY_EDITOR
            AndroidLogin();
        #elif UNITY_IOS || UNITY_IPHONE && !UNITY_EDITOR
            IOSLogin();
        #else
            CustomIDLogin();
        #endif
        }
        
        
        /// <summary>
        /// Android Login
        /// Take Android device unique identifier id as device id and login
        /// </summary>
        void AndroidLogin()
        {
            PlayFabClientAPI.LoginWithAndroidDeviceID(
                new LoginWithAndroidDeviceIDRequest()
                {
                    CreateAccount = true,
                    AndroidDeviceId = SystemInfo.deviceUniqueIdentifier
                }, 
                HandleLoginSuccess, 
                HandleLoginError
            );
        }

        /// <summary>
        /// IOS Login
        /// Take IOS device unique identifier id as device id and login
        /// </summary>
        void IOSLogin()
        {
            PlayFabClientAPI.LoginWithIOSDeviceID(
                new LoginWithIOSDeviceIDRequest()
                {
                    CreateAccount = true,
                    DeviceId = SystemInfo.deviceUniqueIdentifier
                }, 
                HandleLoginSuccess, 
                HandleLoginError
                );
        }

        /// <summary>
        /// Custom ID Login
        /// For now custom ID login is used on PC
        /// </summary>
        void CustomIDLogin()
        {
            PlayFabClientAPI.LoginWithCustomID(
                new LoginWithCustomIDRequest()
                {
                    CreateAccount = true,
                    CustomId = SystemInfo.deviceUniqueIdentifier
                }, 
                HandleLoginSuccess, 
                HandleLoginError
                );
        }

        void HandleLoginSuccess(LoginResult loginResult = null)
        {
            PlayerAccount = PlayerAccount ?? new PlayerAccount();
            if (loginResult != null)
            {
                PlayerAccount.PlayFabId = loginResult.PlayFabId;
                PlayerAccount.AuthContext = loginResult.AuthenticationContext;
                PlayerAccount.IsNewlyCreated = loginResult.NewlyCreated;

                Debug.Log($"AuthenticationManager - Logged in - Newly Created: {loginResult.NewlyCreated.ToString()}");
                Debug.Log($"AuthenticationManager - Play Fab Id: {PlayerAccount.PlayFabId}");
                Debug.Log($"AuthenticationManager - Entity Type: {PlayerAccount.AuthContext.EntityType}");
                Debug.Log($"AuthenticationManager - Entity Id: {PlayerAccount.AuthContext.EntityId}");
                Debug.Log($"AuthenticationManager - Session Ticket: {PlayerAccount.AuthContext.ClientSessionTicket}");

                OnLoginSuccess?.Invoke(this, loginResult);
            }
        }

        void HandleLoginError(PlayFabError loginError)
        {
            Debug.LogError("AuthenticationManager - " + loginError.GenerateErrorReport());
            OnLoginError?.Invoke();
        }


        #region Unlinking

        /// <summary>
        /// Unlink Device Unique Identifier
        /// Unlink based on the device unique identifier
        /// Can be tested on Unlink Anonymous Login button
        /// Reframe from clicking on it too much, it will abandon the anonymous account, next time login will create a whole new account.
        /// </summary>
        public void UnlinkDeviceUniqueIdentifier()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        UnlinkAndroidLogin();
#elif UNITY_IPHONE || UNITY_IOS && !UNITY_EDITOR
        UnlinkIOSLogin();
#else
            UnlinkCustomIDLogin();
#endif
        }

        private void UnlinkAndroidLogin()
        {
            PlayFabClientAPI.UnlinkAndroidDeviceID(new UnlinkAndroidDeviceIDRequest()
            {
                AndroidDeviceId = SystemInfo.deviceUniqueIdentifier
            }, (result) =>
            {
                Debug.Log("Android Device Unlinked.");
            }, (error) =>
            {
                Debug.LogError(error.GenerateErrorReport());
            });
        }

        private void UnlinkIOSLogin()
        {
            PlayFabClientAPI.UnlinkIOSDeviceID(new UnlinkIOSDeviceIDRequest()
            {
                DeviceId = SystemInfo.deviceUniqueIdentifier
            }, (result) =>
            {
                Debug.Log("IOS Device Unlinked.");
            }, (error) =>
            {
                Debug.LogError(error.GenerateErrorReport());
            });
        }

        private void UnlinkCustomIDLogin()
        {
            PlayFabClientAPI.UnlinkCustomID(new UnlinkCustomIDRequest()
            {
                CustomId = SystemInfo.deviceUniqueIdentifier
            }, (result) =>
            {
                Debug.Log("Custom Device Unlinked.");
            }, (error) =>
            {
                Debug.LogError(error.GenerateErrorReport());
            });
        }

        #endregion

        #region WIP Email Login


        /// <summary>
        /// Email Login
        /// Can be tested with Email Login button
        /// </summary>
        public void OnEmailLogin()
        {
            var email = "yeah@froglet.studio";

            EmailLogin(email, GetPassword());
        }

        /// <summary>
        /// Email Login logic
        /// Make sure password stays in memory no longer than necessary
        /// </summary>
        private void EmailLogin([NotNull] string email, [NotNull] SecureString password)
        {
            if (email == null) throw new ArgumentNullException(nameof(email));
            if (password == null) throw new ArgumentNullException(nameof(password));
            PlayFabClientAPI.LoginWithEmailAddress(
                new LoginWithEmailAddressRequest()
                {
                    TitleId = PlayFabSettings.TitleId,
                    Email = email,
                    Password = password.ToString()
                },
                (result) =>
                {
                    var authenticationContext = result.AuthenticationContext;
                    password?.Dispose();
                    Debug.Log("Logged in with email.");
                    PlayFabClientAPI.GetAccountInfo(
                        new GetAccountInfoRequest()
                        {
                            Email = email,
                            PlayFabId = authenticationContext.PlayFabId
                        },
                        (GetAccountInfoResult result) =>
                        {
                            Debug.Log($"PlayFab ID: {result.AccountInfo.PlayFabId}");
                            Debug.Log($"Player email retrieved: {result.AccountInfo.PrivateInfo.Email}");
                        }, null);
                },
                (error) =>
                {
                    Debug.Log(error.GenerateErrorReport());
                }
                );
        }

        /// <summary>
        /// Update player display name with random generated one
        /// Can be tested by clicking Generate Random Name button
        /// </summary>
        public void OnRegisterWithEmail()
        {
            var email = "yeah@froglet.studio";
            // This is a test for email register, we can worry about it linking device later
            // AnonymousLogin();
            RegisterWithEmail(email, GetPassword());
        }

        void RegisterWithEmail(string email, SecureString password)
        {

            PlayFabClientAPI.AddUsernamePassword(
                new AddUsernamePasswordRequest()
                {
                    Username = "Tim",
                    Email = email,
                    Password = password.ToString()
                }, (result) =>
                {
                    Debug.Log("Register with email succeeded.");
                    Debug.Log($"Playfab ID {result.Username}");
                }, (error) =>
                {
                    Debug.Log(error.GenerateErrorReport());
                }
            );

        }

        SecureString GetPassword()
        {
            const string chars = "very secure";
            var password = new SecureString();
            foreach (var c in chars)
            {
                password.AppendChar(c);
            }

            return password;
        }
        #endregion
    }
}