using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public Slider healthSlider;
    public Slider staminaSlider;
    public Slider chargeSlider;


    private void Awake()
    {
        instance = this;
    }
}
