using Firebase;
using Firebase.Extensions;
using Firebase.Unity.Editor;

using UnityEngine;

namespace BlestX.Services.Firebase
{
    public class FirebaseCore : MonoBehaviour
    {
        public const string URL = "https://oculus-experiments.firebaseio.com";

        /// <summary>
        /// Once the Firebase app has initialized, the Initialized event is fired
        /// </summary>
        public event System.Action Initialized;

        FirebaseApp _app;

        void Start()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    _app = FirebaseApp.DefaultInstance;
                    _app.SetEditorDatabaseUrl(URL);
                    Initialized?.Invoke();
                }
                else
                {
                    throw new UnityException(
                        "Could not resolve all Firebase dependencies: " + dependencyStatus);
                }
            });
        }
    }
}