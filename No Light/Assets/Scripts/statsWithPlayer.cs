using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class statsWithPlayer : MonoBehaviour
{

    // HP points

    [SerializeField] private int Hp = 100;                          // если < 0 = смерть
    [SerializeField] private float conscious = 100;                 // если меньше 20%  



    // hunger  их потом можно не выводить даже в инспектор 
    // измеряется в каллориях

    [SerializeField] private float hungerFood = 300;
    [SerializeField] private float hungerAqua = 300;






    void HungerFoodExpense()
    {

    }

    void FoodIndex()
    {
        switch (hungerFood) // work - add UX UI
        {
            case (>= 0) when hungerFood >= 0 && hungerFood < 100: 
                Debug.Log("0-100");
                break;
            case (>= 101) when hungerFood >= 101 && hungerFood < 1000:
                Debug.Log("101 - 1000");
                break;
            case (>= 1001) when hungerFood >= 1001 && hungerFood < 2500:
                Debug.Log("1001 - 2500");
                break;
            case (>= 2501) when hungerFood >= 2501 && hungerFood < 4500:
                Debug.Log("2501 - 4500");
                break;
            case (>= 4501) when hungerFood >= 4501 && hungerFood < 7999:
                Debug.Log("4501 - 7999");
                break;
            case (>= 8000) when hungerFood >= 8000:
                Debug.Log("> 8000");
                break;
        }    
    }



    void Start()
    {
        
    }

    
    void Update()
    {
        FoodIndex(); 
    }
}
