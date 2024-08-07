﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;

public class buttonsScript : MonoBehaviour
{

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
    public int stage = 0;
    public int stage12 = 1;
    public int ontimer;


    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;
    private bool incorrect;

    void Awake()
    {
        moduleId = moduleIdCounter++;

        foreach (KMSelectable button in buttons)
        {
            KMSelectable pressed = button;
            button.OnInteract += delegate () { PressedButton(pressed); return false; };
            button.OnInteractEnded += delegate () { ReleasedButton(pressed); };
        }

    }

    // Use this for initialization
    void Start()
    {
        ColorstaticCopy();
        RandomButtonColor();
        RandomNumberText();
        GenerateList();
        float scalar = transform.lossyScale.x;
        led.range *= scalar;
    }
    void ColorstaticCopy()
    {
        for (int i = 0; i < 4; i++)
        {
            colorstatic[i] = colors[i];
        }

    }

    void GenerateList()
    {
        count = UnityEngine.Random.Range(4, 8);
        //Press the top left button once.
        if (count >= 1)
        {
            count = count - 1;
            listtopress.Add(buttons[2]);
        }
        //If the number on the green button and the number on the blue button differ by more than 3, press the blue button once.
        if (count >= 1 && (number[Array.IndexOf(colors, colorstatic[2])] - number[Array.IndexOf(colors, colorstatic[3])] > 3 || number[Array.IndexOf(colors, colorstatic[2])] - number[Array.IndexOf(colors, colorstatic[3])] < -3))
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
        if (count >= 1 && Bomb.GetSerialNumberLetters().All(x => x != 'R' && x != 'S' && x != 'T') == false)
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
        if (count >= 1 && number[Array.IndexOf(colors, colorstatic[3])] > Bomb.GetPortCount(Port.Parallel))
        {
            int min = 10;
            int countmin = 1;
            foreach (int num in number)
            {
                if (num <= min)
                {
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
                listtopress.Add(buttons[Array.IndexOf(number, min)]);
            }
        }
        //If count is not reached yet
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
        if (Array.IndexOf(colors, colorstatic[1]) == 2)
        {
            if (colors[0] == colorstatic[0]) //bottomleft=red
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
        //log listtopress
        for (int i = 0; i < listtopress.Count; i++)
        {
            Debug.LogFormat("[4 Buttons #{0}] The order to press {1}", moduleId, i + 1);
            Debug.LogFormat("[4 Buttons #{0}] The button to press {1}", moduleId, listtopress[i]);
        }
    }

    void RandomNumberText()
    {
        for (int i = 0; i < 4; i++)
        {
            number[i] = UnityEngine.Random.Range(1, 10);
            buttontext[i].text = number[i].ToString();
        }
    }


    void RandomButtonColor()
    {
        Material[] colorcopyforrandom = new Material[4];
        int[] randomindex = { 70, 70, 70, 70 };
        int j = 0;
        bool statuss = true;
        while (statuss)
        {
            int x = UnityEngine.Random.Range(0, 4);
            if (Array.IndexOf(randomindex, 70) == -1)
            {
                statuss = false;
            }
            if (Array.IndexOf(randomindex, x) == -1)
            {
                randomindex[j] = x;
                j++;
            }
        }
        int l = 0;
        foreach (int k in randomindex)
        {
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
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);


        //passes or strikes
        if (stage12 == 1)
        {
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
            if (stage >= listtopress.Count || stage > 7)
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

    //twitch plays
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"To press the buttons on the module (in reading order), use !{0} press [1-4] (The numbers can be chained. Example: !{0} press 1423141321) | To hold and release a button on the module, use !{0} hold [1-4] until [0-9] or !{0} hold [1-4] then release";
#pragma warning restore 414

    int[] ButtonSort = { 2, 3, 0, 1 };
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length != 2)
            {
                yield return "sendtochaterror Parameter length invalid. Command ignored.";
                yield break;
            }

            if (parameters[1].Length == 0)
            {
                yield return "sendtochaterror Number length invalid. Command ignored.";
                yield break;
            }

            for (int x = 0; x < parameters[1].Length; x++)
            {
                int Out;
                if (!int.TryParse(parameters[1][x].ToString(), out Out))
                {
                    yield return "sendtochaterror A number given is not valid. Command ignored.";
                    yield break;
                }

                if (Out < 1 || Out > 4)
                {
                    yield return "sendtochaterror A number given is not 1-4. Command ignored.";
                    yield break;
                }
                buttons[ButtonSort[Out - 1]].OnInteract();
                yield return new WaitForSecondsRealtime(0.1f);
                buttons[ButtonSort[Out - 1]].OnInteractEnded();
                yield return new WaitForSecondsRealtime(0.1f);
                if (stage12 == 2 && x != parameters[1].Length - 1)
                {
                    yield return "sendtochat Input has been interrupted due to the LED turning on on Module {1} (4 Buttons).";
                    yield break;
                }
            }
        }

        if (Regex.IsMatch(parameters[0], @"^\s*hold\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            int Out;
            if (parameters.Length != 4)
            {
                yield return "sendtochaterror Parameter length invalid. Command ignored.";
                yield break;
            }

            if (parameters[1].Length == 0)
            {
                yield return "sendtochaterror Number length invalid. Command ignored.";
                yield break;
            }

            if (!int.TryParse(parameters[1].ToString(), out Out))
            {
                yield return "sendtochaterror A number given is not valid. Command ignored.";
                yield break;
            }

            if (Out < 1 || Out > 4)
            {
                yield return "sendtochaterror The button number given is not 1-4. Command ignored.";
                yield break;
            }

            if (Regex.IsMatch(parameters[2], @"^\s*until\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                int Out2;
                if (!int.TryParse(parameters[3].ToString(), out Out2))
                {
                    yield return "sendtochaterror The timer number given is not valid. Command ignored.";
                    yield break;
                }

                if (Out2 < 0 || Out2 > 9)
                {
                    yield return "sendtochaterror The number given is not 0-9. Command ignored.";
                    yield break;
                }

                buttons[ButtonSort[Out - 1]].OnInteract();
                while (((int)Bomb.GetTime()) % 10 != Out2)
                {
                    yield return new WaitForSecondsRealtime(0.1f);
                }
                buttons[ButtonSort[Out - 1]].OnInteractEnded();
            }

            if (Regex.IsMatch(parameters[2], @"^\s*then\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) && Regex.IsMatch(parameters[3], @"^\s*release\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                buttons[ButtonSort[Out - 1]].OnInteract();
                yield return new WaitForSecondsRealtime(0.5f);
                buttons[ButtonSort[Out - 1]].OnInteractEnded();
            }
        }
    }
}

