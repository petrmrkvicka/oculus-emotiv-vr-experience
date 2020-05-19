using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


using NativeWebSocket;


[Serializable]
public class ResultObj {
    public MetricsArray met;
    public string sid;
    public string time;
}

[Serializable]
public class MetricsArray {
    public bool engAct;
    public float eng;
    public bool excAct;
    public float exc;
    public float lex;
    public bool strAct;
    public float str;
    public bool relAct;
    public float rel;
    public bool intAct;
    public float inte;
    public bool focAct;
    public float foc;
}

[Serializable]
public class LoginJSONArray {
    public int id;
    public string jsonrpc;
    public string method;
    public ResultArrayJson [] result;
    public ParametersArrayJson @params;
}

[Serializable]
public class LoginJSON {
    public int id;
    public string jsonrpc;
    public string method;
    public ResultArrayJson result;
    public ParametersArrayJson @params;
}


[Serializable]
public class ParametersArrayJson {
    public string clientId;
    public string clientSecret;
    public int debit;
    public string cortexToken;
    public string headset;
    public string status;
}

[Serializable]
public class ResultArrayJson {
    public string currentOSUId;
    public string currentOSUsername;
    public string lastLoginTime;
    public string cortexToken;
    public string warning; 
    public string id; 
    public object [] success; 
}


[Serializable]

public class ConnectHeadset : MonoBehaviour
{
    WebSocket websocket;
    private string messageGlobal;
    public string clientSecret;
    public string clientId; 
    protected string cortexToken; 
    protected string typeOfFn;
    protected string headsetId;
    protected string sessionId;
    protected GameObject headsetData;

    public GameObject textBlock;

    // Start is called before the first frame update
    void Start() {

        GameObject.DontDestroyOnLoad(this.gameObject);
    }

    public void NextLevel() {
        Debug.Log("next levet");
        SceneManager.LoadScene("Dashboard");
    }

    public async void ConnectEmotiv()
    {
        Debug.Log("Start");

        websocket = new WebSocket("wss://localhost:6868");

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            // encode message
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log(message);

            messageGlobal = message;


            if(typeOfFn == "array") {
                // if json data has an array
                var jsonMessage = JsonUtility.FromJson<LoginJSONArray>(message);
                messageGlobal = jsonMessage.ToString();

                // write username
                if(jsonMessage.result[0].currentOSUsername != null){
                    messageGlobal = "Loaded user";
                    Debug.Log("username");
                    Debug.Log(jsonMessage.result[0].currentOSUsername);
                    Invoke("getHeadsets", 2.0f);
                } else if(jsonMessage.result[0].id != null) {
                    messageGlobal = "Loaded headset";
                    headsetId = jsonMessage.result[0].id;
                    Debug.Log("Headset");
                    Debug.Log(headsetId);
                    Invoke("authorizeLogin", 2.0f);
                } else {
                    messageGlobal = "Other";
                    Debug.Log("other");
                    Debug.Log(jsonMessage.result[0]);
                }

            } else if(typeOfFn == "subscr") {
                Debug.Log("GETTING SUBSCRIPTION!");
                ResultObj jsonMessage = JsonUtility.FromJson<ResultObj>(message);
                MetricsArray metrics = jsonMessage.met;
                if(SceneManager.GetActiveScene().name != "Dashboard")
                    SceneManager.LoadScene("Dashboard");

                messageGlobal = "Stress-o-meter: " + metrics.str.ToString("#.00");
            
            } else {
                var jsonMessage = JsonUtility.FromJson<LoginJSON>(message);
                messageGlobal = jsonMessage.ToString();

                // write token 
                if(jsonMessage.result.cortexToken != null){
                    messageGlobal = "Authorized connection";
                    Debug.Log("cortexToken");
                    cortexToken = jsonMessage.result.cortexToken;
                    Debug.Log(cortexToken);
                    Invoke("createSession", 2.0f);
                } else if(jsonMessage.result.id != null) {
                    messageGlobal = "Created session";
                    Debug.Log("sessionId");
                    sessionId = jsonMessage.result.id;
                    Debug.Log(sessionId);
                    Invoke("subscribeData", 2.0f);
                } else if(jsonMessage.result.success != null) {
                    messageGlobal = "Connection successful!";
                    SceneManager.LoadScene("Dashboard");
                    Debug.Log("streams");
                    messageGlobal = jsonMessage.result.success.ToString();

                } else {
                    messageGlobal = "Connection successful!";
                    if(SceneManager.GetActiveScene().name == "LoadEmotiv") {
                        SceneManager.LoadScene("Dashboard");
                    }
                    Debug.Log("other");
                    Debug.Log(jsonMessage.result);  
                }
            }
        };

        // get user login request
        Invoke("getUserLogin", 3.0f);        

        // waiting for messages
        await websocket.Connect();
    }


    async void getUserLogin()
    {
        if (websocket.State == WebSocketState.Open)
        {
            // initiate request class
            LoginJSON loginData = new LoginJSON();
            loginData.id = 1;
            loginData.jsonrpc = "2.0";
            loginData.method = "getUserLogin";
            // stringify
            string json = JsonUtility.ToJson(loginData);

            string jsonString = "{\"id\": 1, \"jsonrpc\": \"2.0\", \"method\": \"getUserLogin\"}";
            Debug.Log(jsonString);

            typeOfFn = "array";

            // send plain text
            await websocket.SendText(jsonString);
        }
    }

    async void authorizeLogin()
    {
        if (websocket.State == WebSocketState.Open)
        {
            // initiate request class

            ParametersArrayJson parameters = new ParametersArrayJson();
            parameters.clientId = clientId;
            parameters.clientSecret = clientSecret;
            parameters.debit = 10;
            
            LoginJSON loginData = new LoginJSON();
            loginData.id = 1;
            loginData.jsonrpc = "2.0";
            loginData.method = "authorize";
            loginData.@params = parameters;

            // stringify
            string jsonString = "{\"id\": 1, \"jsonrpc\": \"2.0\", \"method\": \"authorize\", \"params\": {\"clientId\": \"" + clientId + "\", \"clientSecret\": \"" + clientSecret + "\", \"debit\": 10}}";

            // string json = JsonUtility.ToJson(loginData);
            string json = jsonString;

            Debug.Log("authorize json");
            Debug.Log(json);

            typeOfFn = "object";

            // send plain text
            await websocket.SendText(json);
        }
    }


    async void getHeadsets()
    {
        if (websocket.State == WebSocketState.Open)
        {
            // stringify
            string json = "{\"id\": 1, \"jsonrpc\": \"2.0\", \"method\": \"queryHeadsets\"}";
            Debug.Log(json);

            typeOfFn = "array";

            // send plain text
            await websocket.SendText(json);
        }
    }

    async void createSession()
    {
        if (websocket.State == WebSocketState.Open)
        {

            // stringify
            string jsonString = "{\"id\": 1, \"jsonrpc\": \"2.0\", \"method\": \"createSession\", \"params\": {\"cortexToken\": \"" + cortexToken + "\", \"headset\": \"" + headsetId + "\", \"status\": \"active\"}}";

            // string json = JsonUtility.ToJson(loginData);
            string json = jsonString;

            Debug.Log("createSessionJson");
            Debug.Log(json);

            typeOfFn = "no";

            // send plain text
            await websocket.SendText(json);

        }
    }

    async void subscribeData() 
    {
        if (websocket.State == WebSocketState.Open)
        {
            
            // stringify
            string jsonString = "{\"id\": 1, \"jsonrpc\": \"2.0\", \"method\": \"subscribe\", \"params\": {\"cortexToken\": \"" + cortexToken + "\", \"session\": \"" + sessionId + "\", \"streams\": [\"met\"]}}";
            //, \"fac\", \"eeg\"

            // string json = JsonUtility.ToJson(loginData);
            string json = jsonString;

            Debug.Log("subscribe");
            Debug.Log(json);

            typeOfFn = "subscr";

            // send plain text
            await websocket.SendText(json);

        }
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }

    private void Update() {
        Text updatesText = textBlock.GetComponent<Text>();
        updatesText.text = messageGlobal;
    }


}