using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class simonTriamondScript : MonoBehaviour
{
	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMBombInfo Bomb;
	public KMSelectable Button1;
	public KMSelectable Button2;
	public KMSelectable Button3;
	public KMSelectable Button4;
	public KMSelectable Button5;
	public KMSelectable Button6;
	public KMBombModule Module;
	public KMColorblindMode CBM;
	private int[] InitialColours;
	private int[] NewColours;
	private int[] OldColours = { 0, 0, 0, 0, 0, 0 };
	private int[] InitialPos;
	private int[] NewPos;
	private string[] SFX = { "SimonC4", "SimonD", "SimonD#", "SimonF", "SimonG", "SimonA", "SimonB", "SimonC5" };
	private string[] Colours = { "black", "blue", "green", "cyan", "red", "magenta", "yellow", "white" };
	private string[] ColourSingle = { "k", "b", "g", "c", "r", "m", "y", "w" };
	private string[] Orientations = { "down", "up" };
	private string[] Positions = { "error", "top left", "top middle", "top right", "bottom left", "bottom middle", "bottom right" };
	private int ColIndexVar;
	private int t;
	private int t2;
	private int targetT = 1;
	private int a;
	private int b;
	private int c;
	private int k;
	private int n = 0;
	private int[] L;
	private int orient;
	private int placecount = 0;
	private int rule = 0;
	private bool annoy = false;
	private int status = 0;
	private int centernum;
	private int[] submit = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
	private bool[] available = { true, true, true, true, true, true };
	private bool colorblind;
	private string[] TPCmds;
	static int _moduleIdCounter = 1;
	int _moduleID = 0;

	void Awake()
	{
		_moduleID = _moduleIdCounter++;
		colorblind = CBM.ColorblindModeActive;
		Button1.OnInteract += delegate () { if ((status == 0 && t2 == 4 || status == 1 && t2 == targetT) && available[0]) { Audio.PlaySoundAtTransform(SFX[OldColours[0]], Button1.transform); Button1.AddInteractionPunch(); n = 1 + n * 10; targetT++; available[0] = false; } return false; };
		Button2.OnInteract += delegate () { if ((status == 0 && t2 == 4 || status == 1 && t2 == targetT) && available[1]) { Audio.PlaySoundAtTransform(SFX[OldColours[1]], Button2.transform); Button2.AddInteractionPunch(); n = 2 + n * 10; targetT++; available[1] = false; } return false; };
		Button3.OnInteract += delegate () { if ((status == 0 && t2 == 4 || status == 1 && t2 == targetT) && available[2]) { Audio.PlaySoundAtTransform(SFX[OldColours[2]], Button3.transform); Button3.AddInteractionPunch(); n = 3 + n * 10; targetT++; available[2] = false; } return false; };
		Button4.OnInteract += delegate () { if ((status == 0 && t2 == 4 || status == 1 && t2 == targetT) && available[3]) { Audio.PlaySoundAtTransform(SFX[OldColours[3]], Button4.transform); Button4.AddInteractionPunch(); n = 4 + n * 10; targetT++; available[3] = false; } return false; };
		Button5.OnInteract += delegate () { if ((status == 0 && t2 == 4 || status == 1 && t2 == targetT) && available[4]) { Audio.PlaySoundAtTransform(SFX[OldColours[4]], Button5.transform); Button5.AddInteractionPunch(); n = 5 + n * 10; targetT++; available[4] = false; } return false; };
		Button6.OnInteract += delegate () { if ((status == 0 && t2 == 4 || status == 1 && t2 == targetT) && available[5]) { Audio.PlaySoundAtTransform(SFX[OldColours[5]], Button6.transform); Button6.AddInteractionPunch(); n = 6 + n * 10; targetT++; available[5] = false; } return false; };
		t = 0;
		t2 = 0;
		//Colour Array
		int[] InitialColours = { 0, 1, 2, 3, 4, 5, 6, 7 };
		int[] NewColours = { 0, 0, 0, 0, 0, 0 };
		int IndexVar = 0;
		for (int i = 0; i < 6; i++)
		{
			IndexVar = Rnd.Range(0, 8 - i);
			NewColours[i] = InitialColours[IndexVar];
			OldColours[i] = InitialColours[IndexVar];
			InitialColours = InitialColours.Where((j, index) => index != IndexVar).ToArray();
		}
		Button1.GetComponent<MeshRenderer>().material.color = new Color(NewColours[0] / 4, (NewColours[0] / 2) % 2, NewColours[0] % 2);
		Button2.GetComponent<MeshRenderer>().material.color = new Color(NewColours[1] / 4, (NewColours[1] / 2) % 2, NewColours[1] % 2);
		Button3.GetComponent<MeshRenderer>().material.color = new Color(NewColours[2] / 4, (NewColours[2] / 2) % 2, NewColours[2] % 2);
		Button4.GetComponent<MeshRenderer>().material.color = new Color(NewColours[3] / 4, (NewColours[3] / 2) % 2, NewColours[3] % 2);
		Button5.GetComponent<MeshRenderer>().material.color = new Color(NewColours[4] / 4, (NewColours[4] / 2) % 2, NewColours[4] % 2);
		Button6.GetComponent<MeshRenderer>().material.color = new Color(NewColours[5] / 4, (NewColours[5] / 2) % 2, NewColours[5] % 2);
		//Pulse Array
		int[] InitialPos = { 0, 1, 2, 3, 4, 5 };
		int[] NewPos = { 0, 0, 0 };
		for (int i = 0; i < 3; i++)
		{
			IndexVar = Rnd.Range(0, 6 - i);
			NewPos[i] = InitialPos[IndexVar];
			InitialPos = InitialPos.Where((j, index) => index != IndexVar).ToArray();
		}
		a = NewPos[0];
		b = NewPos[1];
		c = NewPos[2];
		int[] L = { a, b, c };
		//Translating Input Triangles
		orient = 7 - ((a % 2) * 4 + (1 - (a % 2)) * 2 + c % 2);
		if (a % 2 == b % 2)
		{
			NewColours[b] = 7 - NewColours[b];
			Debug.LogFormat("[Triamonds #{0}] Flipped second due to matching orientation.", _moduleID);
			if (NewColours[a] == NewColours[b])
			{
				NewColours[a] = 7 - NewColours[a];
				Debug.LogFormat("[Triamonds #{0}] Inverted first colour due to matching colours.", _moduleID);
			}
			if (NewColours[b] == NewColours[c])
			{
				NewColours[c] = 7 - NewColours[c];
				Debug.LogFormat("[Triamonds #{0}] Inverted third colour due to matching colours.", _moduleID);
			}
		}
		Debug.LogFormat("[Triamonds #{0}] Colours are: {1}, {2}, {3}.", _moduleID, Colours[NewColours[a]], Colours[NewColours[b]], Colours[NewColours[c]]);
		Debug.LogFormat("[Triamonds #{0}] Orientations are: {1}, {2}, {3}.", _moduleID, Orientations[(1 - (a % 2))], Orientations[(a % 2)], Orientations[(1 - (c % 2))]);

		if (a % 2 == c % 2) { centernum = 1; } else { centernum = 0; }
		//Rule1
		k = 4;
		for (int i = 0; i < 3; i++)
		{
			if ((NewColours[L[i]] != 0 && NewColours[L[i]] != 7) && (((orient / k) % 2) == 0) ^ (NewColours[L[i]] == 1 || NewColours[L[i]] == 2 || NewColours[L[i]] == 4))
			{
				if (NewColours[L[i]] == 4) { submit[2] = i+1; placecount++; }
				else if (NewColours[L[i]] == 6) { submit[3] = i+1; placecount++; }
				else if (NewColours[L[i]] == 2) { submit[4] = i+1; placecount++; }
				else if (NewColours[L[i]] == 5) { submit[7] = i+1; placecount++; }
				else if (NewColours[L[i]] == 1) { submit[8] = i+1; placecount++; }
				else if (NewColours[L[i]] == 3) { submit[9] = i+1; placecount++; }
			}
			k = k / 2;
		}
		Debug.LogFormat("[Triamonds #{0}] Applied rule 1.", _moduleID);
		//Rule2 + Rule3
		k = 4;
		for (int i = 0; i < 3; i++)
		{
			if ((NewColours[L[i]] != 0 && NewColours[L[i]] != 7) && (((orient / k) % 2) == 1) ^ (NewColours[L[i]] == 1 || NewColours[L[i]] == 2 || NewColours[L[i]] == 4))
			{
				if (NewColours[L[i]] == 3 && submit[2] == 0) { submit[2] = i+1; placecount++; } else if (NewColours[L[i]] == 3) { rule = i + 1; }
				else if (NewColours[L[i]] == 1 && submit[3] == 0) { submit[3] = i+1; placecount++; } else if (NewColours[L[i]] == 1) { rule = i + 1; }
				else if (NewColours[L[i]] == 5 && submit[4] == 0) { submit[4] = i+1; placecount++; } else if (NewColours[L[i]] == 5) { rule = i + 1; }
				else if (NewColours[L[i]] == 2 && submit[7] == 0) { submit[7] = i+1; placecount++; } else if (NewColours[L[i]] == 2) { rule = i + 1; }
				else if (NewColours[L[i]] == 6 && submit[8] == 0) { submit[8] = i+1; placecount++; } else if (NewColours[L[i]] == 6) { rule = i + 1; }
				else if (NewColours[L[i]] == 4 && submit[9] == 0) { submit[9] = i+1; placecount++; } else if (NewColours[L[i]] == 4) { rule = i + 1; }
			}
			k = k / 2;
		}
		if (rule != 0) { goto Rule3; }
		//Rule4
		if ((NewColours[L[0]] == 7 - NewColours[L[1]] || NewColours[L[0]] == 7 - NewColours[L[2]] || NewColours[L[1]] == 7 - NewColours[L[2]]) && placecount > 1)
		{
			if (placecount == 2)
			{
				if (submit[2] == centernum + 1) { submit[1] = submit[9]; submit[9] = 0; }
				else if (submit[3] == centernum + 1) { submit[0] = submit[8]; submit[8] = 0; }
				else if (submit[4] == centernum + 1) { submit[5] = submit[7]; submit[7] = 0; }
				else if (submit[7] == centernum + 1) { submit[6] = submit[4]; submit[4] = 0; }
				else if (submit[8] == centernum + 1) { submit[11] = submit[3]; submit[3] = 0; }
				else if (submit[9] == centernum + 1) { submit[10] = submit[2]; submit[2] = 0; }
				for(int i = 0; i < 3; i++)
				{
					if (NewColours[L[i]] == 0)
					{
						if (submit[2] != 0) { submit[7] = i + 1; }
						else if (submit[3] != 0) { submit[2] = i + 1; }
						else if (submit[4] != 0) { submit[3] = i + 1; }
						else if (submit[9] != 0) { submit[4] = i + 1; }
						else if (submit[8] != 0) { submit[9] = i + 1; }
						else if (submit[7] != 0) { submit[8] = i + 1; }
						Debug.LogFormat("[Triamonds #{0}] Applied rule 3.", _moduleID);
						Debug.LogFormat("[Triamonds #{0}] Applied subrule 1.", _moduleID);
					}
					if (NewColours[L[i]] == 7)
					{
						if (submit[2] != 0) { submit[3] = i + 1; }
						else if (submit[3] != 0) { submit[4] = i + 1; }
						else if (submit[4] != 0) { submit[9] = i + 1; }
						else if (submit[9] != 0) { submit[8] = i + 1; }
						else if (submit[8] != 0) { submit[7] = i + 1; }
						else if (submit[7] != 0) { submit[2] = i + 1; }
						Debug.LogFormat("[Triamonds #{0}] Applied rule 3.", _moduleID);
						Debug.LogFormat("[Triamonds #{0}] Applied subrule 2.", _moduleID);
					}
				}
				goto Finish;
			}
			if (placecount == 3)
			{
				if (submit[2] != 0 && submit[9] != 0) { if (submit[3] == 0 && submit[7] == 0) { submit[10] = submit[2]; submit[2] = 0; } else { submit[1] = submit[9]; submit[9] = 0; } }
				else if (submit[3] != 0 && submit[8] != 0) { if (submit[2] == 0 && submit[4] == 0) { submit[11] = submit[3]; submit[3] = 0; } else { submit[0] = submit[8]; submit[8] = 0; } }
				else if (submit[4] != 0 && submit[7] != 0) { if (submit[2] == 0 && submit[8] == 0) { submit[5] = submit[7]; submit[7] = 0; } else { submit[6] = submit[4]; submit[4] = 0; } }
				Debug.LogFormat("[Triamonds #{0}] Applied rule 3.", _moduleID);
			}

		}
		if (placecount == 3) { goto Finish; }
		//Rule5
		if ((NewColours[L[0]] == 0 || NewColours[L[1]] == 0 || NewColours[L[2]] == 0) && NewColours[L[centernum]] != 0 && placecount == 2)
		{
			k = 4;
			for (int i = 0; i < 3; i++)
			{
				if (NewColours[L[i]] == 0)
				{
					if (((orient / k) % 2) == 0)
					{
						if (submit[2] != 0) { submit[1] = i + 1; }
						else if (submit[4] != 0) { submit[5] = i + 1; }
						else if (submit[8] != 0) { submit[11] = i + 1; }
					}
					else
					{
						if (submit[3] != 0) { submit[0] = i + 1; }
						else if (submit[7] != 0) { submit[6] = i + 1; }
						else if (submit[9] != 0) { submit[10] = i + 1; }
					}
				}
				k = k / 2;
			}
			Debug.LogFormat("[Triamonds #{0}] Applied rule 4.", _moduleID);
			Debug.LogFormat("[Triamonds #{0}] Applied subrule 1.", _moduleID);
			goto Finish;
		}
		//Rule7
		if (NewColours[L[centernum]] == 0 && placecount == 2)
		{
			int j = 0;
			k = 0;
			for (int i = 0; i < 12; i++)
			{
				if (submit[i] != 0)
				{
					if (k == 0) { k = submit[i]; j = i; } else { submit[j] = submit[i]; submit[i] = k; }
				}
			}
			if (submit[2] != 0 && submit[4] != 0) { submit[3] = centernum + 1; }
			else if (submit[3] != 0 && submit[9] != 0) { submit[4] = centernum + 1; }
			else if (submit[4] != 0 && submit[8] != 0) { submit[9] = centernum + 1; }
			else if (submit[9] != 0 && submit[7] != 0) { submit[8] = centernum + 1; }
			else if (submit[8] != 0 && submit[2] != 0) { submit[7] = centernum + 1; }
			else if (submit[7] != 0 && submit[3] != 0) { submit[2] = centernum + 1; }
			placecount++;
			Debug.LogFormat("[Triamonds #{0}] Applied rule 4.", _moduleID);
			Debug.LogFormat("[Triamonds #{0}] Applied subrule 2.", _moduleID);
			goto Finish;
		}
		//Rule8
		if ((NewColours[L[0]] == 7 || NewColours[L[1]] == 7 || NewColours[L[2]] == 7) && placecount == 2)
		{
			k = 4;
			for (int i = 0; i < 3; i++)
			{
				if (NewColours[L[i]] == 7)
				{
					if (((orient / k) % 2) == 0)
					{
						if ((submit[2] != 0 && submit[4] != 0) || (submit[2] != 0 && submit[7] != 0) || (submit[4] != 0 && submit[9] != 0)) { submit[3] = i + 1; }
						else if ((submit[2] != 0 && submit[8] != 0) || (submit[2] != 0 && submit[3] != 0) || (submit[8] != 0 && submit[9] != 0)) { submit[7] = i + 1; }
						else if ((submit[4] != 0 && submit[8] != 0) || (submit[3] != 0 && submit[4] != 0) || (submit[7] != 0 && submit[8] != 0)) { submit[9] = i + 1; }
					}
					else
					{
						if ((submit[3] != 0 && submit[7] != 0) || (submit[3] != 0 && submit[4] != 0) || (submit[7] != 0 && submit[8] != 0)) { submit[2] = i + 1; }
						else if ((submit[3] != 0 && submit[9] != 0) || (submit[2] != 0 && submit[3] != 0) || (submit[8] != 0 && submit[9] != 0)) { submit[4] = i + 1; }
						else if ((submit[7] != 0 && submit[9] != 0) || (submit[2] != 0 && submit[7] != 0) || (submit[4] != 0 && submit[9] != 0)) { submit[8] = i + 1; }
					}
					placecount++;
				}
				k = k / 2;
			}
			Debug.LogFormat("[Triamonds #{0}] Applied rule 5.", _moduleID);
			goto Finish;
		}
		//Rule10
		if(placecount == 1 && NewColours[L[centernum]] != 0)
		{
			if(NewColours[L[centernum]] == 7)
			{
				for(int i = 0; i < 3; i++)
				{
					if(NewColours[L[i]] == 0)
					{
						if (submit[2] != 0 && submit[3] == 0) { submit[6] = i + 1; }
						else if (submit[3] != 0 && submit[4] == 0) { submit[1] = i + 1; }
						else if (submit[4] != 0 && submit[9] == 0) { submit[0] = i + 1; }
						else if (submit[9] != 0 && submit[8] == 0) { submit[5] = i + 1; }
						else if (submit[8] != 0 && submit[7] == 0) { submit[10] = i + 1; }
						else if (submit[7] != 0 && submit[2] == 0) { submit[11] = i + 1; }
						placecount++;
					}
					if (NewColours[L[i]] == 7)
					{
						if (submit[2] != 0) { submit[7] = i + 1; }
						else if (submit[3] != 0) { submit[2] = i + 1; }
						else if (submit[4] != 0) { submit[3] = i + 1; }
						else if (submit[9] != 0) { submit[4] = i + 1; }
						else if (submit[8] != 0) { submit[9] = i + 1; }
						else if (submit[7] != 0) { submit[8] = i + 1; }
						placecount++;
					}
				}
			}
			else
			{
				k = 4;
				for (int i = 0; i < 3; i++)
				{
					if (NewColours[L[i]] == 0)
					{ 
						if (submit[2] != 0 && submit[7] == 0) { submit[1] = i + 1; }
						else if (submit[3] != 0 && submit[2] == 0) { submit[0] = i + 1; }
						else if (submit[4] != 0 && submit[3] == 0) { submit[5] = i + 1; }
						else if (submit[7] != 0 && submit[8] == 0) { submit[6] = i + 1; }
						else if (submit[8] != 0 && submit[9] == 0) { submit[11] = i + 1; }
						else if (submit[9] != 0 && submit[4] == 0) { submit[10] = i + 1; }
					}
					else if (NewColours[L[i]] == 7)
					{
						if (submit[2] != 0) { submit[3] = i + 1; }
						else if (submit[3] != 0) { submit[4] = i + 1; }
						else if (submit[4] != 0) { submit[9] = i + 1; }
						else if (submit[7] != 0) { submit[2] = i + 1; }
						else if (submit[8] != 0) { submit[7] = i + 1; }
						else if (submit[9] != 0) { submit[8] = i + 1; }
					}
					k = k / 2;
				}
			}
			Debug.LogFormat("[Triamonds #{0}] Applied rule 6.", _moduleID);
			Debug.LogFormat("[Triamonds #{0}] Applied subrule 1.", _moduleID);
			goto Finish;
		}
		//Rule11
		if (placecount == 1 && NewColours[L[centernum]] == 0)
		{
			submit[10] = submit[2]; submit[2] = 0; 
			submit[1] = submit[9]; submit[9] = 0; 
			submit[11] = submit[3]; submit[3] = 0; 
			submit[0] = submit[8]; submit[8] = 0; 
			submit[5] = submit[7]; submit[7] = 0;
			submit[6] = submit[4]; submit[4] = 0;
			for (int i = 0; i < 3; i++)
			{
				if (NewColours[L[i]] == 0)
				{
					if (submit[1] != 0) { submit[2] = i + 1; }
					else if (submit[0] != 0) { submit[3] = i + 1; }
					else if (submit[5] != 0) { submit[4] = i + 1; }
					else if (submit[10] != 0) { submit[9] = i + 1; }
					else if (submit[11] != 0) { submit[8] = i + 1; }
					else if (submit[6] != 0) { submit[7] = i + 1; }
					placecount++;
				}
				else if (NewColours[L[i]] == 7)
				{
					if (submit[1] != 0) { submit[3] = i + 1; }
					else if (submit[0] != 0) { submit[4] = i + 1; }
					else if (submit[5] != 0) { submit[9] = i + 1; }
					else if (submit[10] != 0) { submit[8] = i + 1; }
					else if (submit[11] != 0) { submit[7] = i + 1; }
					else if (submit[6] != 0) { submit[2] = i + 1; }
					placecount++;
				}
			}
			Debug.LogFormat("[Triamonds #{0}] Applied rule 6.", _moduleID);
			Debug.LogFormat("[Triamonds #{0}] Applied subrule 2.", _moduleID);
		}
		goto Finish;
	Rule3:;
		Debug.LogFormat("[Triamonds #{0}] Applied rule 2.", _moduleID);
		if (placecount == 2)
		{
			k = 4;
			for (int i = 0; i < 3; i++)
			{
				if (i == rule - 1)
				{
					if (((orient / k) % 2) == 0)
					{
						if ((submit[2] != 0 && submit[7] != 0) || (submit[4] != 0 && submit[9] != 0)) { submit[3] = i + 1; }
						else if ((submit[2] != 0 && submit[3] != 0) || (submit[8] != 0 && submit[9] != 0)) { submit[7] = i + 1; }
						else if ((submit[3] != 0 && submit[4] != 0) || (submit[7] != 0 && submit[8] != 0)) { submit[9] = i + 1; }
					}
					else
					{
						if ((submit[2] != 0 && submit[7] != 0) || (submit[4] != 0 && submit[9] != 0)) { submit[8] = i + 1; }
						else if ((submit[2] != 0 && submit[3] != 0) || (submit[8] != 0 && submit[9] != 0)) { submit[4] = i + 1; }
						else if ((submit[3] != 0 && submit[4] != 0) || (submit[7] != 0 && submit[8] != 0)) { submit[2] = i + 1; }
					}
				}
				k = k / 2;
			}
			Debug.LogFormat("[Triamonds #{0}] Applied subrule 3.", _moduleID);
			goto Finish;
		}
		if (placecount == 1)
		{
			if (NewColours[L[centernum]] == 0)
			{
				submit[10] = submit[2]; submit[2] = 0;
				submit[1] = submit[9]; submit[9] = 0;
				submit[11] = submit[3]; submit[3] = 0;
				submit[0] = submit[8]; submit[8] = 0;
				submit[5] = submit[7]; submit[7] = 0;
				submit[6] = submit[4]; submit[4] = 0;
				for (int i = 0; i < 3; i++)
				{
					if (NewColours[L[i]] == 0)
					{ 
						if (submit[1] != 0) { submit[2] = i + 1; }
						else if (submit[0] != 0) { submit[3] = i + 1; }
						else if (submit[5] != 0) { submit[4] = i + 1; }
						else if (submit[10] != 0) { submit[9] = i + 1; }
						else if (submit[11] != 0) { submit[8] = i + 1; }
						else if (submit[6] != 0) { submit[7] = i + 1; }
					}
					else if((i + 1) == rule)
					{
						if (submit[1] != 0) { submit[7] = i + 1; }
						else if (submit[0] != 0) { submit[2] = i + 1; }
						else if (submit[5] != 0) { submit[3] = i + 1; }
						else if (submit[10] != 0) { submit[4] = i + 1; }
						else if (submit[11] != 0) { submit[9] = i + 1; }
						else if (submit[6] != 0) { submit[8] = i + 1; }
					}
				}
				Debug.LogFormat("[Triamonds #{0}] Applied subrule 1.", _moduleID);
				goto Finish;
			}
			if (NewColours[L[centernum]] == 7)
			{
				for (int i = 0; i < 3; i++)
				{
					if (NewColours[L[i]] == 7)
					{
						if (submit[2] != 0 && submit[4] == 0) { submit[7] = i + 1; }
						else if (submit[3] != 0 && submit[9] == 0) { submit[2] = i + 1; }
						else if (submit[4] != 0 && submit[8] == 0) { submit[3] = i + 1; }
						else if (submit[9] != 0 && submit[7] == 0) { submit[4] = i + 1; }
						else if (submit[8] != 0 && submit[2] == 0) { submit[9] = i + 1; }
						else if (submit[7] != 0 && submit[3] == 0) { submit[8] = i + 1; }
					}
					else if ((i + 1) == rule)
					{
						if (submit[2] != 0 && submit[3] == 0) { submit[8] = i + 1; }
						else if (submit[3] != 0 && submit[4] == 0) { submit[7] = i + 1; }
						else if (submit[4] != 0 && submit[9] == 0) { submit[2] = i + 1; }
						else if (submit[9] != 0 && submit[8] == 0) { submit[3] = i + 1; }
						else if (submit[8] != 0 && submit[7] == 0) { submit[4] = i + 1; }
						else if (submit[7] != 0 && submit[2] == 0) { submit[9] = i + 1; }
					}
				}
				Debug.LogFormat("[Triamonds #{0}] Applied subrule 2.", _moduleID);
				goto Finish;
			}
		}
		Debug.LogFormat("[Triamonds #{0}] Incomplete.", _moduleID);
	Finish:;
		if ((submit[1] != 0 && submit[3] != 0) || (submit[0] != 0 && submit[2] != 0)) { submit[4] = submit[0]; submit[7] = submit[1]; submit[8] = submit[2]; submit[9] = submit[3]; submit[1] = 0; submit[0] = 0; submit[2] = 0; submit[3] = 0; }
		if ((submit[0] != 0 && submit[4] != 0) || (submit[5] != 0 && submit[3] != 0)) { submit[2] = submit[0]; submit[7] = submit[3]; submit[8] = submit[4]; submit[9] = submit[5]; submit[0] = 0; submit[5] = 0; submit[3] = 0; submit[4] = 0; }
		if ((submit[5] != 0 && submit[9] != 0) || (submit[10] != 0 && submit[4] != 0)) { submit[2] = submit[4]; submit[3] = submit[5]; submit[7] = submit[9]; submit[8] = submit[10]; submit[5] = 0; submit[10] = 0; submit[4] = 0; submit[9] = 0; }
		if ((submit[10] != 0 && submit[8] != 0) || (submit[11] != 0 && submit[9] != 0)) { submit[2] = submit[8]; submit[3] = submit[9]; submit[4] = submit[10]; submit[7] = submit[11]; submit[10] = 0; submit[11] = 0; submit[9] = 0; submit[8] = 0; }
		if ((submit[11] != 0 && submit[7] != 0) || (submit[6] != 0 && submit[8] != 0)) { submit[2] = submit[6]; submit[3] = submit[7]; submit[4] = submit[8]; submit[9] = submit[11]; submit[11] = 0; submit[6] = 0; submit[8] = 0; submit[7] = 0; }
		if ((submit[6] != 0 && submit[2] != 0) || (submit[1] != 0 && submit[7] != 0)) { submit[3] = submit[1]; submit[4] = submit[2]; submit[8] = submit[6]; submit[9] = submit[7]; submit[6] = 0; submit[1] = 0; submit[7] = 0; submit[2] = 0; }
		//Encrypt to ordering
		k = 0;
		for(int i = 1; i < 4; i++)
		{
			k = k * 10;
			if (submit[2] == i) { k += 1; }
			else if (submit[3] == i) { k += 2; }
			else if (submit[4] == i) { k += 3; }
			else if (submit[7] == i) { k += 4; }
			else if (submit[8] == i) { k += 5; }
			else if (submit[9] == i) { k += 6; }
			else { Debug.LogFormat("[Triamonds #{0}] Failed to retrieve solution. Please contact the developer.", _moduleID); ; status = -1; colorblind = false; }
		}
		Debug.LogFormat("[Triamonds #{0}] Expected presses: {1}, {2}, {3}.", _moduleID, Positions[(k / 100)], Positions[((k / 10) % 10)], Positions[(k % 10)]);
		CbDisplay();
	}

	void FixedUpdate()
	{
		if (status == -1) { Button1.GetComponent<MeshRenderer>().material.color = new Color(.25f, .25f, .25f); Button2.GetComponent<MeshRenderer>().material.color = new Color(.25f, .25f, .25f); Button3.GetComponent<MeshRenderer>().material.color = new Color(.25f, .25f, .25f); Button4.GetComponent<MeshRenderer>().material.color = new Color(.25f, .25f, .25f); Button5.GetComponent<MeshRenderer>().material.color = new Color(.25f, .25f, .25f); Button6.GetComponent<MeshRenderer>().material.color = new Color(.25f, .25f, .25f); Button1.transform.localPosition = new Vector3(-0.03f, 0f, 0.03f); Button1.transform.localPosition = new Vector3(-0.03f, 0f, 0.03f); Button2.transform.localPosition = new Vector3(0f, 0f, 0.03f); Button3.transform.localPosition = new Vector3(0.03f, 0f, 0.03f); Button4.transform.localPosition = new Vector3(-0.03f, 0f, -0.03f); Button5.transform.localPosition = new Vector3(0f, 0f, -0.03f); Button6.transform.localPosition = new Vector3(0.03f, 0f, -0.03f); Module.HandlePass(); }
		if (status == 0)
		{
			if (t2 == 0)
			{
				if (annoy && t%50 == 0) {
					Audio.PlaySoundAtTransform(SFX[OldColours[a]], Button1.transform);
				}
				if (a == 0)
				{
					Button1.transform.localPosition = new Vector3(Button1.transform.localPosition.x - 0.00025f, Button1.transform.localPosition.y, Button1.transform.localPosition.z + 0.000125f);
				}
				if (a == 1)
				{
					Button2.transform.localPosition = new Vector3(Button2.transform.localPosition.x, Button2.transform.localPosition.y, Button2.transform.localPosition.z + 0.00025f);
				}
				if (a == 2)
				{
					Button3.transform.localPosition = new Vector3(Button3.transform.localPosition.x + 0.00025f, Button3.transform.localPosition.y, Button3.transform.localPosition.z + 0.000125f);
				}
				if (a == 3)
				{
					Button4.transform.localPosition = new Vector3(Button4.transform.localPosition.x - 0.00025f, Button4.transform.localPosition.y, Button4.transform.localPosition.z - 0.000125f);
				}
				if (a == 4)
				{
					Button5.transform.localPosition = new Vector3(Button5.transform.localPosition.x, Button5.transform.localPosition.y, Button5.transform.localPosition.z - 0.00025f);
				}
				if (a == 5)
				{
					Button6.transform.localPosition = new Vector3(Button6.transform.localPosition.x + 0.00025f, Button6.transform.localPosition.y, Button6.transform.localPosition.z - 0.000125f);
				}
			}
			if (t2 == 1)
			{
				if (annoy && t % 50 == 0)
				{
					Audio.PlaySoundAtTransform(SFX[OldColours[b]], Button1.transform);
				}
				if (b == 0)
				{
					Button1.transform.localPosition = new Vector3(Button1.transform.localPosition.x - 0.00025f, Button1.transform.localPosition.y, Button1.transform.localPosition.z + 0.000125f);
				}
				if (b == 1)
				{
					Button2.transform.localPosition = new Vector3(Button2.transform.localPosition.x, Button2.transform.localPosition.y, Button2.transform.localPosition.z + 0.00025f);
				}
				if (b == 2)
				{
					Button3.transform.localPosition = new Vector3(Button3.transform.localPosition.x + 0.00025f, Button3.transform.localPosition.y, Button3.transform.localPosition.z + 0.000125f);
				}
				if (b == 3)
				{
					Button4.transform.localPosition = new Vector3(Button4.transform.localPosition.x - 0.00025f, Button4.transform.localPosition.y, Button4.transform.localPosition.z - 0.000125f);
				}
				if (b == 4)
				{
					Button5.transform.localPosition = new Vector3(Button5.transform.localPosition.x, Button5.transform.localPosition.y, Button5.transform.localPosition.z - 0.00025f);
				}
				if (b == 5)
				{
					Button6.transform.localPosition = new Vector3(Button6.transform.localPosition.x + 0.00025f, Button6.transform.localPosition.y, Button6.transform.localPosition.z - 0.000125f);
				}
			}
			if (t2 == 2)
			{
				if (annoy && t % 50 == 0)
				{
					Audio.PlaySoundAtTransform(SFX[OldColours[c]], Button1.transform);
				}
				if (c == 0)
				{
					Button1.transform.localPosition = new Vector3(Button1.transform.localPosition.x - 0.00025f, Button1.transform.localPosition.y, Button1.transform.localPosition.z + 0.000125f);
				}
				if (c == 1)
				{
					Button2.transform.localPosition = new Vector3(Button2.transform.localPosition.x, Button2.transform.localPosition.y, Button2.transform.localPosition.z + 0.00025f);
				}
				if (c == 2)
				{
					Button3.transform.localPosition = new Vector3(Button3.transform.localPosition.x + 0.00025f, Button3.transform.localPosition.y, Button3.transform.localPosition.z + 0.000125f);
				}
				if (c == 3)
				{
					Button4.transform.localPosition = new Vector3(Button4.transform.localPosition.x - 0.00025f, Button4.transform.localPosition.y, Button4.transform.localPosition.z - 0.000125f);
				}
				if (c == 4)
				{
					Button5.transform.localPosition = new Vector3(Button5.transform.localPosition.x, Button5.transform.localPosition.y, Button5.transform.localPosition.z - 0.00025f);
				}
				if (c == 5)
				{
					Button6.transform.localPosition = new Vector3(Button6.transform.localPosition.x + 0.00025f, Button6.transform.localPosition.y, Button6.transform.localPosition.z - 0.000125f);
				}
			}
			if (t2 == 3)
			{
				if (annoy && t % 50 == 0)
				{
					Audio.PlaySoundAtTransform(SFX[OldColours[a] ^ OldColours[b] ^ OldColours[c]], Button1.transform);
				}
				if (a == 0 || b == 0 || c == 0)
				{
					Button1.transform.localPosition = new Vector3(Button1.transform.localPosition.x + 0.00025f, Button1.transform.localPosition.y, Button1.transform.localPosition.z - 0.000125f);
				}
				if (a == 1 || b == 1 || c == 1)
				{
					Button2.transform.localPosition = new Vector3(Button2.transform.localPosition.x, Button2.transform.localPosition.y, Button2.transform.localPosition.z - 0.00025f);
				}
				if (a == 2 || b == 2 || c == 2)
				{
					Button3.transform.localPosition = new Vector3(Button3.transform.localPosition.x - 0.00025f, Button3.transform.localPosition.y, Button3.transform.localPosition.z - 0.000125f);
				}
				if (a == 3 || b == 3 || c == 3)
				{
					Button4.transform.localPosition = new Vector3(Button4.transform.localPosition.x + 0.00025f, Button4.transform.localPosition.y, Button4.transform.localPosition.z + 0.000125f);
				}
				if (a == 4 || b == 4 || c == 4)
				{
					Button5.transform.localPosition = new Vector3(Button5.transform.localPosition.x, Button5.transform.localPosition.y, Button5.transform.localPosition.z + 0.00025f);
				}
				if (a == 5 || b == 5 || c == 5)
				{
					Button6.transform.localPosition = new Vector3(Button6.transform.localPosition.x - 0.00025f, Button6.transform.localPosition.y, Button6.transform.localPosition.z + 0.000125f);
				}
			}
			if (t2 == 4)
			{
				Button1.transform.localPosition = new Vector3(-0.03f, 0f, 0.03f); Button1.transform.localPosition = new Vector3(-0.03f, 0f, 0.03f); Button2.transform.localPosition = new Vector3(0f, 0f, 0.03f); Button3.transform.localPosition = new Vector3(0.03f, 0f, 0.03f); Button4.transform.localPosition = new Vector3(-0.03f, 0f, -0.03f); Button5.transform.localPosition = new Vector3(0f, 0f, -0.03f); Button6.transform.localPosition = new Vector3(0.03f, 0f, -0.03f);
				if (n > 0) { status++; t2 = 1; t = 49; }
			}
			t++;
			t2 = (t / 50) % 5;
		}
		if (status == 1)
		{
			if (targetT > t2)
			{
				t++;
				t2 = (t / 50);
				if (n % 10 == 1)
				{
					Button1.transform.localPosition = new Vector3(Button1.transform.localPosition.x - 0.00025f, Button1.transform.localPosition.y, Button1.transform.localPosition.z + 0.000125f);
				}
				if (n % 10 == 2)
				{
					Button2.transform.localPosition = new Vector3(Button2.transform.localPosition.x, Button2.transform.localPosition.y, Button2.transform.localPosition.z + 0.00025f);
				}
				if (n % 10 == 3)
				{
					Button3.transform.localPosition = new Vector3(Button3.transform.localPosition.x + 0.00025f, Button3.transform.localPosition.y, Button3.transform.localPosition.z + 0.000125f);
				}
				if (n % 10 == 4)
				{
					Button4.transform.localPosition = new Vector3(Button4.transform.localPosition.x - 0.00025f, Button4.transform.localPosition.y, Button4.transform.localPosition.z - 0.000125f);
				}
				if (n % 10 == 5)
				{
					Button5.transform.localPosition = new Vector3(Button5.transform.localPosition.x, Button5.transform.localPosition.y, Button5.transform.localPosition.z - 0.00025f);
				}
				if (n % 10 == 6)
				{
					Button6.transform.localPosition = new Vector3(Button6.transform.localPosition.x + 0.00025f, Button6.transform.localPosition.y, Button6.transform.localPosition.z - 0.000125f);
				}
			}
			else
			{
				if (n > 100) { status = 2; }
				else
				{

				}
			}
		}
		if (status == 2)
		{
			if(t2 > 2)
			{
				if (n == k)
				{
					t2 = 0;
					t = 1;
					status = 3;
				}
				else
				{
					Debug.LogFormat("[Triamonds #{0}] Pressed: {1}, {2}, {3}. Expected: {4}, {5}, {6}.", _moduleID, Positions[(n / 100)], Positions[((n / 10) % 10)], Positions[(n % 10)], Positions[(k / 100)], Positions[((k / 10) % 10)], Positions[(k % 10)]);
					Module.HandleStrike();
					for(int i = 0; i < 6; i++) { available[i] = true; }
					t2 = 0;
					t = 1;
					status = 3;
					annoy = true;
				}
				Audio.PlaySoundAtTransform(SFX[(OldColours[(n / 100) - 1] ^ OldColours[(n / 10) % 10 - 1]) ^ OldColours[n % 10 - 1]], Button1.transform);
			}
		}
		if (status == 3)
		{
			if (t2 == 0)
			{
				if (n % 10 == 1)
				{
					Button1.transform.localPosition = new Vector3(Button1.transform.localPosition.x + 0.00025f, Button1.transform.localPosition.y, Button1.transform.localPosition.z - 0.000125f);
				}
				if (n % 10 == 2)
				{
					Button2.transform.localPosition = new Vector3(Button2.transform.localPosition.x, Button2.transform.localPosition.y, Button2.transform.localPosition.z - 0.00025f);
				}
				if (n % 10 == 3)
				{
					Button3.transform.localPosition = new Vector3(Button3.transform.localPosition.x - 0.00025f, Button3.transform.localPosition.y, Button3.transform.localPosition.z - 0.000125f);
				}
				if (n % 10 == 4)
				{
					Button4.transform.localPosition = new Vector3(Button4.transform.localPosition.x + 0.00025f, Button4.transform.localPosition.y, Button4.transform.localPosition.z + 0.000125f);
				}
				if (n % 10 == 5)
				{
					Button5.transform.localPosition = new Vector3(Button5.transform.localPosition.x, Button5.transform.localPosition.y, Button5.transform.localPosition.z + 0.00025f);
				}
				if (n % 10 == 6)
				{
					Button6.transform.localPosition = new Vector3(Button6.transform.localPosition.x - 0.00025f, Button6.transform.localPosition.y, Button6.transform.localPosition.z + 0.000125f);
				}
				if ((n / 10) % 10 == 1)
				{
					Button1.transform.localPosition = new Vector3(Button1.transform.localPosition.x + 0.00025f, Button1.transform.localPosition.y, Button1.transform.localPosition.z - 0.000125f);
				}
				if ((n / 10) % 10 == 2)
				{
					Button2.transform.localPosition = new Vector3(Button2.transform.localPosition.x, Button2.transform.localPosition.y, Button2.transform.localPosition.z - 0.00025f);
				}
				if ((n / 10) % 10 == 3)
				{
					Button3.transform.localPosition = new Vector3(Button3.transform.localPosition.x - 0.00025f, Button3.transform.localPosition.y, Button3.transform.localPosition.z - 0.000125f);
				}
				if ((n / 10) % 10 == 4)
				{
					Button4.transform.localPosition = new Vector3(Button4.transform.localPosition.x + 0.00025f, Button4.transform.localPosition.y, Button4.transform.localPosition.z + 0.000125f);
				}
				if ((n / 10) % 10 == 5)
				{
					Button5.transform.localPosition = new Vector3(Button5.transform.localPosition.x, Button5.transform.localPosition.y, Button5.transform.localPosition.z + 0.00025f);
				}
				if ((n / 10) % 10 == 6)
				{
					Button6.transform.localPosition = new Vector3(Button6.transform.localPosition.x - 0.00025f, Button6.transform.localPosition.y, Button6.transform.localPosition.z + 0.000125f);
				}
				if ((n / 100) % 10 == 1)
				{
					Button1.transform.localPosition = new Vector3(Button1.transform.localPosition.x + 0.00025f, Button1.transform.localPosition.y, Button1.transform.localPosition.z - 0.000125f);
				}
				if ((n / 100) % 10 == 2)
				{
					Button2.transform.localPosition = new Vector3(Button2.transform.localPosition.x, Button2.transform.localPosition.y, Button2.transform.localPosition.z - 0.00025f);
				}
				if ((n / 100) % 10 == 3)
				{
					Button3.transform.localPosition = new Vector3(Button3.transform.localPosition.x - 0.00025f, Button3.transform.localPosition.y, Button3.transform.localPosition.z - 0.000125f);
				}
				if ((n / 100) % 10 == 4)
				{
					Button4.transform.localPosition = new Vector3(Button4.transform.localPosition.x + 0.00025f, Button4.transform.localPosition.y, Button4.transform.localPosition.z + 0.000125f);
				}
				if ((n / 100) % 10 == 5)
				{
					Button5.transform.localPosition = new Vector3(Button5.transform.localPosition.x, Button5.transform.localPosition.y, Button5.transform.localPosition.z + 0.00025f);
				}
				if ((n / 100) % 10 == 6)
				{
					Button6.transform.localPosition = new Vector3(Button6.transform.localPosition.x - 0.00025f, Button6.transform.localPosition.y, Button6.transform.localPosition.z + 0.000125f);
				}
				t2 = (t / 50) % 6;
				t++;
			}
			else if (n != k) { status = 0; n = 0; t = 0; targetT = 1; }
			else 
			{ 
				Button1.GetComponent<MeshRenderer>().material.color = new Color(.25f, .25f, .25f);
				Button2.GetComponent<MeshRenderer>().material.color = new Color(.25f, .25f, .25f);
				Button3.GetComponent<MeshRenderer>().material.color = new Color(.25f, .25f, .25f);
				Button4.GetComponent<MeshRenderer>().material.color = new Color(.25f, .25f, .25f);
				Button5.GetComponent<MeshRenderer>().material.color = new Color(.25f, .25f, .25f);
				Button6.GetComponent<MeshRenderer>().material.color = new Color(.25f, .25f, .25f);
				Button1.GetComponentInChildren<TextMesh>().text = "";
				Button2.GetComponentInChildren<TextMesh>().text = "";
				Button3.GetComponentInChildren<TextMesh>().text = "";
				Button4.GetComponentInChildren<TextMesh>().text = "";
				Button5.GetComponentInChildren<TextMesh>().text = "";
				Button6.GetComponentInChildren<TextMesh>().text = "";
				Module.HandlePass();
			}
		}
	}

	void CbDisplay()
	{
		if (colorblind)
		{
			Button1.GetComponentInChildren<TextMesh>().text = ColourSingle[OldColours[0]]; if (OldColours[0] == 0) { Button1.GetComponentInChildren<TextMesh>().color = new Color(1, 1, 1); }
			Button2.GetComponentInChildren<TextMesh>().text = ColourSingle[OldColours[1]]; if (OldColours[1] == 0) { Button2.GetComponentInChildren<TextMesh>().color = new Color(1, 1, 1); }
			Button3.GetComponentInChildren<TextMesh>().text = ColourSingle[OldColours[2]]; if (OldColours[2] == 0) { Button3.GetComponentInChildren<TextMesh>().color = new Color(1, 1, 1); }
			Button4.GetComponentInChildren<TextMesh>().text = ColourSingle[OldColours[3]]; if (OldColours[3] == 0) { Button4.GetComponentInChildren<TextMesh>().color = new Color(1, 1, 1); }
			Button5.GetComponentInChildren<TextMesh>().text = ColourSingle[OldColours[4]]; if (OldColours[4] == 0) { Button5.GetComponentInChildren<TextMesh>().color = new Color(1, 1, 1); }
			Button6.GetComponentInChildren<TextMesh>().text = ColourSingle[OldColours[5]]; if (OldColours[5] == 0) { Button6.GetComponentInChildren<TextMesh>().color = new Color(1, 1, 1); }
		}
		else
		{
			Button1.GetComponentInChildren<TextMesh>().text = "";
			Button2.GetComponentInChildren<TextMesh>().text = "";
			Button3.GetComponentInChildren<TextMesh>().text = "";
			Button4.GetComponentInChildren<TextMesh>().text = "";
			Button5.GetComponentInChildren<TextMesh>().text = "";
			Button6.GetComponentInChildren<TextMesh>().text = "";
		}
	}

#pragma warning disable 414
	private string TwitchHelpMessage = "'!{0} colorblind' to toggle colorblind mode, '!{0} tl/tm/tr/bl/bm/br' to press a button. e.g. '!{0} tr br tm'";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		yield return null;
		command = command.ToLowerInvariant();
		if (command == "colorblind") { colorblind = !colorblind; CbDisplay(); }
		else
		{
			TPCmds = command.Split(' ');
			for (int i = 0; i < TPCmds.Length; i++)
			{
				string[] validCommands = new string[6] { "tl", "tm", "tr", "bl", "bm", "br" };
				if (!validCommands.Contains(TPCmds[i]))
				{
					yield return "sendtochaterror @{0}, invalid command.";
					yield break;
				}
			}
			for (int i = 0; n < 100 && i < TPCmds.Length; i++)
			{
				yield return null;
				while ((!((status == 0 && t2 == 4 && t % 50 <= 5) || status == 1 && t2 == targetT))) { yield return new WaitForSeconds(0.1f); }
				if (TPCmds[i] == "tl") { Button1.OnInteract(); }
				else if (TPCmds[i] == "tm") { Button2.OnInteract(); }
				else if (TPCmds[i] == "tr") { Button3.OnInteract(); }
				else if (TPCmds[i] == "bl") { Button4.OnInteract(); }
				else if (TPCmds[i] == "bm") { Button5.OnInteract(); }
				else if (TPCmds[i] == "br") { Button6.OnInteract(); }
				yield return new WaitForSeconds(1.02f);
			}
			yield return "solve";
		}
		yield return null;
	}
	IEnumerator TwitchHandleForcedSolve()
	{
		status = -1;
		yield return new WaitForSeconds(0.05f);
		yield return null;
	}
}