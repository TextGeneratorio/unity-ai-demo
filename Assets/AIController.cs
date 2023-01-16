using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
// using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/*
 * A Text Generation hub Embedding
Python
Language
import requests

headers = {"secret": ""}

data = {
    "text": "The battle between the elves and goblins has begun,",
    "number_of_results": 1,
    "max_length": 100,
    "max_sentences": 4,
    "min_probability": 0,
    "stop_sequences": [],
    "top_p": 0.9,
    "top_k": 40,
    "temperature": 0.7,
    "repetition_penalty": 1.17,
    "seed": 0
}

response = requests.post(
   "https://api.text-generator.io/api/v1/generate",
   json=data,
   headers=headers
)

json_response = response.json()

for generation in json_response:
    generated_text = generation["generated_text"][len(data['text']):]
    print(generated_text)
Close
 */
public class AIController: MonoBehaviour
{
    public TMP_InputField input;
    public TMP_InputField secret;
    public Button submitButton;
    public TMP_Text output;
    
    void Start()
    {
        submitButton.onClick.AddListener(SubmitForm);
    }
    
    public void SubmitForm()
    {
        string textInput = input.text;
        string secretInput = secret.text;
        Debug.Log("Form submitted");
        string url = "https://api.text-generator.io/api/v1/generate";
        // post json

        Dictionary<string, string> json = new Dictionary<string, string>(); 
        json.Add("text", textInput);
        json.Add("number_of_results", "1");
        json.Add("max_length", "100");
        json.Add("max_sentences", "4");
        json.Add("min_probability", "0");
        json.Add("top_p", "0.9");
        json.Add("top_k", "40");
        json.Add("temperature", "0.7");
        json.Add("repetition_penalty", "1.17");
        json.Add("seed", "0");

        UnityWebRequest www = Post(url, json);
        www.SetRequestHeader("secret", secretInput);
        www.SendWebRequest();
        while (!www.isDone)
        {
            // Debug.Log("Waiting for response");
        }
        if( www.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError )
        {
            Debug.Log(www.error);
            Debug.Log(www.downloadHandler.text);
        }
        else
        {
            // Show results as text
            Debug.Log(www.downloadHandler.text);
            // get json response
            string jsonText = www.downloadHandler.text;
            // parse json to list of JSONTextGeneratorResponse objects
            JsonReader jr = new JsonTextReader(new StringReader(jsonText));
            JsonSerializer serializer = new JsonSerializer();
            List<JSONTextGeneratorResponseItem> response = serializer.Deserialize<List<JSONTextGeneratorResponseItem>>(jr);
            // get generated text 
            string generatedText = response[0].generated_text;
            output.text = generatedText;
        }
    }
    public string DictToJSON(Dictionary<string, string> dict)
    {
        string json = "{";
        foreach (KeyValuePair<string, string> entry in dict)
        {
            string value = entry.Value.Replace("\u000b", "\\n").Replace(@"
", "\\n");
            
            json += "\"" + entry.Key + "\":\"" + value + "\",";
        }
        json = json.Substring(0, json.Length - 1);
        json += "}";
        return json;
    }
    
    public UnityWebRequest Post(string url, Dictionary<string, string> form)
    {
        
        UnityWebRequest www = new UnityWebRequest(url, "POST");
        www.downloadHandler = new DownloadHandlerBuffer();
        string json = DictToJSON(form);
        Debug.Log(json);
        www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Accept", "application/json");
        return www;
    }
}

class JSONTextGeneratorResponseItem
{
    public string generated_text;
    public string stop_reason;
}