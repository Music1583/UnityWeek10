using UnityEngine;

public class loopLab : MonoBehaviour
{
    [SerializeField] int[] numbers = { 12, 7, 30, 4, 19, 25, 9, 18 };

    int sum;
    float average;
    int maximum;
    int minimum;
    [SerializeField] int[] counts = { 1,2};
    [SerializeField] int count = 5;
    [SerializeField] int energy = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       while (energy > 0)
        {
            Debug.Log("While: round ");
            energy--;
        }
        do
        {
            Debug.Log("Do_while : round");
            energy--;
        } while (energy > 0);
        while (energy < 50)
        {
            if (energy == 15)
            {
                Debug.Log("Exit loop");
                break;
            }
            energy++;
            Debug.Log($"Energy : {energy}");
        }
        System.Array.Sort(numbers);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
