using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Firebase.Auth;
using Firebase.Extensions;

using UnityEngine;

namespace BlestX.Services.Firebase
{
    public class EmailAuth : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] FirebaseCore _app;
#pragma warning restore 649 

        FirebaseAuth _auth;

        Dictionary<string, FirebaseUser> _userByAuth =
            new Dictionary<string, FirebaseUser>();

        public string DisplayName
        {
            get;
            private set;
        }

        public FirebaseUser User
        {
            get;
            private set;
        }

        public event Action<FirebaseUser> SignedIn;
        public event Action SignedOut;

        // Start is called before the first frame update
        void Start()
        {
            DisplayName = Environment.UserName.ToLower();
            _app.Initialized += initialize;
        }

        // Handle initialization of the necessary firebase modules:
        void initialize()
        {
            _auth = FirebaseAuth.DefaultInstance;
            _auth.StateChanged += authStateChanged;
            _auth.IdTokenChanged += idTokenChanged;

            authStateChanged(this, null);
            signIn();
        }

        void signIn()
        {
            var email = $"{DisplayName}@blestx.local";
            var password = $"{email}{email.GetHashCode()}";
            Debug.Log($"[EmailAuth::signIn] Signing in {DisplayName} / {email}");

            var credential = EmailAuthProvider.GetCredential(email, password);
            _auth.SignInWithCredentialAsync(credential)
                .ContinueWithOnMainThread(handleSignInWithUser);
        }

        void authStateChanged(object sender, System.EventArgs eventArgs)
        {
            FirebaseAuth senderAuth = sender as FirebaseAuth;
            FirebaseUser user = null;

            if (senderAuth != null)
                _userByAuth.TryGetValue(senderAuth.App.Name, out user);

            if (senderAuth == _auth && senderAuth.CurrentUser != User)
            {
                bool signedIn = User != senderAuth.CurrentUser && senderAuth.CurrentUser != null;
                if (!signedIn && User != null)
                {
                    Debug.Log($"[EmailAuth::authStateChanged] Signed out {User.UserId}");
                    User = null;
                    SignedOut?.Invoke();
                }

                User = senderAuth.CurrentUser;
                _userByAuth[senderAuth.App.Name] = User;

                if (signedIn)
                {
                    Debug.Log($"[EmailAuth::authStateChanged] Signed in {DisplayName} {User.UserId} / {User.Email}");
                    // DisplayDetailedUserInfo(user, 1);
                }
            }
        }

        void idTokenChanged(object sender, System.EventArgs eventArgs)
        {
            FirebaseAuth senderAuth = sender as FirebaseAuth;
            if (senderAuth == _auth && senderAuth.CurrentUser != null)
            {
                senderAuth.CurrentUser.TokenAsync(false).ContinueWithOnMainThread(
                    task => Debug.Log($"[EmailAuth::idTokenChanged] Token[0:8] = {task.Result.Substring(0, 8)}"));
            }
        }

        // Called when a sign-in with profile data completes.
        // void handleSignInWithSignInResult(Task<Firebase.Auth.SignInResult> task)
        // {
        //     Debug.Log($"[handleSignInWithSignInResult]");
        //     SignedOut?.Invoke(task.Result.User);
        // }

        // Called when a sign-in without fetching profile data completes.
        void handleSignInWithUser(Task<FirebaseUser> task)
        {
            switch (task.Status)
            {
                case TaskStatus.Faulted:
                    Debug.LogWarning($"[EmailAuth::handleSignInWithUser] Task faulted: {task.Exception.ToString()}");
                    handleFault(task.Exception);
                    break;

                case TaskStatus.RanToCompletion:
                    this.User = task.Result;
                    Debug.Log($"[EmailAuth] Sign in complete {this.User.UserId}");
                    SignedIn?.Invoke(task.Result);
                    break;

                default:
                    Debug.LogWarning($"[EmailAuth::handleSignInWithUser] Unhandled task status: {task.Status}");
                    break;
            }
        }

        void createUser()
        {
            var email = $"{DisplayName}@reticle.local";
            var password = $"{email}{email.GetHashCode()}";
            Debug.Log($"[EmailAuth::createUser] Creating {DisplayName} / {email}");

            var credential = EmailAuthProvider.GetCredential(email, password);
            _auth.CreateUserWithEmailAndPasswordAsync(email, password)
                .ContinueWithOnMainThread(handleSignInWithUser);
        }


        void handleFault(Exception taskEx)
        {
            Exception exception = null;
            if (taskEx is AggregateException)
            {
                exception = taskEx;
                while (exception.InnerException != null)
                {
                    Debug.Log(exception.InnerException.ToString());
                    exception = exception.InnerException;
                }
            }

            if (exception != null)
            {
                const string NO_USER_EXISTS = "There is no user record corresponding to this identifier. The user may have been deleted.";

                if (exception.Message == NO_USER_EXISTS)
                {
                    Debug.Log(exception.ToString());
                    createUser();
                    return;
                }

                throw taskEx;
            }
        }
    }
}