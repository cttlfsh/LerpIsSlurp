using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Achievement
{
    public string title { get; private set; }
    public string description { get; private set; }
    public Image image { get; private set; }
    public bool achieved { get; set; }

    public Achievement(string tit, string desc, Image img)
    {
        title = tit;
        description = desc;
        image = img;
    }

    public void CheckCompletion()
    {
        // da cambiare con le PlayerPrefs
        if (achieved)
        {
            return;
        }
    }

}
