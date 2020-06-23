using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class simonTetriamondsScript : MonoBehaviour
{
	//publics
	public KMAudio Audio;
	public AudioClip[] sounds;
	public KMBombInfo Bomb;
	public KMSelectable[] Button;
	public KMBombModule Module;
	public KMColorblindMode CBM;
	//visuals
	private int[] InitialColours = { 0, 1, 2, 3, 4, 5, 6 };
	private float[] Red = { .5f, 1f, 1f, .5f, 0f, 0f, .5f };
	private float[] Green = { .5f, 0f, .5f, 1f, 1f, .5f, 0f };
	private float[] Blue = { .5f, .5f, 0f, .0f, .5f, 1f, 1f };
	private int[] DispColours = { 0, 0, 0, 0, 0, 0 };
	private string[] Colours = { "grey", "rose", "orange", "lime", "jade", "azure", "violet" };
	private string[] ColourSingle = { "X", "E", "O", "L", "J", "A", "V" };
	private string[] Directions = { "left", "right", "right", "left", "left", "right" };
	private string[] PosName = { "top left", "top right", "middle left", "middle right", "bottom left", "bottom right" };
	int[] InitialPos = { 0, 1, 2, 3, 4, 5 };
	int[] DispPos = { 0, 0, 0 };
	private string[] SFX = { "SimonC4", "SimonG#", "SimonC5", "SimonD", "SimonE", "SimonA#", "SimonF#" };
	private int Index;
	private bool annoy = false;
	private float[] Xtransl = { -0.000125f, 0.000125f, -0.00025f, 0.00025f, -0.000125f, 0.000125f };
	private float[] Ztransl = { 0.00025f, 0.00025f, 0, 0, -0.00025f, -0.00025f };
	private float[] Xmain = { -0.03f, 0.03f, -0.03f, 0.03f, -0.03f, 0.03f };
	private float[] Zmain = { 0.03f, 0.03f, 0f, 0f, -0.03f, -0.03f };
	private bool colorblind;
	//functionality
	private bool[] available = { true, true, true, true, true, true };
	private int status = 0;
	private int n = 0;
	private int k = 0;
	private int t = 0;
	private int t2 = 0;
	private int targetT = 1;
	//selfsolving
	private bool[] col = { false, false, false, false, false, false, false, false };
	private bool[] ord = { false, false, false, false, false, false, false };
	private int OrderPos;
	private int[] grid = { -1, -1, -1, -1, -1, -1, -1, -1 };
	private int Pivot = 0;
	private int pivotOrd = 0;
	private bool pivotUsed = false;
	private int Valid = 0;
	private int LastGrey = 0;
	private int[] PosArray = { 1, 3, 0, 5, 2, 4 };
	private int[] OrderArray = { 0, 0, 0, 0, 0, 0, 0 };
	private int[] InvArray = { 0, 0, 0, 0, 0, 0, 0 };
	private int type;

	private int[] Utransf = { 2, 0, 1, 3, 5, 4 };
	private int[] Istore = { 0, 0, 0, 0 };
	private int[] Iorient = { 1, 3, 4 };
	private int[][] ItrEven = { new int[] { 0, 2, 3, 5 }, new int[] { 4, 5, 0, 1 }, new int[] { 2, 4, 1, 3 } };
	private int[][] ItrOdds = { new int[] { 5, 4, 1, 0 }, new int[] { 1, 3, 2, 4 }, new int[] { 3, 5, 0, 2 } };
	private int[] Atransf = { 1, 4, 0, 6, 3, 7 };
	private int[][] Atrans2 = { new int[] { 4, 0, 1, 3 }, new int[] { 0, 3, 2, 4 }, new int[] { 3, 4, 5, 0 }, new int[] { 5, 0, 1, 2 }, new int[] { 1, 3, 2, 5 }, new int[] { 2, 4, 5, 1 } };
	//external
	static int _moduleIdCounter = 1;
	int _moduleID = 0;
	private string[] TPCmds;


	private KMSelectable.OnInteractHandler ButtonPressed(int pos)
	{
		return delegate
		{
			if ((status == 0 && t2 == 4 || status == 1 && t2 == targetT) && available[pos])
			{
				Audio.PlaySoundAtTransform(SFX[pos], Button[pos].transform); 
				Button[pos].AddInteractionPunch();
				n = n * 10 + pos + 1;
				targetT++;
				available[pos] = false;
			}
			return false;
		};
	}

	void Awake()
	{
		_moduleID = _moduleIdCounter++;
		colorblind = CBM.ColorblindModeActive;
		for (int i = 0; i < Button.Length; i++)
		{
			Button[i].OnInteract += ButtonPressed(i);
		}

		//Initialising colours and positions
		for (int i = 0; i < 6; i++)
		{
			Index = Rnd.Range(0, 7 - i);
			DispColours[i] = InitialColours[Index];
			InitialColours = InitialColours.Where((j, index) => index != Index).ToArray();
			Button[i].GetComponent<MeshRenderer>().material.color = new Color(Red[DispColours[i]], Green[DispColours[i]], Blue[DispColours[i]]);
		}
		for (int i = 0; i < 3; i++)
		{
			Index = Rnd.Range(0, 6 - i);
			DispPos[i] = InitialPos[Index];
			InitialPos = InitialPos.Where((j, index) => index != Index).ToArray();
		}

		//Determining tetriamond colours
		Colours:
		for (int i = 0; i < 3; i++) { col[DispColours[DispPos[i]]] = true; }
		if (!col[0] || ((col[1] && col[2]) || (col[2] && col[3]) || (col[3] && col[4]) || (col[4] && col[5]) || (col[5] && col[6]) || (col[6] && col[1]))) { for (int i = 0; i < 7; i++) { col[i] = !col[i]; } Debug.LogFormat("[Tetriamonds #{0}] Swapped colours.", _moduleID); goto Shape; }
		else if ((col[1] && col[4]) || (col[2] && col[5]) || (col[3] && col[6])) { OrderPos = DispPos[0]; for (int i = 0; i < 3; i++) { OrderPos = PosArray[OrderPos]; if (!col[DispColours[OrderPos]]) { Pivot = DispColours[OrderPos]; Debug.LogFormat("[Tetriamonds #{0}] Added colour {1}.", _moduleID, Colours[DispColours[OrderPos]]); goto Shape; } } }
		else { if (DispColours[DispPos[0]] != 0) { Pivot = (DispColours[DispPos[0]] + 2) % 6 + 1; Debug.LogFormat("[Tetriamonds #{0}] Added colour {1}.", _moduleID, Colours[(DispColours[DispPos[0]] + 2) % 6 + 1]); } else { Pivot = (DispColours[DispPos[1]] + 2) % 6 + 1; Debug.LogFormat("[Tetriamonds #{0}] Added colour {1}.", _moduleID, Colours[(DispColours[DispPos[1]] + 2) % 6 + 1]); } goto Shape; }

		//Determining tetriamond shape
		Shape:
		if (Directions[DispPos[0]] != Directions[DispPos[1]]) { Debug.LogFormat("[Tetriamonds #{0}] Constructing a U-shape.", _moduleID); type = 1; }
		else if (Directions[DispPos[1]] != Directions[DispPos[2]]) { Debug.LogFormat("[Tetriamonds #{0}] Constructing a rhomboid.", _moduleID); type = 2; }
		else { Debug.LogFormat("[Tetriamonds #{0}] Constructing a triangle.", _moduleID); type = 3; }

		//Determining button order
		Order:
		for (int i = 0; i < 3; i++) { ord[DispColours[DispPos[i]]] = true; OrderArray[DispColours[DispPos[i]]] = i; InvArray[i] = DispColours[DispPos[i]]; }
		for (int i = 0; i < 3; i++) { OrderPos = DispPos[i]; int j = 0; while (j == 0) { OrderPos = PosArray[OrderPos]; if (!ord[DispColours[OrderPos]]) { ord[DispColours[OrderPos]] = true; OrderArray[DispColours[OrderPos]] = i + 3; InvArray[i + 3] = DispColours[OrderPos]; j++; } } }
		for (int i = 0; i < 7; i++) { if (!ord[i]) { ord[i] = true; OrderArray[i] = 6; InvArray[6] = i; if (Pivot == 0) { Pivot = i; if (i != 0) { col[i] = false; } } } }
		Debug.LogFormat("[Tetriamonds #{0}] Order: {1}, {2}, {3}, {4}, {5}, {6}, {7}.", _moduleID, Colours[InvArray[0]], Colours[InvArray[1]], Colours[InvArray[2]], Colours[InvArray[3]], Colours[InvArray[4]], Colours[InvArray[5]], Colours[InvArray[6]]);

		//Determining tetriamond orientation
		//To do: Grey comes 7th; work out triangle; decrypt grid; add TP;
		Orientation:
		for (int i = 1; i < 7; i++) { if (Pivot == 0 && OrderArray[i] == 5) { Pivot = i; col[i] = false; } }
		if (type == 1 || type == 2)
		{
			while (true)
			{
				for (int i = 0; i < 6; i++) { if (col[i + 1]) { grid[i] = OrderArray[i + 1]; } else { grid[i] = -1; } }
				grid[Pivot - 1] = OrderArray[Pivot];
				if (!col[0])
				{
					//Debug.LogFormat("[Tetriamonds #{0}] Grid: {1} {2} {3} {4} {5} {6}.", _moduleID, grid[0], grid[1], grid[2], grid[3], grid[4], grid[5]);
					if (((grid[0] == -1) && (grid[1] == -1)) || ((grid[1] == -1) && (grid[2] == -1)) || ((grid[2] == -1) && (grid[3] == -1)) || ((grid[3] == -1) && (grid[4] == -1)) || ((grid[4] == -1) && (grid[5] == -1)) || ((grid[5] == -1) && (grid[0] == -1))) { goto Decrypt; } else { if (!pivotUsed) { Debug.LogFormat("[Tetriamonds #{0}] Invalid. Swapping out {1}.", _moduleID, Colours[Pivot]); pivotOrd = 0; while (col[InvArray[pivotOrd]] || InvArray[pivotOrd] == 0) { pivotOrd++; } Pivot = InvArray[pivotOrd]; pivotUsed = true; } else { pivotOrd++; while (col[InvArray[pivotOrd]] || InvArray[pivotOrd] == 0) { pivotOrd++; } Pivot = InvArray[pivotOrd]; } }
				}
				else
				{
					for (int i = 0; i < 6; i++)
					{
						if (grid[i] == -1)
						{
							grid[i] = OrderArray[0];
							//Debug.LogFormat("[Tetriamonds #{0}] Grid: {1} {2} {3} {4} {5} {6}.", _moduleID, grid[0], grid[1], grid[2], grid[3], grid[4], grid[5]);
							if (((grid[0] == -1) && (grid[1] == -1)) || ((grid[1] == -1) && (grid[2] == -1)) || ((grid[2] == -1) && (grid[3] == -1)) || ((grid[3] == -1) && (grid[4] == -1)) || ((grid[4] == -1) && (grid[5] == -1)) || ((grid[5] == -1) && (grid[0] == -1))) { Valid++; LastGrey = i; }
							grid[i] = -1;
						}
					}
					if (Valid == 1) { grid[LastGrey] = OrderArray[0]; goto Decrypt; } else { if (!pivotUsed) { Debug.LogFormat("[Tetriamonds #{0}] Invalid. Swapping out {1}.", _moduleID, Colours[Pivot]); pivotOrd = 0; while (col[InvArray[pivotOrd]] || InvArray[pivotOrd] == 0) { pivotOrd++; } Pivot = InvArray[pivotOrd]; pivotUsed = true; } else { pivotOrd++; while (col[InvArray[pivotOrd]] || InvArray[pivotOrd] == 0) { pivotOrd++; } Pivot = InvArray[pivotOrd]; } Valid = 0; }
				}
			}
		}
		else if (type == 3)
		{
			while (true)
			{
				grid[2] = -1; grid[5] = -1;
				for (int i = 0; i < 6; i++) { if (col[i + 1]) { grid[Atransf[i]] = OrderArray[i + 1]; } else { grid[Atransf[i]] = -1; } }
				grid[Atransf[Pivot - 1]] = OrderArray[Pivot];

				//Debug.LogFormat("[Tetriamonds #{0}] grid: {1} {2} {3} {4} {5} {6} {7} {8}.", _moduleID, grid[0], grid[1], grid[2], grid[3], grid[4], grid[5], grid[6], grid[7]);
				if ((grid[0] != -1) && (grid[1] != -1) && (grid[3] != -1))
				{
					if (grid[4] != -1) { grid[2] = grid[4]; } else if (grid[6] != -1) { grid[2] = grid[6]; } else if (grid[7] != -1) { grid[2] = grid[7]; } else { grid[2] = OrderArray[0]; }
					goto Decrypt;
				}
				else if (((grid[4] != -1) && (grid[6] != -1) && (grid[7] != -1)))
				{
					if (grid[0] != -1) { grid[5] = grid[0]; } else if (grid[1] != -1) { grid[5] = grid[1]; } else if (grid[3] != -1) { grid[5] = grid[3]; } else { grid[5] = OrderArray[0]; }
					goto Decrypt;
				}
				else if (col[0])
				{
					if (grid[4] == grid[6] || grid[4] == grid[7] || grid[6] == grid[7]) { for (int i = 0; i < 4; i++) { if (grid[i + 4] != -1) { grid[2] = grid[i + 4]; } } for (int i = 0; i < 4; i++) { if (grid[i] == -1) { grid[i] = OrderArray[0]; } } } else { for (int i = 0; i < 4; i++) { if (grid[i] != -1) { grid[5] = grid[i]; } } for (int i = 0; i < 4; i++) { if (grid[i + 4] == -1) { grid[i + 4] = OrderArray[0]; } } }
					goto Decrypt;
				}
				else
				{
					if (!pivotUsed) { Debug.LogFormat("[Tetriamonds #{0}] Invalid. Swapping out {1}.", _moduleID, Colours[Pivot]); pivotOrd = 0; while (col[InvArray[pivotOrd]] || InvArray[pivotOrd] == 0) { pivotOrd++; } Pivot = InvArray[pivotOrd]; pivotUsed = true; } else { pivotOrd++; while (col[InvArray[pivotOrd]] || InvArray[pivotOrd] == 0) { pivotOrd++; } Pivot = InvArray[pivotOrd]; } Valid = 0;
				}
			}
		}


		//Determining solution
		Decrypt:
		if (pivotUsed) { Debug.LogFormat("[Tetriamonds #{0}] First valid colour is {1}.", _moduleID, Colours[Pivot]); }
		//Debug.LogFormat("[Tetriamonds #{0}] Grid: {1} {2} {3} {4} {5} {6} {7} {8}.", _moduleID, grid[0], grid[1], grid[2], grid[3], grid[4], grid[5], grid[6], grid[7]);

		if (type == 1)
		{
			for (int i = 0; i < 7; i++)
			{
				for (int j = 0; j < 6; j++)
				{
					if (grid[j] == i) { k = k * 10 + Utransf[j] + 1; }
				}
			}
		}
		else if (type == 2)
		{
			for (int i = 0; i < 6; i++)
			{
				if (grid[i] == -1 && grid[(i + 1) % 6] != -1) { Index = i; };
			}
			for (int i = 0; i < 4; i++)
			{
				Istore[i] = grid[(i + Index + 1) % 6];
			}
			//Debug.LogFormat("[Tetriamonds #{0}] Temporary grid {1} {2} {3} {4}.", _moduleID, Istore[0], Istore[1], Istore[2], Istore[3]);
			//Debug.LogFormat("[Tetriamonds #{0}] Index: {1}, Orientation: {2}.", _moduleID, Index, DispPos[1] - DispPos[0]);
			for (int i = 0; i < 3; i++)
			{
				if (Math.Abs(DispPos[1] - DispPos[0]) == Iorient[i])
				{
					if (Index % 2 != 0)
					{
						for (int j = 0; j < 7; j++)
						{
							for (int l = 0; l < 4; l++)
							{
								if (Istore[l] == j)
								{
									if (DispPos[1] > DispPos[0]) { k = k * 10 + ItrEven[i][l] + 1; } else { k = k * 10 + ItrEven[i][3 - l] + 1; }
								}
							}
						}
					}
					else
					{
						for (int j = 0; j < 7; j++)
						{
							for (int l = 0; l < 4; l++)
							{
								if (Istore[l] == j)
								{
									if (DispPos[1] > DispPos[0]) { k = k * 10 + ItrOdds[i][l] + 1; } else { k = k * 10 + ItrOdds[i][3 - l] + 1; }
								}
							}
						}
					}
				}
			}
		}
		else if (type == 3)
		{
			//Debug.LogFormat("[Tetriamonds #{0}] grid: {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}.", _moduleID, grid[0], grid[1], grid[2], grid[3], grid[4], grid[5], grid[6], grid[7]);
			if (grid[5] == -1)
			{
				if (grid[0] > grid[1] && grid[0] > grid[3])
				{
					for (int i = 0; i < 7; i++)
					{
						for (int j = 0; j < 4; j++)
						{
							if (grid[j] == i) { k = k * 10 + Atrans2[0][j] + 1; }
						}
					}
				}
				else if (grid[1] > grid[0] && grid[1] > grid[3])
				{
					for (int i = 0; i < 7; i++)
					{
						for (int j = 0; j < 4; j++)
						{
							if (grid[j] == i) { k = k * 10 + Atrans2[1][j] + 1; }
						}
					}
				}
				else if (grid[3] > grid[0] && grid[3] > grid[1])
				{
					for (int i = 0; i < 7; i++)
					{
						for (int j = 0; j < 4; j++)
						{
							if (grid[j] == i) { k = k * 10 + Atrans2[2][j] + 1; }
						}
					}
				}
			}
			else
			{
				if (grid[4] > grid[6] && grid[4] > grid[7])
				{
					for (int i = 0; i < 7; i++)
					{
						for (int j = 0; j < 4; j++)
						{
							if (grid[j + 4] == i) { k = k * 10 + Atrans2[3][j] + 1; }
						}
					}
				}
				else if (grid[6] > grid[4] && grid[6] > grid[7])
				{
					for (int i = 0; i < 7; i++)
					{
						for (int j = 0; j < 4; j++)
						{
							if (grid[j + 4] == i) { k = k * 10 + Atrans2[4][j] + 1; }
						}
					}
				}
				else if (grid[7] > grid[4] && grid[7] > grid[6])
				{
					for (int i = 0; i < 7; i++)
					{
						for (int j = 0; j < 4; j++)
						{
							if (grid[j + 4] == i) { k = k * 10 + Atrans2[5][j] + 1; }
						}
					}
				}
			}
		}

		Debug.LogFormat("[Tetriamonds #{0}] Expected presses: {1}, {2}, {3}, {4}.", _moduleID, PosName[k / 1000 - 1], PosName[(k / 100) % 10 - 1], PosName[(k / 10) % 10 - 1], PosName[k % 10 - 1]);

		CbDisplay();
	}

	void FixedUpdate()
	{
		if (status == -1)
		{
			for (int i = 0; i < 6; i++) { Button[i].GetComponent<MeshRenderer>().material.color = new Color(0.75f, 0.75f, 0.75f); Button[i].transform.localPosition = new Vector3(Xmain[i], 0f, Zmain[i]); }
		}
		if (status == 0)
		{
			if (t2 <= 2)
			{
				Button[DispPos[t2]].transform.localPosition = new Vector3(Button[DispPos[t2]].transform.localPosition.x + Xtransl[DispPos[t2]], Button[DispPos[t2]].transform.localPosition.y, Button[DispPos[t2]].transform.localPosition.z + Ztransl[DispPos[t2]]);
				if (t % 50 == 0 && annoy) { Audio.PlaySoundAtTransform(SFX[DispPos[t2]], Module.transform); }
			}
			if (t2 == 3)
			{
				if (t % 50 == 0 && annoy) { Audio.PlaySoundAtTransform(SFX[6], Module.transform); }
				Button[DispPos[0]].transform.localPosition = new Vector3(Button[DispPos[0]].transform.localPosition.x - Xtransl[DispPos[0]], Button[DispPos[0]].transform.localPosition.y, Button[DispPos[0]].transform.localPosition.z - Ztransl[DispPos[0]]);
				Button[DispPos[1]].transform.localPosition = new Vector3(Button[DispPos[1]].transform.localPosition.x - Xtransl[DispPos[1]], Button[DispPos[1]].transform.localPosition.y, Button[DispPos[1]].transform.localPosition.z - Ztransl[DispPos[1]]);
				Button[DispPos[2]].transform.localPosition = new Vector3(Button[DispPos[2]].transform.localPosition.x - Xtransl[DispPos[2]], Button[DispPos[2]].transform.localPosition.y, Button[DispPos[2]].transform.localPosition.z - Ztransl[DispPos[2]]);
			}
			if (t2 == 4)
			{
				for (int i = 0; i < 6; i++) { Button[i].transform.localPosition = new Vector3(Xmain[i], 0f, Zmain[i]); }
				if (n > 0) { status++; t2 = 1; t = 49; }
			}
			t++;
			t2 = (t / 50) % 5;
		}
		if (status == 1)
		{
			if (targetT > t2)
			{
				Button[n % 10 - 1].transform.localPosition = new Vector3(Button[n % 10 - 1].transform.localPosition.x + Xtransl[n % 10 - 1], Button[n % 10 - 1].transform.localPosition.y, Button[n % 10 - 1].transform.localPosition.z + Ztransl[n % 10 - 1]);
				t++;
				t2 = (t / 50);
			}
			else
			{
				if (n > 1000) { status = 2; }
			}
		}
		if (status == 2)
		{
			if (t2 > 3)
			{
				if (n == k)
				{
					t2 = 0;
					t = 1;
					status = 3;
					Audio.PlaySoundAtTransform(SFX[6], Module.transform);
				}
				else
				{
					Debug.LogFormat("[Tetriamonds #{0}] Pressed: {1}, {2}, {3}, {4}. Expected: {5}, {6}, {7}, {8}.", _moduleID, PosName[n / 1000 - 1], PosName[(n / 100) % 10 - 1], PosName[(n / 10) % 10 - 1], PosName[n % 10 - 1], PosName[k / 1000 - 1], PosName[(k / 100) % 10 - 1], PosName[(k / 10) % 10 - 1], PosName[k % 10 - 1]);
					Module.HandleStrike();
					annoy = true;
					Audio.PlaySoundAtTransform(SFX[6], Module.transform);
					for (int i = 0; i < 6; i++) { available[i] = true; }
					t2 = 0;
					t = 1;
					status = 3;
				}
			}
		}
		if (status == 3)
		{
			if (t2 == 0)
			{
				Button[n / 1000 - 1].transform.localPosition = new Vector3(Button[n / 1000 - 1].transform.localPosition.x - Xtransl[n / 1000 - 1], Button[n / 1000 - 1].transform.localPosition.y, Button[n / 1000 - 1].transform.localPosition.z - Ztransl[n / 1000 - 1]);
				Button[(n / 100) % 10 - 1].transform.localPosition = new Vector3(Button[(n / 100) % 10 - 1].transform.localPosition.x - Xtransl[(n / 100) % 10 - 1], Button[(n / 100) % 10 - 1].transform.localPosition.y, Button[(n / 100) % 10 - 1].transform.localPosition.z - Ztransl[(n / 100) % 10 - 1]);
				Button[(n / 10) % 10 - 1].transform.localPosition = new Vector3(Button[(n / 10) % 10 - 1].transform.localPosition.x - Xtransl[(n / 10) % 10 - 1], Button[(n / 10) % 10 - 1].transform.localPosition.y, Button[(n / 10) % 10 - 1].transform.localPosition.z - Ztransl[(n / 10) % 10 - 1]);
				Button[n % 10 - 1].transform.localPosition = new Vector3(Button[n % 10 - 1].transform.localPosition.x - Xtransl[n % 10 - 1], Button[n % 10 - 1].transform.localPosition.y, Button[n % 10 - 1].transform.localPosition.z - Ztransl[n % 10 - 1]);
				t2 = (t / 50) % 6;
				t++;
			}
			else if (n != k) { status = 0; n = 0; t = 0; targetT = 1; }
			else
			{
				for (int i = 0; i < 6; i++)
				{
					Button[i].GetComponent<MeshRenderer>().material.color = new Color(.75f, .75f, .75f);
					Button[i].GetComponentInChildren<TextMesh>().text = "";
				}
				Module.HandlePass();
			}
		}
	}

	void CbDisplay()
	{
		for (int i = 0; i < 6; i++)
		{
			if (colorblind)
			{
				Button[i].GetComponentInChildren<TextMesh>().text = ColourSingle[DispColours[i]];
			}
			else
			{
				Button[i].GetComponentInChildren<TextMesh>().text = "";
			}
		}
	}

#pragma warning disable 414
	private string TwitchHelpMessage = "'!{0} colorblind' to toggle colorblind mode, '!{0} tl/tr/ml/mr/bl/br' to press a button. e.g. '!{0} tr br ml mr'";
#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		yield return null;
		command = command.ToLowerInvariant();
		if (command == "colorblind") { colorblind = !colorblind; CbDisplay(); }
		else
		{
			string[] validCommands = new string[6] { "tl", "tr", "ml", "mr", "bl", "br" };
			TPCmds = command.Split(' ');
			for (int i = 0; i < TPCmds.Length; i++)
			{
				if (!validCommands.Contains(TPCmds[i]))
				{
					yield return "sendtochaterror @{0}, invalid command.";
					yield break;
				}
			}
			for (int i = 0; n < 1000 && i < TPCmds.Length; i++)
			{
				yield return null;
				while ((!((status == 0 && t2 == 4 && t % 50 <= 5) || status == 1 && t2 == targetT))) { yield return new WaitForSeconds(0.1f); }
                for (int j = 0; j < validCommands.Length; j++)
                {
					if (TPCmds[i] == validCommands[j]) { Button[j].OnInteract(); }
				}
				yield return new WaitForSeconds(1.1f);
			}
			yield return "strike";
			yield return "solve";
		}
		yield return null;
	}
	IEnumerator TwitchHandleForcedSolve()
	{
		status = -1;
		yield return null;
	}
}
