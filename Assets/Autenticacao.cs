using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Database;

public class Autenticacao : MonoBehaviour
{
    public Text login;
    public Text senha;
    public GameObject loginPage;
    public Text xp;
    public Text level;
    public GameObject databasePage;
    [Header("Firebase")]
    DependencyStatus dependencyStatus;
    FirebaseAuth auth; 
    FirebaseUser user;
    DatabaseReference DBreference;
    string username;
    

    private void Awake() {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available) {
                InitializeFirebase();
            } else {
                print(dependencyStatus);
            }
        });
    }

    public void InitializeFirebase() {
        auth = FirebaseAuth.DefaultInstance;
        DBreference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void loginVai() {
        StartCoroutine(fazLogin());
    }

    public IEnumerator fazLogin() {
        string meuLogin = login.text;
        string minhaSenha = senha.text;
        
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(meuLogin, minhaSenha);

        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if(LoginTask.Exception != null) {
            SceneManager.LoadScene("erro");

            FirebaseException fex = LoginTask.Exception.GetBaseException() as FirebaseException;

            AuthError ae = (AuthError)fex.ErrorCode;

            print(fex.Message);
            print(ae.ToString());

        } else {
            //SceneManager.LoadScene("ok");

            //LOAD DATA
            username = login.text;
            StartCoroutine(loadUserData());
            //GO TO PAGE
            goToDatabasePage();

        }
    }

    public void registrarVAI() {
        StartCoroutine(registrarNovoUsuario());
    }

    public IEnumerator registrarNovoUsuario() {
        string meuLogin = login.text;
        string minhaSenha = senha.text;
        var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(meuLogin, minhaSenha);

        yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

        if(RegisterTask.Exception != null) {

        } else {
            user = RegisterTask.Result;

            if(user != null){
                UserProfile profile = new UserProfile { DisplayName = meuLogin };
                var ProfileTask = user.UpdateUserProfileAsync(profile);
                yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                if(ProfileTask.Exception != null) {

                } else {
                    print("REGISTRADO COM SUCESSO!");
                }
            }
        }
    }

    public void signOUT() {
        auth.SignOut();
        login.text = "";
        senha.text = "";
    }

    public void goToLoginPage() {
        loginPage.SetActive(true);
        databasePage.SetActive(false);
        login.text = "";
        senha.text = "";
    }

    public void goToDatabasePage() {
        loginPage.SetActive(false);
        databasePage.SetActive(true);
        xp.text = "";
        level.text = "";
    }

    public void salvarDados() {
        StartCoroutine(updateUserName(login.text));
        StartCoroutine(updateXP(int.Parse(xp.text)));
        StartCoroutine(updateLevel(int.Parse(level.text)));
    }

    private IEnumerator updateUserName(string _username) {
        var DBTask = DBreference.Child("users").Child(login.text.Split('@')[0]).Child("email").SetValueAsync(_username);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if(DBTask.Exception != null) {
            Debug.LogWarning(message: $"failed to register task with {DBTask.Exception}");
        } else {
            //database username is now updated
        }
    }

    private IEnumerator updateXP(int _xp) {
        var DBTask = DBreference.Child("users").Child(login.text.Split('@')[0]).Child("xp").SetValueAsync(_xp);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null) {
            Debug.LogWarning(message: $"failed to register task with {DBTask.Exception}");
        } else {
            //database username is now updated
        }
    }

    private IEnumerator updateLevel(int _level) {
        var DBTask = DBreference.Child("users").Child(login.text.Split('@')[0]).Child("level").SetValueAsync(_level);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null) {
            Debug.LogWarning(message: $"failed to register task with {DBTask.Exception}");
        } else {
            //database username is now updated
        }
    }

    private IEnumerator loadUserData() {
        var DBTask = DBreference.Child("users").Child(login.text.Split('@')[0]).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null) {
            Debug.LogWarning(message: $"failed to load task with {DBTask.Exception}");
        } else if (DBTask.Result.Value == null) {
            xp.text = "0";
            level.text = "0";
        } else {
            DataSnapshot snapshot = DBTask.Result;
            print(snapshot.Child("xp").Value.ToString());
            print(snapshot.Child("level").Value.ToString());

            StartCoroutine(scoreboard());
        }
    }

    private IEnumerator scoreboard() {
        var DBTask = DBreference.Child("users").OrderByChild("level").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null) {
            Debug.LogWarning(message: $"failed to load score with {DBTask.Exception}");
        } else {
            DataSnapshot snapshot = DBTask.Result;

            foreach(DataSnapshot childSnaps in snapshot.Children){
                print("LEVEL = " + childSnaps.Child("level").Value);
            }
        }
    }
}
