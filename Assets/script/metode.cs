using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Greet();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Greet(string name = "No name pls enter name")
    {
        Debug.Log($"Hello ,{name}");
    }
}
