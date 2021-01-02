using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;

public class NegativityScript : MonoBehaviour
{
	public KMAudio Audio;
	public KMBombInfo Bomb;
	public KMBombModule Module;

	public AudioClip[] SFX;
	public KMSelectable[] Buttons;
	public Renderer[] ButtonRenderer;
	public Material[] BlackAndWhite;
	public TextMesh NumberLine;
	public TextMesh Ternary;
	public TextMesh[] SAndC;
	
	public SpriteRenderer Star;
	public Sprite[] Stars;

	private int[] Numbering = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
	private int[] NumberingConverted = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

	private int[][] TernaryFunctions = new int[][]{
		new int[9] {0, 0, 0, 0, 0, 0, 0, 0, 0},
	};
	
	bool Playable = false;
	bool Silent = false;

	private int Totale = 0;
	string Tables;
	
	int RotationsNumber = 0;
	private int[] Status = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
	private int KSop = 0;
	private int Switcher = 0;

	//Logging
	static int moduleIdCounter = 1;
	int moduleId;
	private bool ModuleSolved;

	void Awake()
	{
		moduleId = moduleIdCounter++;
		for (int i = 0; i < 3; i++)
		{
			int index = i;
			Buttons[index].OnInteract += delegate
			{
				PressButton(index);
				return false;
			};
		}
	}

	void Start()
	{
		Module.OnActivate += NumberSheet;
	}

	void NumberSheet()
	{
		for (int a = 0; a < 10; a++)
		{
			Status[a] = Random.Range(0,2);
			Numbering[a] = Random.Range(100,1000);
			if (Random.Range(0,2) != 0) Numbering[a] = -Numbering[a];
			if (Status[a] == 0) NumberingConverted[a] = -Numbering[a]; else NumberingConverted[a] = Numbering[a];
			Totale += NumberingConverted[a];
			Debug.LogFormat("[Negativity #{2}] Original Number : {0}, Converted Number: {1}", Numbering[a].ToString(), NumberingConverted[a].ToString(), moduleId);
		}
		Debug.LogFormat("[Negativity #{0}] The total value: {1}", moduleId, Totale.ToString());
		
		int n = Totale >= 0 ? Totale : -Totale;
		while (n > 0)
		{
		   int rem = n%3;
		   n = n/3;
		   if (rem == 2)
		   {
			   rem = -1;
			   n++;
		   }
		   Tables = Totale >= 0 ? (rem == 0 ? "0" : (rem==1) ? "+" : "-") + Tables : (rem == 0 ? "0" : (rem==1) ? "-" : "+") + Tables;
		}
		Debug.LogFormat("[Negativity #{0}] The balanced ternary generated: {1}", moduleId, Tables);
		Tables = Regex.Replace(Tables, "0", "");
		Debug.LogFormat("[Negativity #{0}] The answer is: {1}", moduleId, Tables);
		StartCoroutine(Rotations());
		Playable = true;
	}

	void PressButton(int index)
	{
		if (Playable)
		{
			Buttons[index].AddInteractionPunch(.2f);
			
			switch (index)
			{
				case 0:
					if (KSop == 1 && Ternary.text.Length < 9)
					{
						Audio.PlaySoundAtTransform(SFX[1].name, transform);
						Ternary.text += Switcher == 0 ? "-" : "+";
					}
					break;
				case 1:
					switch (KSop)
					{
						case 0:
							Audio.PlaySoundAtTransform(SFX[0].name, transform);
							StopAllCoroutines();
							StartCoroutine(Flashes());
							Star.sprite = null;
							KSop = 1;
							break;
						case 1:
							StopAllCoroutines();
							Debug.LogFormat("[Negativity #{0}] The submitted balance: {1}", moduleId, Ternary.text);
							StartCoroutine(MusicPlay());
							break;
						default:
							break;
					}
					break;
				case 2:
					switch (KSop)
					{
						case 1:
							switch (Ternary.text.Length)
							{
								case 0:
									StopAllCoroutines();
									KSop = 0;
									RotationsNumber = (RotationsNumber - 1 + 10) % 10;
									StartCoroutine(Rotations());
									break;
								default:
									Audio.PlaySoundAtTransform(SFX[0].name, transform);
									StartCoroutine(Clearer());
									break;
							}
							break;
						default:
							break;
					}
					break;
				default:
					break;
			}
		}
	}

	IEnumerator Rotations()
	{
		while (true)
		{
			for (int b = RotationsNumber; b < 10; b++)
			{	
				RotationsNumber = (RotationsNumber + 1) % 10;
				Star.sprite = b == 0 ? Status[b] == 1 ? Stars[0] : Stars[1] : null;
				ButtonRenderer[0].material = Status[b] == 0 ? BlackAndWhite[0] : BlackAndWhite[1];
				ButtonRenderer[1].material = Status[b] == 0 ? BlackAndWhite[1] : BlackAndWhite[0];
				NumberLine.color = Status[b] == 0 ? Color.white : Color.black;
				NumberLine.text = Numbering[b].ToString();
				yield return new WaitForSecondsRealtime(1f);
			}
		}
	}

	IEnumerator Flashes()
	{
		while (true)
		{
			NumberLine.text = "";
			for (int c = 0; c < 2; c++)
			{
				ButtonRenderer[0].material = c == 0 ? BlackAndWhite[0] : BlackAndWhite[1];
				ButtonRenderer[1].material = c == 0 ? BlackAndWhite[1] : BlackAndWhite[0];
				Switcher = c;
				yield return new WaitForSecondsRealtime(0.8f);
			}
		}
	}

	IEnumerator Clearer()
	{
		string Copper = Ternary.text;
		int Heal = Ternary.text.Length;
		for (int g = 0; g < Heal; g++)
		{
			Copper = Copper.Remove(Copper.Length - 1);
			Ternary.text = Copper;
			yield return new WaitForSecondsRealtime(0.05f);
		}
	}
	
	IEnumerator MusicPlay()
	{
		Playable = false;
		Debug.LogFormat("[Negativity #{0}] The balanced was disturbed. You are being judged", moduleId);
		string Answer = Ternary.text;
		Ternary.text = "";
		Audio.PlaySoundAtTransform(SFX[3].name, transform);
		float Switches = 0.2f;
		string[] Cycles = {"P", "N", "Po", "Ne", "Pos", "Neg", "Posi", "Nega", "Posit", "Negat", "Positi", "Negati", "Positiv", "Negativ", "Positivi", "Negativi", "Positivit", "Negativit", "Positivity", "Negativity", "The", "Balance", "Was", "Disturbed", "I", "Will", "Restore", "Balance", "I", "Will", "Provide", "Judgement", "My", "Final", "Decision", "Is"};
		for (int x = 0; x < 36; x++)
		{
			ButtonRenderer[0].material = x % 2 == 0 ? BlackAndWhite[0] : BlackAndWhite[1];
			ButtonRenderer[1].material = x % 2 == 0 ? BlackAndWhite[1] : BlackAndWhite[0];
			NumberLine.color = x % 2 == 0 ? Color.white : Color.black;
			NumberLine.text = Cycles[x];
			SAndC[0].text = x < 20 ? x % 2 == 0 ? "+" : "-" : x % 2 == 0 ? "O" : "X";
			SAndC[1].text = x < 20 ? x % 2 == 0 ? "+" : "-" : x % 2 == 0 ? "X" : "O";
			yield return new WaitForSecondsRealtime(Switches);
			Switches = x % 4 == 3 ? Switches * 0.8f : Switches;
		}
		
		SAndC[0].text = ""; SAndC[1].text = "";
		ButtonRenderer[0].material = Answer == Tables ? BlackAndWhite[1] : BlackAndWhite[0];
		ButtonRenderer[1].material = Answer == Tables ? BlackAndWhite[0] : BlackAndWhite[1];
		NumberLine.color = Answer == Tables ? Color.black : Color.white;
		NumberLine.text = Answer == Tables ? "Peace" : "Chaos";
		
		if (Answer == Tables)
		{
			Module.HandlePass();
			Audio.PlaySoundAtTransform(SFX[2].name, transform);
			Debug.LogFormat("[Negativity #{0}] The balanced was preserved. Module solved.", moduleId);
		}
		
		else
		{
			Tables = "";
			yield return new WaitForSecondsRealtime(0.5f);
			Module.HandleStrike();
			Debug.LogFormat("[Negativity #{0}] The balanced was destroyed. The balanced is being restored. A strike is given as a punishment.", moduleId);
			Playable = true; SAndC[0].text = "C"; SAndC[1].text = "S";
			Totale = KSop = RotationsNumber = 0;
			TernaryFunctions[0] = new int[] {0, 0, 0, 0, 0, 0, 0, 0, 0};
			NumberSheet();
		}
	}
	
	//twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} submit presses the submit button | !{0} clear presses the clear button | !{0} [- or +] delivers the answer to the module (This command can be performed in a chain)";
    #pragma warning restore 414
	
	string[] Validity = {"+", "-"};
	
	IEnumerator ProcessTwitchCommand(string command)
	{
		string[] parameters = command.Split(' ');
		if (Regex.IsMatch(command, @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			if (!Playable)
			{
				yield return "sendtochaterror Can not press the button since the button is not yet accessable.";
				yield break;
			}
			yield return "solve";
			yield return "strike";
			Buttons[1].OnInteract();
		}
		
		else if (Regex.IsMatch(command, @"^\s*clear\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
		{
			yield return null;
			if (!Playable)
			{
				yield return "sendtochaterror Can not press the button since the button is not yet accessable.";
				yield break;
			}
			
			if (KSop == 0)
			{
				yield return "sendtochaterror Can not clear text since the module is still cycling.";
				yield break;
			}
			
			Buttons[2].OnInteract();
		}
		
		else if (parameters[0].Contains('+') || parameters[0].Contains('-'))
		{
			yield return null;
			if (!Playable)
			{
				yield return "sendtochaterror Can not press the screen since the button is not yet accessable.";
				yield break;
			}
			
			if (KSop == 0)
			{
				yield return "sendtochaterror Can not toggle screen since the module is still cycling.";
				yield break;
			}
			
			foreach (char c in parameters[0])
			{
				if (!c.ToString().EqualsAny(Validity))
				{
					yield return "sendtochaterror The command being submitted contains a character that is not + or -";
					yield break;
				}
			}
			
			if (parameters[0].Length > 9 - Ternary.text.Length)
			{
				yield return "sendtochaterror The text being submitted will cause the display to go over 9 characters. Command was ignored.";
				yield break;
			}
			
			foreach (char c in parameters[0])
			{ 
				if (c.ToString() == "+")
				{
					while (Switcher != 1)
					{
						 yield return "trycancel The command to press the screen was halted due to a cancel request";
						 yield return new WaitForSeconds(0.01f);
					}
					Buttons[0].OnInteract();
					yield return new WaitForSeconds(0.1f);
				}
				
				else if (c.ToString() == "-")
				{
					while (Switcher != 0)
					{
						 yield return "trycancel The command to press the screen was halted due to a cancel request";
						 yield return new WaitForSeconds(0.01f);
					}
					Buttons[0].OnInteract();
					yield return new WaitForSeconds(0.1f);
				}
			}
		}
	}
}
