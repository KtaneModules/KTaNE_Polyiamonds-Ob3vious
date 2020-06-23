using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class simonPentiamondssScript : MonoBehaviour
{
	//publics
	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMBombInfo Bomb;
	public KMSelectable[] Button;
	public KMBombModule Module;
	public KMColorblindMode CBM;

	private int[] InitialColours = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };
	private float[] Red = { 0f, 0f, 0f, .5f, .5f, .5f, .5f, .5f, .5f, 1f, 1f, 1f };
	private float[] Green = { 0f, .5f, .5f, 0f, 0f, .5f, .5f, 1f, 1f, .5f, .5f, 1f };
	private float[] Blue = { .5f, 0f, .5f, 0f, .5f, 0f, 1f, .5f, 1f, .5f, 1f, .5f };
	private int[][] DispColours = { new int[] { 0, 0, 0, 0, 0, 0 }, new int[] { 0, 0, 0, 0, 0, 0 } };
	private string[] ColourSingle = { "Na", "Fo", "Te", "Ma", "Pu", "Ol", "Co", "Mi", "To", "Sa", "Pi", "Be" };
	private int[][] InitialPos = { new int[] { 0, 1, 2, 3, 4, 5 }, new int[] { 0, 1, 2, 3, 4, 5 } };
	private int[][] DispPos = { new int[] { 0, 0, 0 }, new int[] { 0, 0, 0 } };
	private int Index;
	private float[][] Xtransl = { new float[] { -0.00025f, 0f, 0.00025f, -0.00025f, 0f, 0.00025f }, new float[] { -0.000125f, 0.000125f, -0.00025f, 0.00025f, -0.000125f, 0.000125f } };
	private float[][] Ztransl = { new float[] { 0.000125f, 0.00025f, 0.000125f, -0.000125f, -0.00025f, -0.000125f }, new float[] { 0.00025f, 0.00025f, 0, 0, -0.00025f, -0.00025f } };
	private float[][] Xmain = { new float[] { -0.03f, 0f, 0.03f, -0.03f, 0f, 0.03f }, new float[] { -0.02f, 0.02f, -0.02f, 0.02f, -0.02f, 0.02f } };
	private float[][] Zmain = { new float[] { 0.03f, 0.03f, 0.03f, -0.03f, -0.03f, -0.03f }, new float[] { 0.02f, 0.02f, 0f, 0f, -0.02f, -0.02f } };
	private bool colorblind;
	private bool[] available = { true, true, true, true, true, true, true, true, true, true, true, true };
	private int status = 0;
	private int n = 0;
	private int k = 0;
	private int t = 0;
	private int t2 = 0;
	private int targetT = 1;

	private KMSelectable.OnInteractHandler ButtonPressed(int pos)
	{
		return delegate
		{
			if ((status == 0 && t2 == 4 || status == 1 && t2 == targetT) && available[pos])
			{
				//Audio.PlaySoundAtTransform(SFX[pos], Button[pos].transform);
				Button[pos].AddInteractionPunch();
				n = n * 100 + pos + 1;
				targetT++;
				available[pos] = false;
			}
			return false;
		};
	}

	void Awake()
	{
		colorblind = CBM.ColorblindModeActive;

		for (int i = 0; i < Button.Length; i++)
		{
			Button[i].OnInteract += ButtonPressed(i);
		}
		for (int i = 0; i < 6; i++)
		{
			for (int i2 = 0; i2 < 2; i2++)
			{
				Index = Rnd.Range(0, 12 - 2 * i - i2);
				DispColours[i2][i] = InitialColours[Index];
				InitialColours = InitialColours.Where((j, index) => index != Index).ToArray();
				Button[i + 6 * i2].GetComponent<MeshRenderer>().material.color = new Color(Red[DispColours[i2][i]], Green[DispColours[i2][i]], Blue[DispColours[i2][i]]);
			}
		}
		for (int i = 0; i < 3; i++)
		{
			for (int i2 = 0; i2 < 2; i2++)
			{
				Index = Rnd.Range(0, 6 - i);
				DispPos[i2][i] = InitialPos[i2][Index];
				InitialPos[i2] = InitialPos[i2].Where((j, index) => index != Index).ToArray();
			}
		}

		CbDisplay();
	}

	void FixedUpdate()
	{
		if (status == 0)
		{
			if (t2 <= 2)
			{
				Button[DispPos[0][t2]].transform.localPosition = new Vector3(Button[DispPos[0][t2]].transform.localPosition.x + Xtransl[0][DispPos[0][t2]], Button[DispPos[0][t2]].transform.localPosition.y, Button[DispPos[0][t2]].transform.localPosition.z + Ztransl[0][DispPos[0][t2]]);
				Button[DispPos[1][t2] + 6].transform.localPosition = new Vector3(Button[DispPos[1][t2] + 6].transform.localPosition.x + Xtransl[1][DispPos[1][t2]], Button[DispPos[1][t2] + 6].transform.localPosition.y, Button[DispPos[1][t2] + 6].transform.localPosition.z + Ztransl[1][DispPos[1][t2]]);
				//if (t % 50 == 0 && annoy) { Audio.PlaySoundAtTransform(SFX[DispPos[t2]], Module.transform); }
			}
			if (t2 == 3)
			{
				//if (t % 50 == 0 && annoy) { Audio.PlaySoundAtTransform(SFX[6], Module.transform); }
				for (int i = 0; i < 6; i++)
				{
					Button[DispPos[i % 2][i / 2] + 6 * (i % 2)].transform.localPosition = new Vector3(Button[DispPos[i % 2][i / 2] + 6 * (i % 2)].transform.localPosition.x - Xtransl[i % 2][DispPos[i % 2][i / 2]], Button[DispPos[i % 2][i / 2] + 6 * (i % 2)].transform.localPosition.y, Button[DispPos[i % 2][i / 2] + 6 * (i % 2)].transform.localPosition.z - Ztransl[i % 2][DispPos[i % 2][i / 2]]);
				}
			}
			if (t2 == 4)
			{
				for (int i = 0; i < 12; i++) { Button[i].transform.localPosition = new Vector3(Xmain[i / 6][i % 6], Button[i].transform.localPosition.y, Zmain[i / 6][i % 6]); }
				if (n > 0) { status++; t2 = 1; t = 49; }
			}
			t++;
			t2 = (t / 50) % 5;
		}
		if (status == 1)
		{
			if (targetT > t2)
			{
				Debug.LogFormat("[Pentiamonds #{0}] {1}", 1, n);
				Button[n % 100 - 1].transform.localPosition = new Vector3(Button[n % 100 - 1].transform.localPosition.x + Xtransl[(n % 100 - 1)/6][(n % 100 - 1)%6], Button[n % 100 - 1].transform.localPosition.y, Button[n % 100 - 1].transform.localPosition.z + Ztransl[(n % 100 - 1)/6][(n % 100 - 1)%6]);
				t++;
				t2 = (t / 50);
			}
			else
			{
				if (n > 100000000) { status = 2; }
			}
		}
	}

	void CbDisplay()
	{
		for (int i = 0; i < 12; i++)
		{
			if (colorblind)
			{
				Button[i].GetComponentInChildren<TextMesh>().text = ColourSingle[DispColours[i/6][i%6]];
			}
			else
			{
				Button[i].GetComponentInChildren<TextMesh>().text = "";
			}
		}
	}
}