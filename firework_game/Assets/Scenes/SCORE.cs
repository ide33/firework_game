using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class SCORE : MonoBehaviour
{
    public GameObject score_object = null;
    public int score_num = 0;
    void Start()
    {
        
    }

    
    void Update()
    {
        Text score_text = score_object.GetComponent<Text>();
        score_text.text = "SCORE:" + score_num;
        score_num += 10;
    }
}
