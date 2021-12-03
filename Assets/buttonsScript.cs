using KModkit;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class buttonsScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSoundOverride Sound;

    public KMSelectable[] buttons;
    public Material[] colors;
    public Material[] colorstatic = new Material[4];
    public Renderer[] colorstatus;
    public TextMesh[] buttontext;
    public Light led;
    public KMSelectable finalbutton;





    public List<KMSelectable> listtopress = new List<KMSelectable>();

    public int[] number = new int[4];
    static public int[] prime = { 2, 3, 5, 7 };
    public int count = 0;
    public int countcopy = 0;
    public int stage = 0;
    public int stage12 = 1;
    public int ontimer;


    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;
    private bool incorrect;

    void Awake ()
    {
        moduleId = moduleIdCounter++;

        foreach (KMSelectable button in buttons)
        {
            KMSelectable pressed = button;
            button.OnInteract += delegate () { PressedButton(pressed); return false; };
            button.OnInteractEnded += delegate () { ReleasedButton(pressed);};
        }

    }

	// Use this for initialization
	void Start () {
        ColorstaticCopy();
        RandomButtonColor();
        RandomNumberText();
        GenerateList();

    }
    void ColorstaticCopy()
    {
        for (int i=0;i<4;i++)
        {
            colorstatic[i]=colors[i];
        }

    }

    void GenerateList()
    {
        count = UnityEngine.Random.Range(4, 8);
        countcopy = count;
        //Press the top left button once.
        if (count >= 1)
        {
            count = count - 1;
            listtopress.Add(buttons[2]);
        }
        //If the number on the green button and the number on the blue button differ by more than 3, press the blue button once.
        if (count >= 1 && (number[Array.IndexOf(colors, colorstatic[2])]- number[Array.IndexOf(colors, colorstatic[3])]>3 || number[Array.IndexOf(colors, colorstatic[2])] - number[Array.IndexOf(colors, colorstatic[3])] < -3))
        {
            count = count - 1;
            listtopress.Add(buttons[Array.IndexOf(colors, colorstatic[3])]); //Add blue to listtopress
        }
        //If a lit indicator SIG is present, press the top right button.
        if (count >= 1 && Bomb.IsIndicatorOn(Indicator.SIG))
        {
            count = count - 1;
            listtopress.Add(buttons[3]);
        }
        //If the serial number contains the letter R,S, or T, press the red button 2 times.
        if (count >= 1 && Bomb.GetSerialNumberLetters().All(x => x != 'R' && x != 'S' && x != 'T')==false)
        {
            for (int i = 0; i < 2; i++)
            {
                if (count >= 1)
                {
                    count = count - 1;
                    listtopress.Add(buttons[Array.IndexOf(colors, colorstatic[0])]); //Add the red button to the listtopress
                }
            }
        }
        //If the number on the green button is prime, press the green button by the amount of AA batteries on the bomb.
        if (count >= 1 && Array.IndexOf(prime, number[Array.IndexOf(colors, colorstatic[2])]) != -1)
        {
            for (int i = 0; i < Bomb.GetBatteryCount(Battery.AA); i++)
            {
                if (count >= 1)
                {
                    count = count - 1;
                    listtopress.Add(buttons[Array.IndexOf(colors, colorstatic[2])]); //Add the green button to the listtopress
                }
            }
        }
        //Press the yellow button once.
        if (count >= 1)
        {
            count = count - 1;
            listtopress.Add(buttons[Array.IndexOf(colors, colorstatic[1])]); //Add the yellow button to the listtopress
        }
        //If the number on the blue button is greater than the number of parallel ports on the bomb, press the button that has the lowest number on it, if there are duplicates, press the top right button twice instead.
        if (count >= 1 && number[Array.IndexOf(colors, colorstatic[3])]>Bomb.GetPortCount(Port.Parallel))
        {
            int min = 10;
            int countmin = 1;
            foreach(int num in number)
            {
                if (num <= min){
                    if (num < min)
                    {
                        min = num;
                        countmin = 1;
                    }
                    else
                    {
                        countmin++;
                    }
                }
            }
            if (countmin > 1)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (count >= 1)
                    {
                        count = count - 1;
                        listtopress.Add(buttons[3]); //Add the top right button to the listtopress
                    }
                }
            }
            else
            {
                count = count - 1;
                listtopress.Add(buttons[Array.IndexOf(number,min)]);
            }
            Debug.Log(countmin);
        }
        //If there are less than 2 RCA and with needy
        if (Bomb.IsDuplicatePortPresent(Port.StereoRCA) == false && Bomb.GetModuleNames().All(x => Bomb.GetSolvableModuleNames().Contains(x)))
        {
            int evenorodd = count % 2;
            while (count >= 1)
            {
                count = count - 1;
                if (count % 2 != evenorodd)
                {
                    listtopress.Add(buttons[1]);
                }
                else
                {
                    listtopress.Add(buttons[3]);
                }
            }
        }
        if (Array.IndexOf(colors, colorstatic[1]) == 2)
        {
            if (colors[0]==colorstatic[0]) //bottomleft=red
            {
                finalbutton = buttons[0];
                ontimer = 6;
            }
            else if (colors[0] == colorstatic[2]) //bottomleft=lime
            {
                finalbutton = buttons[3];
                ontimer = 5;
            }
            else if (colors[0] == colorstatic[3]) //bottomleft=blue
            {
                finalbutton = buttons[Array.IndexOf(colors, colorstatic[3])];
                ontimer = 9;
            }
        }
        else if (Array.IndexOf(colors, colorstatic[1]) == 3)
        {
            if (colors[0] == colorstatic[0]) //bottomleft=red
            {
                finalbutton = buttons[Array.IndexOf(colors, colorstatic[0])];
                ontimer = 7;
            }
            else if (colors[0] == colorstatic[2]) //bottomleft=lime
            {
                finalbutton = buttons[Array.IndexOf(colors, colorstatic[2])];
                ontimer = 3;
            }
            else if (colors[0] == colorstatic[3]) //bottomleft=blue
            {
                finalbutton = buttons[2];
                ontimer = 0;
            }
        }
        else if (Array.IndexOf(colors, colorstatic[1]) == 1)
        {
            if (colors[0] == colorstatic[0]) //bottomleft=red
            {
                finalbutton = buttons[2];
                ontimer = 2;
            }
            else if (colors[0] == colorstatic[2]) //bottomleft=lime
            {
                finalbutton = buttons[Array.IndexOf(colors, colorstatic[3])];
                ontimer = 4;
            }
            else if (colors[0] == colorstatic[3]) //bottomleft=blue
            {
                finalbutton = buttons[Array.IndexOf(colors, colorstatic[2])];
                ontimer = 1;
            }
        }
        else if (Array.IndexOf(colors, colorstatic[1]) == 0)
        {
            finalbutton = buttons[Array.IndexOf(colors, colorstatic[1])];
            ontimer = 50;
        }
    }

    void RandomNumberText()
    {
        for(int i=0; i < 4; i++)
        {
            number[i] = UnityEngine.Random.Range(1, 10);
            buttontext[i].text = number[i].ToString();
        }
    }


    void RandomButtonColor()
    {
        Material[] colorcopyforrandom = new Material[4];
        int[] randomindex = {70,70,70,70};
        int j = 0;
        bool statuss = true;
        while (statuss)
        {
            int x = UnityEngine.Random.Range(0, 4);
            if (Array.IndexOf(randomindex, 70)==-1)
            {
                statuss = false;
            }
            if (Array.IndexOf(randomindex, x) == -1)
            {
               randomindex[j]=x;
                j++;
            }
        }
        int l = 0;
        foreach (int k in randomindex){
            colors[l] = colorstatic[k];
            l++;
        }
        for (int i = 0; i < 4; i++)
        {
            colorstatus[i].material = colors[i];
        }

    }


    void PressedButton(KMSelectable button)
    {

        if (moduleSolved)
        {
            return;
        }
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress,transform);

        for (int i = 0; i < listtopress.Count; i++)
        {
            Debug.Log(i.ToString() + listtopress[i].ToString());
        }
        if (colorstatic == colors) {
            Debug.Log("Something's wrong");
        }
        /*/for(int i=0;i < Bomb.GetSolvableModuleNames().Count; i++)
        {
            Debug.Log(Bomb.GetSolvableModuleNames()[i]);
            Debug.Log(Bomb.GetModuleNames()[i]);
        }/*/
        if (Bomb.GetModuleNames().All(x => Bomb.GetSolvableModuleNames().Contains(x)))
        {
            Debug.Log("No needy");
        }
        //getting text from a button
        //(button).GetComponentInChildren<TextMesh>().text





        //passes or strikes
        if (stage12 == 1) { 
            KMSelectable correct = listtopress[stage];

            if (button != correct)
            {
                incorrect = true;
            }
            stage++;
            if (incorrect)
            {
                stage = 0;
                GetComponent<KMBombModule>().HandleStrike();
                incorrect = false;
            }
            if (stage == countcopy)
            {
                stage12 = 2;
                led.enabled = true;
            }
        }
    }

    void ReleasedButton(KMSelectable button)
    {
        if (moduleSolved)
        {
            return;
        }
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, transform);
        if (stage12 == 2)
            {
                if (!incorrect)
            {
                incorrect = true;
                return;
            }
                if (button == finalbutton)
                {
                    if (ontimer == 50 || Bomb.GetFormattedTime().IndexOf(ontimer.ToString()) != -1)
                    {
                        moduleSolved = true;
                        GetComponent<KMBombModule>().HandlePass();
                    }
                    else
                    {
                        GetComponent<KMBombModule>().HandleStrike();
                    }
                }
                else
                {
                    GetComponent<KMBombModule>().HandleStrike();
                }
         }
    }
}

    /*/
	// Update is called once per frame
	void Update () {
		
	}/*/

