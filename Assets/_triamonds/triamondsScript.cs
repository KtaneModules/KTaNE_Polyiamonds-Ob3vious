using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using System.Text.RegularExpressions;

public class triamondsScript : MonoBehaviour {

    public KMAudio Audio;
    public KMSelectable[] Buttons;
    public TextMesh Text;
    public KMBombModule Module;

    private bool solved;
    private int[] poscolour;
    private int[] pulsing;
    private int[] answerpos;
    private List<int> input = new List<int> { };
    private readonly string[] direction = { "top-left", "top-middle", "top-right", "bottom-right", "bottom-middle", "bottom-left" };
    private readonly string[] colourname = { "black", "red", "green", "yellow", "blue", "magenta", "cyan", "white" };
    private readonly float[,] colours = new float[8, 3] { { 0, 0, 0 }, { 1, 0, 0 }, { 0, 1, 0 }, { 1, 1, 0 }, { 0, 0, 1 }, { 1, 0, 1 }, { 0, 1, 1 }, { 1, 1, 1 } };

    static int _moduleIdCounter = 1;
    int _moduleID = 0;

    private KMSelectable.OnInteractHandler Press(int pos)
    {
        return delegate
        {
            Buttons[pos].AddInteractionPunch();
            Audio.PlaySoundAtTransform((input.Count() + 4).ToString(), Buttons[pos].transform);
            if (input.Contains(pos) || input.Count() == 3)
                input = new List<int> { };
            else
                input.Add(pos);
            return false;
        };
    }

    private void Hover(int pos, bool start)
    {
        if (start && !solved)
            Text.text = colourname[poscolour[pos]];
        else
            Text.text = "";
    }

    void Awake()
    {
        _moduleID = _moduleIdCounter++;
        for (int i = 0; i < 6; i++)
        {
            Buttons[i].OnInteract += Press(i);
            int x = i;
            Buttons[i].OnHighlight += delegate { Hover(x, true); };
            Buttons[i].OnHighlightEnded += delegate { Hover(x, false); };
        }
    }

    void Start () {
        Generate();
        for (int i = 0; i < 6; i++)
            Buttons[i].GetComponent<MeshRenderer>().material.color = new Color(colours[poscolour[i], 0], colours[poscolour[i], 1], colours[poscolour[i], 2]);
        StartCoroutine(Pulse());
    }

    private void Generate()
    {
        //initiating
        poscolour = Enumerable.Range(0, 8).ToList().Shuffle().ToArray();
        pulsing = Enumerable.Range(0, 6).ToList().Shuffle().Take(3).ToArray();
        int[] colour = pulsing.Select(x => poscolour[x]).ToArray();
        int[] orient = pulsing.Select(x => x % 2).ToArray();

        Debug.LogFormat("[Triamonds #{0}] Pulsing triangles are: {1}.", _moduleID, pulsing.Select(x => colourname[poscolour[x]] + "-" + new string[] { "up", "down" }[x % 2]).Join(", "));

        //flipping bad triangles
        if (orient.Take(2).Distinct().Count() == 1)
        {
            Debug.LogFormat("[Triamonds #{0}] Flipping triangle 2 and inverting its colour.", _moduleID);
            orient[1] = 1 - orient[1];
            colour[1] = 7 - colour[1];
            for (int i = 0; i < 3; i += 2)
                if (colour[i] == colour[1])
                {
                    Debug.LogFormat("[Triamonds #{0}] Inverting the colour of triangle {1} as well.", _moduleID, i + 1);
                    colour[i] = 7 - colour[i];
                }
            Debug.LogFormat("[Triamonds #{0}] Used triangles are: {1}.", _moduleID, Enumerable.Range(0, 3).Select(x => colourname[colour[x]] + "-" + new string[] { "up", "down" }[orient[x] % 2]).Join(", "));
        }

        //placing coloured triangles
        int[] index = new int[] { -1, -1, -1 };
        for (int i = 0; i < 3; i++)
            if (0 < colour[i] && colour[i] < 7)
                index[i] = (new int[] { 0, 2, 4, 4, 2, 0 }[colour[i] - 1] + 3 * orient[i]) % 6;
        Debug.LogFormat("[Triamonds #{0}] Placed triangles {1}.", _moduleID, Enumerable.Range(0, 3).Where(x => index[x] != -1).Select(x => x + 1).Join(" and "));

        bool isWrap = false;
        //other rules
        if (index.Count(x => x == -1) <= 1 && index.Distinct().Count() == 2)
        {
            Debug.LogFormat("[Triamonds #{0}] Triangles {1} overlapped. Fixed the placements.", _moduleID, Enumerable.Range(0, 3).Where(x => index.Count(y => y == index[x]) == 2).Select(x => x + 1).Join(" and "));
            int faulty = Enumerable.Range(0, 3).First(x => index.Count(y => y == index[x]) == 2 && new int[] { -1, 0, 0, 1, 0, 1, 1, -1 }[colour[x]] != orient[x]);
            int pos = index.First(x => x != -1);
            switch (index.Count(x => x == -1) == 0 ? -1 : colour[Array.IndexOf(index, -1)])
            {
                case 0:
                    index[Array.IndexOf(index, -1)] = (pos + 3) % 6;
                    index[faulty] = (pos + 2) % 6;
                    break;
                case 7:
                    index[Array.IndexOf(index, -1)] = (pos + 5) % 6;
                    index[faulty] = (pos + 4) % 6;
                    break;
                case -1:
                    index[faulty] = Enumerable.Range(0, 3).Select(x => x * 2 + orient[faulty]).First(x => !index.Contains(x) && (index.Contains((x + 1) % 6) || index.Contains((x + 5) % 6)));
                    break;
            }
        }
        else if (colour.Contains(0) && colour.Contains(7))
        {
            Debug.LogFormat("[Triamonds #{0}] Both black and white appeared.", _moduleID);
            int pos = index.First(x => x != -1);
            if (orient.Count(x => x == orient[Array.IndexOf(colour, 0)]) == 1)
            {
                index[Array.IndexOf(colour, 0)] = (pos + 3) % 6;
                index[Array.IndexOf(colour, 7)] = (pos + 4) % 6;
            }
            else if (orient.Count(x => x == orient[Array.IndexOf(colour, 7)]) == 1)
            {
                index[Array.IndexOf(colour, 0)] = (pos + 2) % 6;
                index[Array.IndexOf(colour, 7)] = (pos + 5) % 6;
            }
            else
            {
                index[Array.IndexOf(colour, 0)] = (pos + 3) % 6;
                index[Array.IndexOf(colour, 7)] = (pos + 1) % 6;
            }
        }
        else if (colour.Contains(0))
        {
            Debug.LogFormat("[Triamonds #{0}] Black appeared.", _moduleID);
            if (index.Any(x => x != -1 && index.Contains(x + 3)))
                index[Array.IndexOf(colour, 0)] = (index[Enumerable.Range(0, 3).First(x => index[x] != -1 && orient[x] == orient[Array.IndexOf(colour, 0)])] + 2) % 6;
            else if (orient.Count(x => x == orient[Array.IndexOf(colour, 0)]) == 1)
            {
                index = index.Select(x => x == -1 ? -1 : index.First(y => y != x && y != -1)).ToArray();
                index[Array.IndexOf(colour, 0)] = Enumerable.Range(0, 6).First(x => index.Contains((x + 1) % 6) && index.Contains((x + 5) % 6));
            }
            else
                index[Array.IndexOf(colour, 0)] = (index.First(x => x != -1 && x % 2 != orient[Array.IndexOf(colour, 0)]) + 3) % 6;
        }
        else if (colour.Contains(7))
        {
            Debug.LogFormat("[Triamonds #{0}] White appeared.", _moduleID);
            if (index.Any(x => x != -1 && index.Contains(x + 3)))
                index[Array.IndexOf(colour, 7)] = (index[Enumerable.Range(0, 3).First(x => index[x] != -1 && orient[x] != orient[Array.IndexOf(colour, 7)])] + 1) % 6;
            else if (orient.Count(x => x == orient[Array.IndexOf(colour, 7)]) == 1)
                index[Array.IndexOf(colour, 7)] = Enumerable.Range(0, 6).First(x => index.Contains((x + 1) % 6) && index.Contains((x + 5) % 6));
            else
                index[Array.IndexOf(colour, 7)] = Enumerable.Range(0, 3).Select(x => x * 2 + orient[Array.IndexOf(colour, 7)]).First(x => !index.Contains(x) && (index.Contains((x + 1) % 6) || index.Contains((x + 5) % 6)));
        }
        else
        {
            isWrap = true;
        }

        //wraparound
        if (index.Any(x => index.Contains(x + 3)))
        {
            Debug.LogFormat("[Triamonds #{0}] Triamond is not positioned correctly. Applying {1}.", _moduleID, (isWrap ? "wraparound" : "shift"));
            int offset = Enumerable.Range(0, 6).First(y => (index.All(z => (z + 5 * y) % 3 != 2)) && !index.Select(z => new int[] { 4, 3, -1, 5, 2, -1 }.Select(w => (w + y) % 6).ToArray()[(z + 5 * y) % 6]).Any(v => index.Select(z => new int[] { 4, 3, -1, 5, 2, -1 }.Select(w => (w + y) % 6).ToArray()[(z + 5 * y) % 6]).Contains(v + 3)));
            index = index.Select(x => (new int[] { 4, 3, -1, 5, 2, -1 }[(x + 5 * offset) % 6] + offset) % 6).ToArray();
        }

        answerpos = index;

        Debug.LogFormat("[Triamonds #{0}] Expected answer: {1}.", _moduleID, answerpos.Select(x => direction[x]).Join(", "));
    }

    private void CheckSolve()
    {
        bool good = true;
        for (int i = 0; i < 3; i++)
            good &= input[i] == answerpos[i];
        if (good)
        {
            Debug.LogFormat("[Triamonds #{0}] You submitted: {1}. That is correct. Module solved!", _moduleID, input.Select(x => direction[x]).Join(", "));
            Module.HandlePass();
            solved = true;
            Audio.PlaySoundAtTransform("7", Module.transform);
            Text.text = "";
            for (int i = 0; i < 6; i++)
                Buttons[i].GetComponent<MeshRenderer>().material.color = new Color(.5f, .5f, .5f);
        }
        else
        {
            Debug.LogFormat("[Triamonds #{0}] You submitted: {1}. That is incorrect. I expected: {2}. Strike!", _moduleID, input.Select(x => direction[x]).Join(", "), answerpos.Select(x => direction[x]).Join(", "));
            Module.HandleStrike();
            input = new List<int> { };
        }
    }

    private IEnumerator Pulse()
    {
        Vector3[] initpos = Buttons.Select(x => x.transform.localPosition).ToArray();
        float constant = 1.5f;
        while (!solved)
        {
            for (int i = 0; i < 3 && input.Count() == 0; i++)
            {
                for (float t = 0; t < 1 && input.Count() == 0; t += Time.deltaTime)
                {
                    Buttons[pulsing[i]].transform.localPosition = Vector3.Lerp(initpos[pulsing[i]], new Vector3(initpos[pulsing[i]].x * constant, initpos[pulsing[i]].y * constant, initpos[pulsing[i]].z * constant), t);
                    yield return null;
                }
                if (input.Count() == 0)
                    Buttons[pulsing[i]].transform.localPosition = new Vector3(initpos[pulsing[i]].x * constant, initpos[pulsing[i]].y * constant, initpos[pulsing[i]].z * constant);
            }
            for (float t = 0; t < 1 && input.Count() == 0; t += Time.deltaTime)
            {
                for (int i = 0; i < 3 && input.Count() == 0; i++)
                    Buttons[pulsing[i]].transform.localPosition = Vector3.Lerp(new Vector3(initpos[pulsing[i]].x * constant, initpos[pulsing[i]].y * constant, initpos[pulsing[i]].z * constant), initpos[pulsing[i]], t);
                yield return null;
            }
            for (int i = 0; i < 3 && input.Count() == 0; i++)
                Buttons[pulsing[i]].transform.localPosition = initpos[pulsing[i]];
            if (input.Count() != 0)
            {
                while (input.Count() != 0 && (input.Count() != 3 || Mathf.Abs(Buttons[input[2]].transform.localPosition.z) < Mathf.Abs(initpos[input[2]].z * constant)))
                {
                    for (int i = 0; i < 6; i++)
                        if (input.Contains(i))
                            if (Mathf.Abs(Buttons[i].transform.localPosition.z) < Mathf.Abs(initpos[i].z * constant))
                                Buttons[i].transform.localPosition = Vector3.Lerp(initpos[i], new Vector3(initpos[i].x * constant, initpos[i].y * constant, initpos[i].z * constant), (Buttons[i].transform.localPosition.z - initpos[i].z) / (initpos[i].z * (constant - 1f)) + Time.deltaTime);
                            else
                                Buttons[i].transform.localPosition = new Vector3(initpos[i].x * constant, initpos[i].y * constant, initpos[i].z * constant);
                        else if (Mathf.Abs(Buttons[i].transform.localPosition.z) > Mathf.Abs(initpos[i].z))
                                Buttons[i].transform.localPosition = Vector3.Lerp(new Vector3(initpos[i].x * constant, initpos[i].y * constant, initpos[i].z * constant), initpos[i], (initpos[i].z * constant - Buttons[i].transform.localPosition.z) / (initpos[i].z * (constant - 1f)) + Time.deltaTime);
                        else
                            Buttons[i].transform.localPosition = initpos[i];
                    yield return null;
                }
                while (Enumerable.Range(0, 6).ToList().Count(x => Mathf.Abs(Buttons[x].transform.localPosition.z) > Mathf.Abs(initpos[x].z)) != 0 && (input.Count() == 0 || input.Count() == 3))
                {
                    for (int i = 0; i < 6; i++)
                        if (Mathf.Abs(Buttons[i].transform.localPosition.z) > Mathf.Abs(initpos[i].z))
                            Buttons[i].transform.localPosition = Vector3.Lerp(initpos[i], new Vector3(initpos[i].x * constant, initpos[i].y * constant, initpos[i].z * constant), (Buttons[i].transform.localPosition.z - initpos[i].z) / (initpos[i].z * (constant - 1f)) - Time.deltaTime);
                        else
                            Buttons[i].transform.localPosition = initpos[i];
                    yield return null;
                }
                for (int i = 0; i < 6 && (input.Count() == 0 || input.Count() == 3); i++)
                    Buttons[i].transform.localPosition = initpos[i];
                if (input.Count() == 0)
                    yield return null;
                else if (input.Count() == 3)
                    CheckSolve();
            }
        }
    }

    IEnumerator TwitchHighlight(int pos)
    {
        Buttons[pos].OnHighlight();
        for (float t = 0f; t < 1; t += Time.deltaTime)
            yield return null;
        Buttons[pos].OnHighlightEnded();
    }

#pragma warning disable 414
    private string TwitchHelpMessage = "'!{0} inspect tl bm tr' to hover over those triangles, '!{0} press tl bm tr' to press those triangles.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;
        command = command.ToLowerInvariant();
        if (Regex.IsMatch(command, @"^(inspect|press)(\s[tb][lmr])+$"))
        {
            string[] set = { "tl", "tm", "tr", "br", "bm", "bl" };
            MatchCollection matches = Regex.Matches(command, @"[tb][lmr]");
            foreach (Match match in matches)
                foreach (Capture capture in match.Captures)
                    if (command.Split(' ')[0] == "press")
                    {
                        Buttons[Array.IndexOf(set, capture.ToString())].OnInteract();
                        yield return null;
                    }
                    else
                    {
                        StartCoroutine(TwitchHighlight(Array.IndexOf(set, capture.ToString())));
                        for (float t = 0f; t < 1; t += Time.deltaTime)
                            yield return "trycancel Highlighting triangles has been stopped.";
                    }
            yield return "strike";
            yield return "solve";
        }
        else
            yield return "sendtochaterror Invalid command.";
        yield return null;
    }
   
    IEnumerator TwitchHandleForcedSolve()
    {
        yield return true;
        if (input.Count() != 0)
        {
            Buttons[input.First()].OnInteract();
            yield return null;
        }
        for (int i = 0; i < 3; i++)
        {
            Buttons[answerpos[i]].OnInteract();
            yield return null;
        }
    }
}
