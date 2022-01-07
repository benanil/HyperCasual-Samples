using UnityEngine;
using UnityEngine.UI;

public class Fps : MonoBehaviour
{
    Text text;

    void Start()
    {
        text = GetComponent<Text>();    
    }

    void Update()
    {
        text.text = $"{1 / Time.deltaTime:0.0}fps";
    }
}
