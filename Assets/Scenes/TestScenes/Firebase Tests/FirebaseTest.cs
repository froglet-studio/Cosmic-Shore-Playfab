using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using Firebase.Auth;
using Firebase.Extensions;
using StarWriter.Utility.Singleton;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Scenes.TestScenes.Firebase_Tests
{
    public class YieldTask : CustomYieldInstruction
    {
        public YieldTask(Task task)
        {
            Task = task;
        }

        public override bool keepWaiting => !Task.IsCanceled;
        public Task Task { get; }
    }
    public class FirebaseTest : SingletonPersistent<FirebaseTest>
    {
        private FirebaseAuth _auth;

        private FirebaseApp _app;
        private FirebaseUser _user;
        public UnityEvent FirebaseInitialized = new();

        private Queue<Action> _actionQueue = new();
        private void Start()
        {
            // CheckAndFixDependencies();
            // CheckAndFixAlt();
            // AuthAfterDependencyCheck(); // works dandy
            // GetSuccesses(); // works
            // await CheckFixAndAuth();// Crashes Unity, don't recommend
            // StartCoroutine(DoTheThing());// Doesn't quite work
            // QueueActions(); // works as well
            // AnonymousLogin(); // works, only returns user id, no user name (of course it's not set)
            // AnonymousLoginWithCustomToken(); // system device id cannot be used as uid, not correct format
            // CreateAccountEmailPassword(); // works for once, the second time running will return duplicated account error
            // LoginWithEmailPassword(); // works with existing account
            // ChangeAuthState();// works but OnAuthStateChanged executed twice
            
            // UpdateUserProfile(); // After updating with new information the user profile can query updated info
            // SetUserEmail(); // This doesn't work right now USE_AUTH_EMULATOR not set
            // GetUserProviderProfile(); // the same result above
            // GetUserProfile(); // The default instance will remember the previous login without having to manually login
        }

        private void Update()
        {
            // UpdateWithAction();
        }

        private void SetUserEmail()
        {
            var email = "echo_update@froglet.studios";
            _auth = FirebaseAuth.DefaultInstance;
            var user = _auth.CurrentUser;

            user?.UpdateEmailAsync(email).ContinueWithOnMainThread(
                updateEmailTask =>
                {
                    if (updateEmailTask.IsCanceled) return;
                    if (updateEmailTask.IsFaulted) return;

                    Debug.Log("Your email is successfully updated.");
                });
        }

        private void UpdateUserProfile()
        {
            _auth = FirebaseAuth.DefaultInstance;
            var user = _auth.CurrentUser;
            if (user == null) return;

            var profile = new UserProfile
            {
                DisplayName = "echoness",
                PhotoUrl = new Uri("https://example.com/jane-q-user/profile.jpg")
            };

            user.UpdateUserProfileAsync(profile).ContinueWith(
                updateTask =>
                {
                    if (updateTask.IsCanceled)
                    {
                        Debug.LogError("Updating profile was canceled.");
                        return;
                    }

                    if (updateTask.IsFaulted)
                    {
                        Debug.LogErrorFormat("Updating profile encountered an error {0}", updateTask.Exception.Message);
                        return;
                    }

                    Debug.Log("My glorious profile updated successfully.");
                });
        }

        private void GetUserProviderProfile()
        {
            _auth = FirebaseAuth.DefaultInstance;
            if (_auth.CurrentUser == null) return;
            
            var user = _auth.CurrentUser;
            
            foreach (var profile in user.ProviderData)
            {
                Debug.LogFormat("Current profile name: {0} email: {1} uid: {2} photo url: {3}", 
                    profile.DisplayName, profile.Email, profile.UserId, profile.PhotoUrl);
            }
        }

        private void GetUserProfile()
        {
            _auth = FirebaseAuth.DefaultInstance;
            if (_auth == null) return;

            var user = _auth.CurrentUser;
            
            Debug.LogFormat("Current user name: {0} email: {1} uid: {2}", user.DisplayName, user.Email, user.UserId);
        }

        private void ChangeAuthState()
        {
            _auth = FirebaseAuth.DefaultInstance;
            _user = null;
            _auth.StateChanged += OnAuthStateChanged;
            OnAuthStateChanged(this, null);
        }

        private void OnDestroy()
        {
            _auth.StateChanged -= OnAuthStateChanged;
            _auth = null;
            Debug.Log("Firebase auth cleans up.");
        }

        private void OnAuthStateChanged(object sender, EventArgs eventArgs)
        {
            if (_auth.CurrentUser == null)
            {
                Debug.LogWarning("Not logged in.");
                return;
            }
            _user = _auth.CurrentUser;
            Debug.LogFormat("Current user: id {0} name {1} email {2}", 
                _user.UserId, _user.DisplayName, _user.Email);
        }

        private void LoginWithEmailPassword()
        {
            var email = "echo@froglet.studio";
            var password = "this is super secure.";
            _auth = FirebaseAuth.DefaultInstance;
            _auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(
                loginTask =>
                {
                    if (loginTask.IsCanceled)
                    {
                        Debug.LogError("Login with email password canceled.");
                        return;
                    }

                    if (loginTask.IsFaulted)
                    {
                        Debug.LogErrorFormat("Email login error: {0}", loginTask.Exception.Message);
                        return;
                    }

                    var result = loginTask.Result;
                    if (result != null)
                        Debug.LogFormat("You've logged in with {0} {1} {2}", result.User.UserId, result.User.Email,
                            result.User.IsEmailVerified.ToString());
                });
        }

        private void CreateAccountEmailPassword()
        {
            var email = "echo@froglet.studio";
            var password = "this is super secure.";
            // var displayName = "Unknown Nightmare";
            _auth = FirebaseAuth.DefaultInstance;
            _auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(
                createAccountTask =>
                {
                    if (createAccountTask.IsCanceled)
                    {
                        Debug.LogError("The create account with email and password is canceled");
                        return;
                    }

                    if (createAccountTask.IsFaulted)
                    {
                        Debug.LogError(
                                $"Error encountered when login with custom token. {createAccountTask.Exception.Message}");
                        return;
                    }
                
                    var result = createAccountTask.Result;
                    // result.User.UpdateUserProfileAsync(updateTask =>
                    // {
                    //     updateTask
                    // })
                    if(result!=null)
                        Debug.LogFormat("You've logged in with {0} {1} {2}", result.User.UserId, result.User.Email, result.User.IsEmailVerified.ToString());
                }
            );
        }

        private void AnonymousLoginWithCustomToken()
        {
            _auth = FirebaseAuth.DefaultInstance;
            _auth.SignInWithCustomTokenAsync(SystemInfo.deviceUniqueIdentifier).ContinueWithOnMainThread(
                task =>
                {
                    if (task.IsCanceled)
                    {
                        Debug.LogError("The custom token login is canceled");
                        return;
                    }

                    if (task.IsFaulted)
                    {
                        Debug.LogError($"Error encountered when login with custom token. {task.Exception}");
                        return;
                    }

                    var result = task.Result;
                    if(result!=null)
                        Debug.LogFormat("User signed in successfully: {0} - {1}", result.User.DisplayName, result.User.UserId);
                });
        }

        private void AnonymousLogin()
        {
            _auth = FirebaseAuth.DefaultInstance;

            _auth.SignInAnonymouslyAsync().ContinueWith(
                task =>
                {
                    if (task.IsCanceled)
                    {
                        Debug.LogError("Anonymous login was canceled.");
                        return;
                    }

                    if (task.IsFaulted)
                    {
                        Debug.LogError($"Anonymous login encountered an error {task.Exception}");
                        return;
                    }

                    var result = task.Result;
                    if (result != null)
                        Debug.LogFormat("User signed in successfully: {0} - {1}", result.User.DisplayName,
                            result.User.UserId);
                });
        }

        private void UpdateWithAction()
        {
            while (_actionQueue.Any())
            {
                Action action;
                lock (_actionQueue)
                {
                    action = _actionQueue.Dequeue();
                }

                action();
            }
        }

        private void EnqueueAction(Action action)
        {
            lock (_actionQueue)
            {
                _actionQueue.Enqueue(action);
            }
        }

        private void QueueActions()
        {
            Debug.Log("Checking Dependencies");
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(fixTask =>
            {
                Assert.IsNull(fixTask.Exception);
                Debug.Log("Authenticating");
                _auth = FirebaseAuth.DefaultInstance;
                _auth.SignInAnonymouslyAsync().ContinueWith(authTask =>
                {
                    EnqueueAction(() =>
                    {
                        Assert.IsNull(authTask.Exception);
                        Debug.Log("Welcome!");
                        GetSuccesses();
                        _auth.SignOut();
                        Debug.Log("Fare thee well.");
                    });
                });
            });
        }

        private IEnumerator DoTheThing()
        {
            Debug.Log("Checking Dependencies.");
            yield return new YieldTask(FirebaseApp.CheckAndFixDependenciesAsync());
            
            Debug.Log("Authenticating");
            _auth = FirebaseAuth.DefaultInstance;
            yield return new YieldTask(_auth.SignInAnonymouslyAsync());
            
            Debug.Log("Welcome!");
            
            // GetSuccesses();
            
            _auth.SignOut();
            Debug.Log("Fare thee well!");
        }

        private async Task CheckFixAndAuth()
        {
            Debug.Log("Checking Dependencies.");
            await FirebaseApp.CheckAndFixDependenciesAsync();
            
            Debug.Log("Authenticating...");
            _auth = FirebaseAuth.DefaultInstance;
            await _auth.SignInAnonymouslyAsync();
            
            Debug.Log("Signed in!");
            
            GetSuccesses();
            
            _auth.SignOut();
            Debug.Log("Signed out!");
            
        }

        private void GetSuccesses()
        {
            var successes = PlayerPrefs.GetInt("Successes", 0);
            PlayerPrefs.SetInt("Successes", ++successes);
            Debug.Log($"Successes after {successes}");
        }

        private void OnEnable()
        {
            FirebaseInitialized.AddListener(OnFirebaseInitialized);
        }

        private void OnDisable()
        {
            // FirebaseInitialized.RemoveListener(OnFirebaseInitialized);
            FirebaseInitialized.RemoveAllListeners();
        }

        private void CheckAndFixDependencies()
        {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(
                fixTask =>
                {
                    Assert.IsNull(fixTask.Exception);
                    Debug.Log("Authenticating");
                    var auth = FirebaseAuth.DefaultInstance;
                    auth.SignInAnonymouslyAsync().ContinueWith(
                        authTask =>
                        {
                            Debug.Log("Starting anonymous login.");
                            Assert.IsNull(authTask.Exception);
                            Debug.Log("Signed in!");
                            
                            var successes = PlayerPrefs.GetInt("Successes", 0);
                            PlayerPrefs.SetInt("Successes", ++successes);
                            Debug.Log($"Successes after {successes}");
                            
                            auth.SignOut();
                            Debug.Log("Signed Out.");
                        }, taskScheduler);
                });
        }

        private void CheckAndFixAlt()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(
                fixTask =>
                {
                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                    Debug.Log("Analytics enabled.");
                });
        }

        private async void AuthAfterDependencyCheck()
        {
            var dependencyResult = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyResult == DependencyStatus.Available)
            {
                _auth = FirebaseAuth.DefaultInstance;
                FirebaseInitialized?.Invoke();
            }
            else
            {
                Debug.LogError($"Failed to initialize Firebase with {dependencyResult}");
            }
        }

        private void OnFirebaseInitialized()
        {
            Debug.Log($"Firebase initialized. Now let's do some cool stuff");
        }

        private void LinkWithDeviceIdentifier()
        {
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            // var credential = Firebase.Auth.
        }
    }
}

