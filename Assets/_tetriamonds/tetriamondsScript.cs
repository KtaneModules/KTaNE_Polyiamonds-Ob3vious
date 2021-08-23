using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using System.Text.RegularExpressions;

public class tetriamondsScript : MonoBehaviour {

    public KMAudio Audio;
    public KMSelectable[] Buttons;
    public TextMesh Text;
    public KMBombModule Module;

    private bool solved;
    private int[] poscolour;
    private int[] pulsing;
    private int[] answerpos;
    private List<int> input = new List<int> { };
    private readonly string[] direction = { "top-left", "top-right", "middle-right", "bottom-right", "bottom-left", "middle-left" };
    private readonly string[] colourname = { "orange", "lime", "jade", "azure", "violet", "rose", "grey" };
    private readonly float[,] colours = new float[7, 3] { { 1, .5f, 0 }, { .5f, 1, 0 }, { 0, 1, .5f }, { 0, .5f, 1 }, { .5f, 0, 1 }, { 1, 0, .5f }, { .5f, .5f, .5f } };

    static int _moduleIdCounter = 1;
    int _moduleID = 0;

    private KMSelectable.OnInteractHandler Press(int pos)
    {
        return delegate
        {
            Buttons[pos].AddInteractionPunch();
            Audio.PlaySoundAtTransform((input.Count() + 3).ToString(), Buttons[pos].transform);
            if (input.Contains(pos) || input.Count() == 4)
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
        List<int> colour = new List<int> { };
        int[] order = new int[7];
        int[] finalpos = new int[4];
        poscolour = Enumerable.Range(0, 7).ToList().Shuffle().Take(6).ToArray();
        pulsing = Enumerable.Range(0, 6).ToList().Shuffle().Take(3).ToArray();
        colour = pulsing.Select(x => poscolour[x]).ToList();
        bool valid = true;
        bool steady = true;
        int invalidpos = 0;

        Debug.LogFormat("[Tetriamonds #{0}] Colours starting from top-left are: {1}.", _moduleID, poscolour.Select(x => colourname[x]).Join(", "));
        Debug.LogFormat("[Tetriamonds #{0}] Pulsing colours are: {1}.", _moduleID, pulsing.Select(x => colourname[poscolour[x]]).Join(", "));

        //setting up order
        for (int i = 0; i < 3; i++)
            order[i] = colour[i];
        for (int i = 0; i < 3; i++)
            order[i + 3] = Enumerable.Range(0, 6).Select(x => poscolour[(pulsing[i] + x) % 6]).First(x => !order.Take(i + 3).Contains(x));
        order[6] = Enumerable.Range(0, 7).First(x => !poscolour.Contains(x));
        Debug.LogFormat("[Tetriamonds #{0}] Colour order is: {1}.", _moduleID, order.Select(x => colourname[x]).Join(", "));

        //finding colours
        if (!colour.Contains(6) || colour.Any(x => colour.Contains((x + 1) % 6) && x != 6))
            colour = order.Where(x => !colour.Contains(x)).ToList();
        else if (colour.Any(x => colour.Contains((x + 3) % 6) && x != 6))
            colour.Add(order[3]);
        else
            colour.Add((colour.First(x => x != 6) + 3) % 6);
        invalidpos = (colour[3] == 6 ? 2 : 3);
        Debug.LogFormat("[Tetriamonds #{0}] Used colours are: {1}.", _moduleID, colour.Select(x => colourname[x]).Join(", "));

        //determining shape
        finalpos = colour.ToArray();
        
        if (pulsing.Select(x => x % 2).Distinct().Count() == 1)
        {
            //triangle
            Debug.LogFormat("[Tetriamonds #{0}] Constructing a triangle.", _moduleID);
            while (steady)
                if (finalpos.Count(x => finalpos.Contains((x + 2) % 6) && x != 6) == 3)
                {
                    finalpos = finalpos.Select(x => finalpos.Contains((x + 2) % 6) ? x : 6).ToArray();
                    steady = false;
                }
                else if (finalpos.Count(x => finalpos.Contains((x + 2) % 6) && x != 6) == 1 && finalpos.Contains(6))
                {
                    finalpos = finalpos.Select(x => (finalpos.Contains((x + 2) % 6) || finalpos.Contains((x + 4) % 6)) && x != 6 ? x : (x == 6 ? Enumerable.Range(0, 6).First(y => finalpos.Contains((y + 2) % 6) && finalpos.Contains((y + 4) % 6)) : 6)).ToArray();
                    steady = false;
                }
                else if (valid)
                {
                    finalpos[invalidpos] = order.First(x => !finalpos.Concat(new int[] { 6 }).Contains(x));
                    colour[invalidpos] = finalpos[invalidpos];
                    valid = false;
                }
                else
                {
                    finalpos[invalidpos] = order.Skip(Array.IndexOf(order, finalpos[invalidpos])).First(x => !finalpos.Concat(new int[] { 6 }).Contains(x));
                    colour[invalidpos] = finalpos[invalidpos];
                }

            finalpos = Enumerable.Range(0, 4).OrderBy(x => Array.IndexOf(order, colour[x])).Select(x => finalpos[x]).ToArray();
            //to hex
            int pivot = finalpos.Last(x => x != 6);
            finalpos = finalpos.Select(x => x == 6 ? pivot : ((x + 2) % 6 == pivot ? (x + 1) % 6 : ((x + 4) % 6 == pivot ? (x + 5) % 6 : (x + 3) % 6))).ToArray();
        }
        else
        {
            //rhomboid or U-shape
            while (steady)
                if (finalpos.Count(x => finalpos.Contains((x + 1) % 6) && x != 6) == 3)
                    steady = false;
                else if (finalpos.Count(x => finalpos.Contains((x + 1) % 6) && x != 6) == 1 && finalpos.Contains(6))
                {
                    finalpos = finalpos.Select(x => x != 6 ? x : Enumerable.Range(0, 6).First(y => finalpos.Contains((y + 1) % 6) && finalpos.Contains((y + 5) % 6))).ToArray();
                    steady = false;
                }
                else if (valid)
                {
                    finalpos[invalidpos] = order.First(x => !finalpos.Concat(new int[] { 6 }).Contains(x));
                    colour[invalidpos] = finalpos[invalidpos];
                    valid = false;
                }
                else
                {
                    finalpos[invalidpos] = order.Skip(Array.IndexOf(order, finalpos[invalidpos])).First(x => !finalpos.Concat(new int[] { 6 }).Contains(x));
                    colour[invalidpos] = finalpos[invalidpos];
                }

            finalpos = Enumerable.Range(0, 4).OrderBy(x => Array.IndexOf(order, colour[x])).Select(x => finalpos[x]).ToArray();
            //to hex (if rhomboid)
            if (pulsing[0] % 2 == pulsing[1] % 2)
            {
                Debug.LogFormat("[Tetriamonds #{0}] Constructing a rhomboid.", _moduleID);
                int pivot = finalpos.First(x => !finalpos.Contains((x + 5) % 6));
                finalpos = finalpos.Select(x => ((7 + 3 * (pivot % 2)) - pivot + x) % 6).ToArray();
                finalpos = finalpos.Select(x => new int[] { 0, 1, 2, 5, 4, 3 }[x]).ToArray();
                finalpos = finalpos.Select(x => (x + pulsing[0] + (((pulsing[0] + 2) % 6 == pulsing[1]) ? 0 : 1)) % 6).ToArray();
            }
            else
                Debug.LogFormat("[Tetriamonds #{0}] Constructing a U-shape.", _moduleID);
        }
        if (!valid)
            Debug.LogFormat("[Tetriamonds #{0}] Invalid set of colours. Replacing colour {1} with {2}.", _moduleID, invalidpos + 1, colourname[colour[invalidpos]]);

        answerpos = finalpos;

        Debug.LogFormat("[Tetriamonds #{0}] Expected answer: {1}.", _moduleID, answerpos.Select(x => direction[x]).Join(", "));
    }

    private void CheckSolve()
    {
        bool good = true;
        for (int i = 0; i < 4; i++)
            good &= input[i] == answerpos[i];
        if (good)
        {
            Debug.LogFormat("[Tetriamonds #{0}] You submitted: {1}. That is correct. Module solved!", _moduleID, input.Select(x => direction[x]).Join(", "));
            Module.HandlePass();
            solved = true;
            Audio.PlaySoundAtTransform("7", Module.transform);
            Text.text = "";
            for (int i = 0; i < 6; i++)
                Buttons[i].GetComponent<MeshRenderer>().material.color = new Color(.5f, .5f, .5f);
        }
        else
        {
            Debug.LogFormat("[Tetriamonds #{0}] You submitted: {1}. That is incorrect. I expected: {2}. Strike!", _moduleID, input.Select(x => direction[x]).Join(", "), answerpos.Select(x => direction[x]).Join(", "));
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
                while (input.Count() != 0 && (input.Count() != 4 || Mathf.Abs(Buttons[input[3]].transform.localPosition.x) < Mathf.Abs(initpos[input[3]].x * constant)))
                {
                    for (int i = 0; i < 6; i++)
                        if (input.Contains(i))
                            if (Mathf.Abs(Buttons[i].transform.localPosition.x) < Mathf.Abs(initpos[i].x * constant))
                                Buttons[i].transform.localPosition = Vector3.Lerp(initpos[i], new Vector3(initpos[i].x * constant, initpos[i].y * constant, initpos[i].z * constant), (Buttons[i].transform.localPosition.x - initpos[i].x) / (initpos[i].x * (constant - 1f)) + Time.deltaTime);
                            else
                                Buttons[i].transform.localPosition = new Vector3(initpos[i].x * constant, initpos[i].y * constant, initpos[i].z * constant);
                        else if (Mathf.Abs(Buttons[i].transform.localPosition.x) > Mathf.Abs(initpos[i].x))
                                Buttons[i].transform.localPosition = Vector3.Lerp(new Vector3(initpos[i].x * constant, initpos[i].y * constant, initpos[i].z * constant), initpos[i], (initpos[i].x * constant - Buttons[i].transform.localPosition.x) / (initpos[i].x * (constant - 1f)) + Time.deltaTime);
                        else
                            Buttons[i].transform.localPosition = initpos[i];
                    yield return null;
                }
                while (Enumerable.Range(0, 6).ToList().Count(x => Mathf.Abs(Buttons[x].transform.localPosition.x) > Mathf.Abs(initpos[x].x)) != 0 && (input.Count() == 0 || input.Count() == 4))
                {
                    for (int i = 0; i < 6; i++)
                        if (Mathf.Abs(Buttons[i].transform.localPosition.x) > Mathf.Abs(initpos[i].x))
                            Buttons[i].transform.localPosition = Vector3.Lerp(initpos[i], new Vector3(initpos[i].x * constant, initpos[i].y * constant, initpos[i].z * constant), (Buttons[i].transform.localPosition.x - initpos[i].x) / (initpos[i].x * (constant - 1f)) - Time.deltaTime);
                        else
                            Buttons[i].transform.localPosition = initpos[i];
                    yield return null;
                }
                for (int i = 0; i < 6 && (input.Count() == 0 || input.Count() == 4); i++)
                    Buttons[i].transform.localPosition = initpos[i];
                if (input.Count() == 0)
                    yield return null;
                else if (input.Count() == 4)
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
    private string TwitchHelpMessage = "'!{0} inspect tl bl tr mr' to hover over those triangles, '!{0} press tl bl tr mr' to press those triangles.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;
        command = command.ToLowerInvariant();
        if (Regex.IsMatch(command, @"^(inspect|press)(\s[tmb][lr])+$"))
        {
            string[] set = { "tl", "tr", "mr", "br", "bl", "ml" };
            MatchCollection matches = Regex.Matches(command, @"[tmb][lr]");
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
        for (int i = 0; i < 4; i++)
        {
            Buttons[answerpos[i]].OnInteract();
            yield return null;
        }
    }
}
