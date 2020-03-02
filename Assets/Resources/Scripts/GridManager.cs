using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class GridManager : MonoBehaviour
{

    public GameObject tile;
    public GameObject white;
    public GameObject black;
    public GameObject endGameText;
    public GameObject playerCapturesText;
    public GameObject IACapturesText;
    public GameObject IATimeText;
    public GameObject undoButton;

    public GameObject menuToggle;
    public GameObject menuStart;
    public GameObject menuDepth;
    public GameObject menuCaptures;

    public GameObject menuForbidden;
    public GameObject menuVersus;
    GameObject stoneToBePlaced;

    private GameObject[,] gridGameObject = new GameObject[19, 19];
    private byte[,] grid = (byte[,])TestGrid.emptyGrid.Clone();
    private List<StonePosition> stonePositions = new List<StonePosition>();
    private GameController gm = new GameController();

    private bool versusIA = true;

    private bool isPlayerOneTurn = true;

    private Dictionary<int, List<StonePosition>> capturedStones = new Dictionary<int, List<StonePosition>>();

    private GameObject lastStone;
    private Color shaderColor;
    private Color specularColor;

    private Color shaderWhiteColor;
    private Color shaderBlackColor;

    private int nbStone = 0;

    bool placeWhite = false;
    bool placeBlack = true;
    bool IAWin = false;
    bool playerWin = false;

    bool placing = false;
    bool IAThinking = false;

    bool randomIAFirstStone = false;

    bool gameStarted = false;

    void Start()
    {
        for (int x = 0; x < 19; x++)
        {
            for (int z = 0; z < 19; z++)
            {
                Vector3 tilePosition = new Vector3(x, 0, z);
                GameObject t = (GameObject)Instantiate(tile, tilePosition, Quaternion.identity);
                Tile tt = t.GetComponent<Tile>();
                tt.Row = x;
                tt.Col = z;
            }
        }
    }

    void Update()
    {
        if (!gameStarted) {
            return;
        }
        if (versusIA) {
            if (stonePositions.Count > 2) {
                if (undoButton.GetComponent<UnityEngine.UI.Button>().IsInteractable() == false)
                {
                    undoButton.GetComponent<UnityEngine.UI.Button>().interactable = true; 
                }
            } else {
                if (undoButton.GetComponent<UnityEngine.UI.Button>().IsInteractable() == true)
                {
                    undoButton.GetComponent<UnityEngine.UI.Button>().interactable = false; 
                }
        }
        }
        if ((placeWhite || placeBlack) && !playerWin && !IAWin)
        {
            DetectTile();
        } else if (playerWin) {
            endGameText.GetComponent<UnityEngine.UI.Text>().text = versusIA ? "PLAYER WIN" : "BLACK WIN";
            endGameText.gameObject.SetActive(true);
            ToggleCanvas(false);
        } else if (IAWin) {
            endGameText.GetComponent<UnityEngine.UI.Text>().text = versusIA ? "IA WIN" : "WHITE WIN";
            endGameText.gameObject.SetActive(true);
            ToggleCanvas(false);
        }

    }

    private void SetStoneNumber(GameObject stone) {
        nbStone++;
        stone.transform.GetChild(0).GetComponent<TextMesh>().text = nbStone.ToString();
    }

    void DetectTile()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000))
        {
            if (hit.collider.gameObject.tag == "tile")
            {
                Vector3 pos = hit.collider.gameObject.transform.position;
                Tile t = hit.collider.gameObject.GetComponent<Tile>();
                if (Input.GetMouseButtonDown(0) && !placing && !EventSystem.current.IsPointerOverGameObject())
                {
                    stoneToBePlaced = (GameObject)Instantiate(placeBlack ? black : white, pos, Quaternion.identity);
                    placing = true;
                }
                else if (Input.GetMouseButtonDown(0) && placing && !EventSystem.current.IsPointerOverGameObject())
                {
                    if (grid[t.Row, t.Col] == 0)
                    {
                        byte playerId = placeWhite ? (byte)2 : (byte)1;

                        grid[t.Row, t.Col] = playerId;
                        StonePosition newStone = new StonePosition() { row = (sbyte)t.Row, col = (sbyte)t.Col, playerId = playerId, position = nbStone + 1 };
                        
                        int[] res = Node.isForbidden(grid, newStone, playerId);
                        if (res[0] == 1)
                        {
                            grid[t.Row, t.Col] = 0;
                            return;
                        }

                        gridGameObject[t.Row, t.Col] = stoneToBePlaced;
                        placeBlack = false;
                        placeWhite = false;
                        stonePositions.Add(newStone);
                        RemoveLastPlacedMarker();
                        SetStoneNumber(stoneToBePlaced);
                        checkEndGame(playerId);
                        RemoveStones(Node.checkCapture(newStone, grid), playerId);
                        
                        if (versusIA) {
                            DateTime beforeIA = DateTime.Now;
                            IAThinking = true;
                            List<StonePosition> withoutCaptured = new List<StonePosition>();
                            foreach (StonePosition s in stonePositions) {
                                if (s.captured == false) {
                                    withoutCaptured.Add(s);
                                }
                            }
                            Node result = gm.StartGomoku(this.grid, withoutCaptured);

                            placeStone(result.positions[result.positions.Count - 1]);
                            DateTime afterIA = DateTime.Now;
                            TimeSpan span = afterIA - beforeIA;
                            int ms = (int)span.TotalMilliseconds;
                            IATimeText.GetComponent<UnityEngine.UI.Text>().text = "IA time: " + ms + "ms";
                            
                            placeBlack = true;
                            IAThinking = false;
                        }
                        placing = false;
                        stoneToBePlaced = null;
                        if (!versusIA) {
                            isPlayerOneTurn = !isPlayerOneTurn;
                            placeBlack = isPlayerOneTurn ? false : true;
                            placeWhite = isPlayerOneTurn ? true : false;
                        }
                    }
                }
                else if (placing)
                {
                    if (stoneToBePlaced != null)
                    {
                        stoneToBePlaced.transform.position = pos;
                    }
                }
            }
        }
    }

    void placeStone(StonePosition stone)
    {
        Vector3 pos = new Vector3(stone.row, 0, stone.col);
        GameObject newStone = (GameObject)Instantiate(white, pos, Quaternion.identity);
        gridGameObject[stone.row, stone.col] = newStone;
        grid[stone.row, stone.col] = 2;
        stone.position = nbStone + 1;
        stonePositions.Add(stone);
        SetStoneNumber(newStone);
        MarkLastIAPlaced(newStone);
        RemoveStones(Node.checkCapture(stone, grid), 2);
        checkEndGame(2);
    }

    void checkEndGame(int playerId) {
        List<ushort> patterns = Node.findPatterns(new Node(stonePositions, grid, null, 0, GameController.nbCapturePlayer, GameController.nbCaptureIA), 1);
        foreach (ushort pattern in patterns)
        {
            if (Node.applyHeuritic(pattern, playerId) == Utils.VICTORY) {
                IAWin = true;
            } else if (Node.applyHeuritic(pattern, playerId) == Utils.DEFEAT) {
                playerWin = true;
            }
        }
    }

    void RemoveStones(List<StonePosition> stones, int playerId)
    {
        int count = 0;
        List<StonePosition> cs = new List<StonePosition>();
        foreach (StonePosition stone in stones)
        {
            grid[stone.row, stone.col] = 0;
            Destroy(gridGameObject[stone.row, stone.col]);
            StonePosition s = stone;
            s.position = int.Parse(gridGameObject[stone.row, stone.col].transform.GetChild(0).GetComponent<TextMesh>().text);
            s.captured = true;
            cs.Add(s);

            stonePositions[s.position -1] = s;
            count++;
        }
        if (cs.Count > 0) {
            capturedStones[nbStone] = cs;
        }
        int nbCaptures = count / 2;
        if (nbCaptures > 0) {
            if (playerId == 1) {
                GameController.nbCapturePlayer += nbCaptures;
                playerCapturesText.GetComponent<UnityEngine.UI.Text>().text = (versusIA ? "Player captures: " : "Black captures: ") + GameController.nbCapturePlayer;
                if (GameController.nbCapturePlayer > 4) {
                    playerWin = true;
                }
            } else {
                GameController.nbCaptureIA += nbCaptures;
                IACapturesText.GetComponent<UnityEngine.UI.Text>().text = (versusIA ? "IA captures: " : "White captures: ") + GameController.nbCaptureIA;
                if (GameController.nbCaptureIA > 4) {
                    IAWin = true;
                }
            }
        }
    }

    private void MarkLastIAPlaced(GameObject stone) {
        if (stone != null) {
            lastStone = stone;
            stone.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);
        }
        
    }

    private void RemoveLastPlacedMarker() {
        if (lastStone != null) {
            lastStone.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
        }
    }

    public void UndoLastMove() {
        if (!IAThinking && stonePositions.Count > 2) {
            List<StonePosition> stonesToRemove = new List<StonePosition>();
            stonesToRemove.Add(stonePositions[stonePositions.Count - 1]);
            stonesToRemove.Add(stonePositions[stonePositions.Count - 2]);
            foreach (StonePosition stone in stonesToRemove)
            {
                grid[stone.row, stone.col] = 0;
                Destroy(gridGameObject[stone.row, stone.col]);
                stonePositions.Remove(stone);
                List<StonePosition> cStones = capturedStones.ContainsKey(stone.position) ? capturedStones[stone.position] : new List<StonePosition>();
                foreach(StonePosition c in cStones) {
                    grid[c.row, c.col] = c.playerId;
                    Vector3 pos = new Vector3(c.row, 0, c.col);
                    GameObject st = (GameObject)Instantiate(c.playerId == 1 ? black : white, pos, Quaternion.identity);

                    st.transform.GetChild(0).GetComponent<TextMesh>().text = c.position.ToString();
                    gridGameObject[c.row, c.col] = st;
                    StonePosition cc = c;
                    cc.captured = false;
                    stonePositions[c.position - 1] = cc;
                }
                if (cStones.Count > 0) {
                    int nbCap = cStones.Count / 2;
                    if (cStones[0].playerId == 1) {
                        GameController.nbCaptureIA -= nbCap;
                        IACapturesText.GetComponent<UnityEngine.UI.Text>().text = (versusIA ? "IA captures: " : "White captures: ") + GameController.nbCaptureIA;
                    } else {
                        GameController.nbCapturePlayer -= nbCap;
                        playerCapturesText.GetComponent<UnityEngine.UI.Text>().text = (versusIA ? "Player captures: " : "Black captures: ") + GameController.nbCapturePlayer;
                    }
                    capturedStones.Remove(stone.position);
                }
            }

            StonePosition lastStone = stonePositions[stonePositions.Count - 1];
            MarkLastIAPlaced(gridGameObject[lastStone.row, lastStone.col]);
            endGameText.gameObject.SetActive(false);
            IAWin = false;
            playerWin = false;
            gameStarted = true;
            nbStone -= 2;
        }
    }

    public void PlaceWhite()
    {
        GameController.enlabledDebug = true;
        placeWhite = true;
    }

    public void PlaceBlack()
    {
        placeBlack = true;
    }

    public void SetDepth(UnityEngine.UI.Dropdown dropdown) {
        GameController.DEPTH = dropdown.value + 1;
    }

    public void SetVersus(UnityEngine.UI.Dropdown dropdown) {
        if (dropdown.value == 0) {
            versusIA = true;
        } else {
            versusIA = false;
        }
    }

    public void SetIARandomFirstStone(UnityEngine.UI.Toggle toggle) {
        this.randomIAFirstStone = toggle.isOn;
    }

    public void StartNewGame() {
        menuStart.GetComponentInChildren<UnityEngine.UI.Text>().text = "Restart";
        ToggleCanvas(true);
        if (versusIA) {
            Vector3 pos = new Vector3(9, 0, 9);
            if (randomIAFirstStone) {
                pos = new Vector3((int)UnityEngine.Random.Range(5, 10), 0, (int)UnityEngine.Random.Range(5, 10));
            }
            GameObject firstStone = (GameObject)Instantiate(white, pos, Quaternion.identity);
            gridGameObject[(int)pos.x, (int)pos.z] = firstStone;
            grid[(int)pos.x, (int)pos.z] = (byte)2;
            SetStoneNumber(firstStone);
            stonePositions.Add(new StonePosition() { row = (sbyte)pos.x, col = (sbyte)pos.z, playerId = 2, position = nbStone });
            MarkLastIAPlaced(firstStone);
        } else {
            isPlayerOneTurn = true;
            placeBlack = false;
            placeWhite = true;
        }

    }

    private void ToggleCanvas(bool gameStarted) {
        IATimeText.SetActive(versusIA);
        menuVersus.SetActive(!gameStarted);
        menuToggle.SetActive(!gameStarted);
        undoButton.SetActive(versusIA);
        menuDepth.SetActive(!gameStarted);
        menuCaptures.SetActive(!gameStarted);
        menuForbidden.SetActive(!gameStarted);
        endGameText.gameObject.SetActive(!gameStarted);
        this.gameStarted = gameStarted;
        if (gameStarted) {
            ResetState();
        }
    }

    private void ResetState() {
        grid = (byte[,])TestGrid.emptyGrid.Clone();
        foreach(StonePosition s in stonePositions) {
            Destroy(gridGameObject[s.row, s.col]);
        }
        stonePositions.Clear();
        capturedStones.Clear();
        GameController.nbCaptureIA = 0;
        GameController.nbCapturePlayer = 0;
        playerCapturesText.GetComponent<UnityEngine.UI.Text>().text = (versusIA ? "Player captures: " : "Black captures: ") + 0;
        IACapturesText.GetComponent<UnityEngine.UI.Text>().text = (versusIA ? "IA captures: " : "White captures: ") + 0;
        nbStone = 0;
        IAWin = false;
        playerWin = false;
    }

    public void EnableCaptures(UnityEngine.UI.Toggle toggle) {
        GameController.captureEnabled = toggle.isOn;
    }

    public void EnableDoubleFreeThree(UnityEngine.UI.Toggle toggle) {
        GameController.doubleFreeThreeEnabled = toggle.isOn;
    }
}
