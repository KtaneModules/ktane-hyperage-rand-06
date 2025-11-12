using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class hyperageScript : MonoBehaviour {

	public KMAudio Audio;
	public AudioClip[] audioClips;
	public KMSelectable[] buttons;
	public MeshRenderer[] buttonsRenderer;
	public TextMesh[] buttonTexts;
	public TextMesh[] coordinateTexts;
	public TextMesh centerText;
	public GameObject[] orbs;
	
	public bool ModuleSolved;
	private static int ModuleIdCounter = 1;
	private int ModuleId;

	private readonly string[] rotations = { "XY", "XZ", "XW", "YX", "YZ", "YW", "ZX", "ZY", "ZW", "WX", "WY", "WZ" };
	private bool rotationIsInProgress;
	private string currentRotation = "";
	private int currentButton = -1;
	private string[] buttonRotations;
	private const string base36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	private int[][] buttonCoordinates;
	private int[] buttonUnknownCoordinateIndices;
	private string[][] coordinatesStrings;
	private bool submissionMode;
	private string answerString = "";
	private bool animationIsInProgress;
	
	private static Vector3 coordinateFromIndex(int index)
	{
		float x = (index & 1) == 1?1f:-1f;
		float y = (index & 2) == 2?1f:-1f;
		float z = (index & 4) == 4?1f:-1f;
		return new Vector3(x, y, z) * ((index & 8) == 8? 1f : .5f);
	}

	private int getIndexAfterRotation(int initialIndex, string rotation)
	{
		bool[] axes = new bool[4]; //[X, Y, Z, W]-ordered.
		for (int i = 0; i < axes.Length; i++)
		{
			axes[i] = initialIndex % 2 == 1;
			initialIndex /= 2;
		}
		int axis2 = "XYZW".IndexOf(rotation[0]);
		int axis1 = "XYZW".IndexOf(rotation[1]);
		bool bool1 = axes[axis1];
		bool bool2 = axes[axis2];
		axes[axis1] = bool2;
		axes[axis2] = !bool1;
		int ans = 0;
		for (int i = axes.Length; i > 0 ; i--)
		{
			ans *= 2;
			ans += axes[i-1]? 1 : 0;
		}

		return ans;
	}
	
	string divideBy1000(int num)
	{
		return Math.Abs(num).ToString("D4").Substring(0, 1)+"."+Math.Abs(num).ToString("D4").Substring(1);
	}
	
	void returnToOriginalPositions() { for (int i = 0; i < orbs.Length; i++) orbs[i].transform.localPosition = coordinateFromIndex(i); }
	
	private IEnumerator rotate()
	{
		while (!ModuleSolved && !submissionMode)
		{
			rotationIsInProgress = true;
			if (currentButton != -1)
			{
				centerText.text = base36[currentButton].ToString();
				for (int i = 0; i < 4; i++)
				{
					coordinateTexts[i].text = coordinatesStrings[currentButton][i];
				}
			}
			else
			{
				centerText.text = "";
				for (int i = 0; i < 4; i++)
				{
					coordinateTexts[i].text = "";
				}
			}

			if (currentRotation != "")
			{
				Vector3[] initialPositions = new Vector3[orbs.Length];
				Vector3[] positionsAfterRotation = new Vector3[orbs.Length];
				for (int i = 0; i < orbs.Length; i++)
				{
					initialPositions[i] = coordinateFromIndex(i);
					positionsAfterRotation[i] = coordinateFromIndex(getIndexAfterRotation(i, currentRotation));
				}

				float timePassed = 0f;
				while (timePassed < .5f)
				{
					timePassed += .05f;
					yield return new WaitForSeconds(.05f);
					for (int i = 0; i < orbs.Length; i++)
						orbs[i].transform.localPosition =
							Vector3.Lerp(initialPositions[i], positionsAfterRotation[i], timePassed/.5f);
				}

				returnToOriginalPositions();
			}
			yield return new WaitForSeconds(.5f);
			rotationIsInProgress = false;
		}
	}

	void buttonInit()
	{
		for (int ii = 0; ii < buttons.Length; ii++)
		{
			int i = ii;
			int x = i%6, y=i/6;
			float length = (float)Math.Sqrt((x - 2.5) * (x - 2.5) + (y - 2.5) * (y - 2.5));
			buttonsRenderer[i].material.color = Color.Lerp(new Color(0f,0f,0.7f), new Color(0f,0f,0.2f), length/(float)Math.Sqrt(12.5));
			buttonTexts[i].text = "";
			buttons[i].OnHighlight += delegate
			{
				if (!submissionMode)
				{
					currentRotation = buttonRotations[i];
					currentButton = i;
				}
			};
			buttons[i].OnHighlightEnded += delegate
			{
				if (!submissionMode)
				{
					currentButton = -1;
					currentRotation = "";
				}
			};
			buttons[i].OnInteract += delegate
			{
				if (ModuleSolved || animationIsInProgress) return false;
				pressButton(i);
				return false;
			};
		}
	}

	IEnumerator stageAnimation()
	{
		animationIsInProgress = true;
		while (rotationIsInProgress) yield return new WaitForSeconds(.1f);
		centerText.text = "";
		for (int i = 0; i < 4; i++) coordinateTexts[i].text = "";
		int timePassed = 0;
		while (timePassed < 21)
		{
			for (int i = 0; i < orbs.Length; i++)
			{
				orbs[i].transform.localPosition = Vector3.Lerp(coordinateFromIndex(i), new Vector3(0, -1f, 0),timePassed/20f);
			}
			yield return new WaitForSeconds(.05f);
			timePassed += 1;
		}

		centerText.color = Color.clear;
		centerText.text = "0";
		for (int i = 0; i < 36; i++) buttonTexts[i].color = Color.clear;
		drawButtonsOnY(0);
		timePassed = 0;
		while (timePassed < 21)
		{
			foreach (var c in buttonTexts) 
				c.color = Color.Lerp(new Color(1,1,1,0), new Color(1,1,1,.5f), timePassed/20f);
			centerText.color = Color.Lerp(new Color(1,1,1,0), new Color(1,1,1,.75f), timePassed/20f);
			yield return new WaitForSeconds(.05f);
			timePassed += 1;
		}
		animationIsInProgress = false;
	}

	int mod(int x, int m)
	{
		return (x % m + m) % m;
	}
	
	void drawButtonsOnY(int y)
	{
		for (int i = 0; i < 6; i++) buttonTexts[i + 6*y].text             = "012345"[i].ToString();
		for (int i = 0; i < 6; i++) buttonTexts[i + mod(6*(y+1),36)].text = "SUBMIT"[i].ToString();
		for (int i = 0; i < 6; i++) buttonTexts[i + mod(6*(y-1),36)].text = "RESET."[i].ToString();
		for (int i = 0; i < 6; i++) buttonTexts[i + mod(6*(y+2),36)].text = "";
		for (int i = 0; i < 6; i++) buttonTexts[i + mod(6*(y+3),36)].text = "";
		for (int i = 0; i < 6; i++) buttonTexts[i + mod(6*(y+4),36)].text = "";
	}

	private IEnumerator rotateInCircles()
	{
		int timePassed = 0;
		while (true)
		{
			for (int i = 0; i < orbs.Length; i++)
			{
				if (i * 22.5f < timePassed && timePassed < i * 22.5f + 360)
					orbs[i].transform.localPosition =
						Rotation.getCoordinate(
							(timePassed - i * 22.5f) / 360f, (timePassed - i * 22.5f) / 360f * 2.5f, -1f
						);
				else if (timePassed >= i * 22.5f + 360) 
					orbs[i].transform.localPosition = 
						Rotation.getCoordinate(
							(timePassed - i * 22.5f) / 360f, 2.5f, -1f
						);
			}
			yield return new WaitForSeconds(1f/40);
			timePassed += 3;
			if (timePassed > 360*3) timePassed -= 360;
		}
	}

	private IEnumerator solve()
	{
		ModuleSolved = true;
		
		int timePassed = 0;
		while (timePassed < 21)
		{
			foreach (var c in buttonTexts) 
				c.color =      Color.Lerp(new Color(1,1,1,.50f), new Color(1,1,1,0), timePassed/20f);
			centerText.color = Color.Lerp(new Color(1,1,1,.75f), new Color(1,1,1,0), timePassed/20f);
			yield return new WaitForSeconds(.05f);
			timePassed += 1;
		}
		centerText.text = "";
		centerText.color = new Color(1, 1, 1, .75f);
		foreach (var c in buttonTexts)
		{
			c.text = "";
			c.color = new Color(1, 1, 1, .50f);
		}
		
		for (int i = 0; i < 36; i++)
		{
			buttonTexts[i].text = "MODULE   FORCELTICHYPER AGE   SOLVED"[i].ToString();
			if ("MODULE   FORCELTICHYPER AGE   SOLVED"[i]!=' ') Audio.HandlePlaySoundAtTransform(audioClips[1].name, transform);
			yield return new WaitForSeconds(.1f);
		}
		
		yield return new WaitForSeconds(.5f);
		GetComponent<KMBombModule>().HandlePass();
		Audio.HandlePlaySoundAtTransform(audioClips[2].name, transform);
		StartCoroutine(rotateInCircles());
	}

	private IEnumerator strike()
	{
		yield return new WaitForSeconds(.1f);
		
		animationIsInProgress = true;
		int timePassed = 0;
		while (timePassed < 21)
		{
			foreach (var c in buttonTexts) 
				c.color =      Color.Lerp(new Color(1,1,1,.50f), new Color(1,1,1,0), timePassed/20f);
			centerText.color = Color.Lerp(new Color(1,1,1,.75f), new Color(1,1,1,0), timePassed/20f);
			yield return new WaitForSeconds(.05f);
			timePassed += 1;
		}
		centerText.text = "";
		centerText.color = new Color(1, 1, 1, .75f);
		foreach (var c in buttonTexts) c.text = "";
		drawButtonsOnY(0);
		timePassed = 0;
		while (timePassed < 21)
		{
			for (int i = 0; i < orbs.Length; i++)
			{
				orbs[i].transform.localPosition = Vector3.Lerp(new Vector3(0, -1f, 0), coordinateFromIndex(i),timePassed/20f);
			}
			yield return new WaitForSeconds(.05f);
			timePassed += 1;
		}
		animationIsInProgress = false;
		GetComponent<KMBombModule>().HandleStrike();
		submissionMode = false;
		currentButton = -1;
		currentRotation = "";
		StartCoroutine(rotate());
	}
	
	void pressButton(int index)
	{
		Audio.HandlePlaySoundAtTransform(audioClips[0].name, transform);
		if (!submissionMode)
		{
			submissionMode = true;
			currentRotation = "";
			currentButton = 35;
			StartCoroutine(stageAnimation());
			return;
		}
		if (animationIsInProgress) return;
		int yPrevious = currentButton / 6;
		int yCurrent = index / 6;
		if (yPrevious == yCurrent)
		{
			if (index % 6 == 5) centerText.text += ".";
			else
			{
				centerText.text = "0";
				drawButtonsOnY(0);
				currentButton = 35;
			}
		}
		else if (mod(yCurrent - yPrevious,6) == 2)
		{
			if (centerText.text == answerString) StartCoroutine(solve());
			else StartCoroutine(strike());
		}
		else if (mod(yCurrent - yPrevious,6) == 1)
		{
			currentButton = index;
			if (centerText.text == "0") centerText.text = (index%6).ToString();
			else centerText.text += (index%6).ToString();
			drawButtonsOnY(mod(yCurrent+1,6));
		}
	}
	
	void generateButtonRotations()
	{
		// Whoever is reading this, I AM GENUINELY SORRY FOR THIS PIECE OF CODE.
		// I don't know how to make the config, and I don't really want to spend way too much time on that,
		// so we have the bruteforce here. If you're not lucky, this function may lag, but I just hope that it won't.
		
		buttonRotations = new string[buttons.Length];
		while (SquaresChecker.squares(buttonRotations) != 1)
			for (int i = 0; i < buttons.Length; i++) 
				buttonRotations[i] = rotations[Random.Range(0, rotations.Length)];
		Debug.LogFormat("[Hyperage #{0}] Center is {1}. Corners: {2}", ModuleId, base36[buttonRotations.getSquare()[0]],
			buttonRotations.getSquare().ToList().GetRange(1,4).ConvertAll(a => base36[a]).Join(", ")
			);
	}

	void generateButtonCoordinates()
	{
		buttonCoordinates = new int[buttons.Length][];
		buttonUnknownCoordinateIndices = new int[buttons.Length];
		coordinatesStrings = new string[buttons.Length][];
		for (int i = 0; i < buttons.Length; i++)
		{
			buttonCoordinates[i] = new int[4];
			coordinatesStrings[i] = new string[4];
			for (int j = 0; j < 4; j++)
			{
				buttonCoordinates[i][j] = Random.Range(1, 10000);
				coordinatesStrings[i][j] = divideBy1000(buttonCoordinates[i][j]);
			}

			buttonUnknownCoordinateIndices[i] = Random.Range(0, 4);
			coordinatesStrings[i][buttonUnknownCoordinateIndices[i]] = "?.???";
		}
	}

	void getAns()
	{
		int[] square = buttonRotations.getSquare();
		int[] matrix = new int[16];
		for (int i = 0; i < 4; i++)
		{
			for (int j = 0; j < 4; j++)
				if (buttonUnknownCoordinateIndices[square[i + 1]] == j)
				{
					if (buttonUnknownCoordinateIndices[square[0]] == i)
					{
						matrix[i * 4 + j] = 1000;
					}
					else matrix[i * 4 + j] = buttonCoordinates[square[0]][i];
				}
				else matrix[i * 4 + j] = buttonCoordinates[square[i + 1]][j];
		}
		Debug.LogFormat("[Hyperage #{0}] Matrix: \n[Hyperage #{0}] {1}\t{2}\t{3}\t{4}\n[Hyperage #{0}] {5}\t{6}\t{7}\t{8}\n[Hyperage #{0}] {9}\t{10}\t{11}\t{12}\n[Hyperage #{0}] {13}\t{14}\t{15}\t{16}", ModuleId,
			divideBy1000(matrix[ 0]), 
			divideBy1000(matrix[ 1]), 
			divideBy1000(matrix[ 2]), 
			divideBy1000(matrix[ 3]), 
			divideBy1000(matrix[ 4]), 
			divideBy1000(matrix[ 5]), 
			divideBy1000(matrix[ 6]),  
			divideBy1000(matrix[ 7]),  
			divideBy1000(matrix[ 8]),
			divideBy1000(matrix[ 9]), 
			divideBy1000(matrix[10]),
			divideBy1000(matrix[11]), 
			divideBy1000(matrix[12]), 
			divideBy1000(matrix[13]), 
			divideBy1000(matrix[14]), 
			divideBy1000(matrix[15])
			);
		answerString = matrix.det().getBase6Determinant();
		Debug.LogFormat("[Hyperage #{0}] Answer is: {1}",ModuleId,answerString);
	}
	
	
	void Start () {
		ModuleId = ModuleIdCounter++;
		generateButtonRotations();
		generateButtonCoordinates();
		buttonInit();
		foreach (var t in orbs)
			t.GetComponent<MeshRenderer>().material.color = new Color (.3f,.5f,.7f);

		StartCoroutine(rotate());
		getAns();
	}

}
