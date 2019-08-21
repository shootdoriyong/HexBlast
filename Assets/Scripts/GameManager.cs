using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager> {

	Camera mainCamera;

	//Objects.
	Transform 	cellRoot;
	Text 		txt_Comment;
	Text 		txt_SuffleTimer;
	GameObject	btn_Restart;

	Text 		txt_Mission;
	Text 		txt_Move;
	Text 		txt_Score;

	GameObject cellPrefab;
	GameObject blockPrefab;

	List<Cell> cells = new List<Cell>();
	List<Cell> list_BurstCell = new List<Cell> ();
	List<Cell> list_LoopCell = new List<Cell>();
	List<Cell> list_DealDamageCell = new List<Cell>();

	float supplyTime = 0.05f;
	float moveTime = 0.05f;
	float burstDelayTime = 0.25f;

	int specialBlockCount = 0;
	public bool touchEnable = true;

	int missionCount;
	int moveCount;
	int score;

	int mapRow = 0;
	int mapHeight = 0;

	GameState gameState = GameState.READY;

	void Start ()
	{
		cellRoot = GameObject.Find ("Canvas/CellRoot").transform;
		txt_Comment = GameObject.Find ("Canvas/TXT_Comment").GetComponent<Text> ();
		txt_SuffleTimer = GameObject.Find ("Canvas/TXT_SuffleTimer").GetComponent<Text> ();
		txt_SuffleTimer.gameObject.SetActive (false);
		btn_Restart = GameObject.Find ("Canvas/BTN_Restart").gameObject;
		btn_Restart.SetActive (false);

		txt_Mission = GameObject.Find ("Canvas/TXT_Mission").GetComponent<Text> ();
		txt_Move = GameObject.Find ("Canvas/TXT_Move").GetComponent<Text> ();
		txt_Score = GameObject.Find ("Canvas/TXT_Score").GetComponent<Text> ();

		score = 0;
		missionCount = IngameDefine.specialBlockMissionCount;
		moveCount = IngameDefine.maximumMoveCount;

		refreshScoreUI ();
		refreshMissionCountUI ();
		refreshMoveCountUI ();

		cellPrefab = Resources.Load<GameObject>("Prefabs/Cell");
		blockPrefab = Resources.Load<GameObject>("Prefabs/Block");

		mapHeight = IngameDefine.mapHeight;
		mapRow = IngameDefine.mapRow;

		InitializeBoard ();

		gameState = GameState.PLAY;
	}

	//보드 초기화.
	void InitializeBoard ()
	{
		Debug.Log ("### InitializeBoard");

		txt_Comment.text = "보드를 생성합니다.";

		int leftRow = mapRow / 2;
		for(int i = leftRow; i<mapHeight; i++)
		{
			int startLeft = (mapHeight - i) * -1;
			for (int j = 0; j < i; j++) {
				Cell cell = GameObject.Instantiate (cellPrefab, cellRoot).GetComponent<Cell>();
				cell.transform.name = string.Format ("Cell( {0}, {1}, {2} )", startLeft, (mapHeight - i) + j, j);
				cell.SetCoordinate (new Vector3(startLeft, (mapHeight - i) + j, j));
				BlockType getBlockType = GetRandomBlockType ();
				cell.InitBlockHP (getBlockType);
				cell.SetBlockType (getBlockType);
				cells.Add (cell);
			}
		}

		for (int i = 0; i < mapHeight; i++) {
			Cell cell = GameObject.Instantiate (cellPrefab, cellRoot).GetComponent<Cell>();
			cell.transform.name = string.Format ("Cell( {0}, {1}, {2} )", 0, i, i);
			cell.SetCoordinate (new Vector3(0, i, i));
			BlockType getBlockType = GetRandomBlockType ();
			cell.InitBlockHP (getBlockType);
			cell.SetBlockType (getBlockType);
			cells.Add (cell);
		}

		int rightRow = mapRow / 2;
		for(int i = mapHeight-1; i>=rightRow; i--)
		{
			int startRight = mapHeight - i;
			for (int j = 0; j < i; j++) {
				Cell cell = GameObject.Instantiate (cellPrefab, cellRoot).GetComponent<Cell>();
				cell.transform.name = string.Format ("Cell( {0}, {1}, {2} )", startRight, j, (mapHeight - i) + j);
				cell.SetCoordinate (new Vector3(startRight, j, (mapHeight - i) + j));
				BlockType getBlockType = GetRandomBlockType ();

				if (startRight == 3 && j < 2)
					getBlockType = BlockType.ORANGE;

				if (startRight == 2 && j == 3)
					getBlockType = BlockType.ORANGE;

				cell.InitBlockHP (getBlockType);
				cell.SetBlockType (getBlockType);
				cells.Add (cell);
			}
		}

		if (ExistMatchingBlock () == true) {
			txt_Comment.text = "이미 매칭중인 블록이 \n 존재하여 맵을 다시 셔플합니다.";
			ShuffleBoard (mapHeight, mapRow);
		}
		else if (CheckPossibleBoard () == false) {
			ShuffleBoard (mapHeight, mapRow);
		}
	}

	//게임 플레이가 가능한 보드인지 검사합니다.
	public bool CheckPossibleBoard ()
	{
		foreach (Cell cell in cells) {
			if (cell.blockType == BlockType.TOP)
				continue;
			if (CheckPossibleMatchingCell (cell))
				return true;
		}
		Debug.Log (" ----------- IMPOSSIBLE BOARD ----------------");
		txt_Comment.text = "매칭 가능한 블록이 \n 존재하지 않아 맵을 다시 셔플합니다.";
		return false;
	}

	bool CheckPossibleMatchingCell (Cell cell)
	{
		Cell checkCell = GetTwentyDirectionCell (cell);
		if(CheckMatchingSwapBlockType(cell, checkCell))
			return true;

		checkCell = GetTwoDirectionCell (cell);
		if(CheckMatchingSwapBlockType(cell, checkCell))
			return true;

		checkCell = GetFourDirectionCell (cell);
		if(CheckMatchingSwapBlockType(cell, checkCell))
			return true;

		checkCell = GetSixDirectionCell (cell);
		if(CheckMatchingSwapBlockType(cell, checkCell))
			return true;

		checkCell = GetEightDirectionCell (cell);
		if(CheckMatchingSwapBlockType(cell, checkCell))
			return true;

		checkCell = GetTenDirectionCell (cell);
		if(CheckMatchingSwapBlockType(cell, checkCell))
			return true;

		return false;
	}

	bool CheckMatchingSwapBlockType (Cell srcCell, Cell checkCell)
	{
		bool matchingEnable = false;

		if ((checkCell != null) && (checkCell.blockType != srcCell.blockType)) {
			SwapBlockType (srcCell, checkCell);
			if (CheckMatchingBlockAxisX (checkCell) || CheckMatchingBlockAxisY (checkCell) || CheckMatchingBlockAxisZ (checkCell)) {
				matchingEnable = true;
			}
			SwapBlockType (srcCell, checkCell);
		}
		return matchingEnable;
	}

	//보드에 이미 매칭된 블록들이 있는지 판단합니다.
	bool ExistMatchingBlock ()
	{
		foreach (Cell cell in cells) {
			if (cell.blockType == BlockType.NONE || cell.blockType == BlockType.TOP)
				continue;
			if (CheckMatchingBlockAxisX (cell)) {
				Debug.Log ("***** Axis X Exist Block ");
				return true;
			}
			if(CheckMatchingBlockAxisY(cell)) {
				Debug.Log ("***** Axis Y Exist Block ");
				return true;	
			}
			if (CheckMatchingBlockAxisZ (cell)) {
				Debug.Log ("***** Axis Z Exist Block ");
				return true;
			}
		}
		return false;
	}

	//X축으로 매칭된 블록이 있는지 검사합니다.
	bool CheckMatchingBlockAxisX (Cell cell)
	{
		List<Cell> list_EqualAxisXCell = new List<Cell> ();
		foreach (Cell c in cells) {
			if (cell.posIdx.x == c.posIdx.x)
				list_EqualAxisXCell.Add (c);
		}
		list_EqualAxisXCell.Sort ((Cell cellOne, Cell cellTwo) => cellOne.posIdx.y.CompareTo (cellTwo.posIdx.y));

		BlockType baseBlockType = list_EqualAxisXCell[0].blockType;
		int matchBlockCount = 1;

		for (int i = 1; i < list_EqualAxisXCell.Count; i++) {
			if ((baseBlockType == list_EqualAxisXCell [i].blockType) && (baseBlockType > BlockType.NONE) && (baseBlockType < BlockType.TOP)) {
				matchBlockCount += 1;
				if (matchBlockCount >= 3) {
					//Debug.Log ("### Matching Possible X ,,, Cell Idx = " + cell.posIdx + " ,,, BlockType = " + cell.blockType);
					return true;
				}
			}
			else {
				matchBlockCount = 1;
				baseBlockType = list_EqualAxisXCell [i].blockType;
			}
		}
		return false;
	}

	//Y축으로 매칭된 블록이 있는지 검사합니다.
	bool CheckMatchingBlockAxisY (Cell cell)
	{
		List<Cell> list_EqualAxisYCell = new List<Cell> ();
		foreach (Cell c in cells) {
			if (cell.posIdx.y == c.posIdx.y)
				list_EqualAxisYCell.Add (c);
		}
		list_EqualAxisYCell.Sort ((Cell cellOne, Cell cellTwo) => cellOne.posIdx.x.CompareTo (cellTwo.posIdx.x));

		BlockType baseBlockType = list_EqualAxisYCell[0].blockType;
		int matchBlockCount = 1;

		for (int i = 1; i < list_EqualAxisYCell.Count; i++) {
			if ((baseBlockType == list_EqualAxisYCell [i].blockType) && (baseBlockType != BlockType.NONE) && (baseBlockType < BlockType.TOP)) {
				matchBlockCount += 1;
				if (matchBlockCount >= 3) {
					//Debug.Log ("### Matching Possible X ,,, Cell Idx = " + cell.posIdx + " ,,, BlockType = " + cell.blockType);
					return true;
				}
			}
			else {
				matchBlockCount = 1;
				baseBlockType = list_EqualAxisYCell [i].blockType;
			}
		}
		return false;
	}

	//Z축으로 매칭된 블록이 있는지 검사합니다.
	bool CheckMatchingBlockAxisZ (Cell cell)
	{
		List<Cell> list_EqualAxisZCell = new List<Cell> ();
		foreach (Cell c in cells) {
			if (cell.posIdx.z == c.posIdx.z)
				list_EqualAxisZCell.Add (c);
		}
		list_EqualAxisZCell.Sort ((Cell cellOne, Cell cellTwo) => cellOne.posIdx.x.CompareTo (cellTwo.posIdx.x));

		BlockType baseBlockType = list_EqualAxisZCell[0].blockType;
		int matchBlockCount = 1;

		for (int i = 1; i < list_EqualAxisZCell.Count; i++) {
			if ((baseBlockType == list_EqualAxisZCell [i].blockType) && (baseBlockType != BlockType.NONE) && (baseBlockType < BlockType.TOP)) {
				matchBlockCount += 1;
				if (matchBlockCount >= 3) {
					//Debug.Log ("### Matching Possible Z ,,, Cell Idx = " + cell.posIdx + " ,,, BlockType = " + cell.blockType);
					return true;
				}
			}
			else {
				matchBlockCount = 1;
				baseBlockType = list_EqualAxisZCell [i].blockType;
			}
		}
		return false;
	}

	//보드를 섞어줍니다.
	void ShuffleBoard (int mapHeight, int mapRow)
	{
		specialBlockCount = 0;
		foreach (Cell cell in cells) {
			BlockType getBlockType = GetRandomBlockType ();
			cell.InitBlockHP (getBlockType);
			cell.SetBlockType (getBlockType);
		}

		if (ExistMatchingBlock () == true) {
			txt_Comment.text = "이미 매칭중인 블록이 \n 존재하여 맵을 다시 셔플합니다.";
			ShuffleBoard (mapHeight, mapRow);
		} 
		else if (CheckPossibleBoard () == false) {
			ShuffleBoard (mapHeight, mapRow);
		}

		// Success.
		txt_Comment.text = "보드를 다시 셋팅했습니다.";
	}

	//블록타입을 랜덤으로 가져옵니다.
	BlockType GetRandomBlockType ()
	{
		int randomNum = specialBlockCount >= IngameDefine.maxSpecialBlockCntInBoard ? UnityEngine.Random.Range (1, 8) : UnityEngine.Random.Range (1, 9);
		BlockType blockType = BlockType.NONE;
		blockType = (BlockType)Enum.Parse (typeof(BlockType), randomNum.ToString());

		if (blockType == BlockType.TOP)
			specialBlockCount += 1;

		return blockType;
	}

	// 12시 방향에있는 블록을 가져옵니다..
	Cell GetTwentyDirectionCell (Cell srcCell, int gap = 1)
	{
		foreach (Cell cell in cells) {
			if ((srcCell.posIdx.x == cell.posIdx.x) && (srcCell.posIdx.y + gap == cell.posIdx.y)) {
				return cell;
			}
		}
		return null;
	}

	// 2시 방향에있는 블록을 가져옵니다..
	Cell GetTwoDirectionCell (Cell srcCell, int gap = 1)
	{
		foreach (Cell cell in cells) {
			if ((srcCell.posIdx.y == cell.posIdx.y) && (srcCell.posIdx.x + gap == cell.posIdx.x)) {
				return cell;
			}
		}
		return null;
	}

	// 4시 방향에있는 블록을 가져옵니다..
	Cell GetFourDirectionCell (Cell srcCell, int gap = 1)
	{
		foreach (Cell cell in cells) {
			if ((srcCell.posIdx.z == cell.posIdx.z) && (srcCell.posIdx.x + gap == cell.posIdx.x)) {
				return cell;
			}
		}
		return null;
	}

	// 6시 방향에있는 블록을 가져옵니다..
	Cell GetSixDirectionCell (Cell srcCell, int gap = 1)
	{
		foreach (Cell cell in cells) {
			if ((srcCell.posIdx.x == cell.posIdx.x) && (srcCell.posIdx.y - gap == cell.posIdx.y)) {
				return cell;
			}
		}
		return null;
	}

	// 8시 방향에있는 블록을 가져옵니다..
	Cell GetEightDirectionCell (Cell srcCell, int gap = 1)
	{
		foreach (Cell cell in cells) {
			if ((srcCell.posIdx.y == cell.posIdx.y) && (srcCell.posIdx.x - gap == cell.posIdx.x)) {
				return cell;
			}
		}
		return null;
	}

	// 10시 방향에있는 블록을 가져옵니다..
	Cell GetTenDirectionCell (Cell srcCell, int gap = 1)
	{
		foreach (Cell cell in cells) {
			if ((srcCell.posIdx.z == cell.posIdx.z) && (srcCell.posIdx.y + gap == cell.posIdx.y)) {
				return cell;
			}
		}
		return null;
	}

	// 두 블록이 스왑이 가능한지 체크합니다.
	// srcCell = 이동시키고 싶은 셀. dir = 이동하고자하는 방향.
	public void CheckEnableSwapCell (Cell srcCell, DragDirection dir)
	{
		Cell targetCell = null;

		switch (dir) {
		case DragDirection.TwentyOClock:
			targetCell = GetTwentyDirectionCell (srcCell);
			break;
		case DragDirection.TwoOClock:
			targetCell = GetTwoDirectionCell (srcCell);
			break;
		case DragDirection.FourOClock:
			targetCell = GetFourDirectionCell (srcCell);
			break;
		case DragDirection.SixOClock:
			targetCell = GetSixDirectionCell (srcCell);
			break;
		case DragDirection.EightOClock:
			targetCell = GetEightDirectionCell (srcCell);
			break;
		case DragDirection.TenOClock:
			targetCell = GetTenDirectionCell (srcCell);
			break;
		}
		CheckSwapCell (srcCell, targetCell);
	}

	void CheckSwapCell (Cell srcCell, Cell targetCell)
	{
		if (srcCell == null || targetCell == null) {
			Debug.Log ("--- Can Not Swap Cell !!! srcCell = " + srcCell + " ,,, targetCell = " + targetCell);
			return;
		}
		if (srcCell.blockType == targetCell.blockType) {
			Debug.Log ("--- Can Not Swap Cell !!! BlockType Equal !!!");
			return;
		}

		SwapCell (srcCell, targetCell);

		GetListBurstCellAxisX (srcCell);
		GetListBurstCellAxisX (targetCell);
		GetListBurstCellAxisY (srcCell);
		GetListBurstCellAxisY (targetCell);
		GetListBurstCellAxisZ (srcCell);
		GetListBurstCellAxisZ (targetCell);

		if (list_BurstCell.Count > 0) {
			StartCoroutine (BurstLoopRoutine ());
			moveCount = Mathf.Max (0, moveCount - 1);;
			refreshMoveCountUI ();
		} else {
			StartCoroutine (ResetCell (srcCell, targetCell));	
		}
	}

	//블록을 target으로 이동.
	void MoveCell(Cell srcCell, Cell targetCell)
	{
		if(targetCell.img_Block == null)
			targetCell.img_Block = GameObject.Instantiate (blockPrefab, targetCell.trBlock.transform).GetComponent<Image> ();

		targetCell.img_Block.transform.localPosition = Vector3.zero;
		SwapCell (srcCell, targetCell);
	}

	//스왑후 매칭이 불가능하면 이전으로 되돌립니다.
	IEnumerator ResetCell (Cell srcCell, Cell targetCell)
	{
		touchEnable = false;
		yield return new WaitForSeconds (0.5f);
		SwapCell (srcCell, targetCell);
		touchEnable = true;
	}

	//블록 스왑.
	void SwapCell(Cell srcCell, Cell targetCell)
	{
		int originSrcHP = srcCell.HP;
		srcCell.HP = targetCell.HP;
		targetCell.HP = originSrcHP;

		SwapBlockType (srcCell, targetCell);

		srcCell.SetBlockType (srcCell.blockType);
		targetCell.SetBlockType (targetCell.blockType);;
	}

	void SwapBlockType (Cell srcCell, Cell targetCell)
	{
		BlockType originSrcBlockType = srcCell.blockType;
		srcCell.blockType = targetCell.blockType;
		targetCell.blockType = originSrcBlockType;
	}

	//버스트된 블록 위의 블록들을 아래로 내려보냅니다.
	IEnumerator SlideAllDown ()
	{
		Dictionary<float, List<Cell>> dic_All = new Dictionary<float, List<Cell>> ();
		foreach (Cell burstCell in list_BurstCell) {
			List<Cell> list_UpCell = new List<Cell> ();
			foreach (Cell cell in cells) {
				if ((burstCell.posIdx.x == cell.posIdx.x) && (burstCell.posIdx.y < cell.posIdx.y)) {
					if (cell.blockType == BlockType.NONE) continue;
					if (list_UpCell.Contains (cell) == true) continue;
					list_UpCell.Add (cell);
				}
			}
			if(dic_All.ContainsKey(burstCell.posIdx.x) == false)
				dic_All.Add (burstCell.posIdx.x, list_UpCell);
		}

		int maxLength = 0;
		foreach (KeyValuePair<float, List<Cell>> kvp in dic_All) {
			maxLength = kvp.Value.Count > maxLength ? kvp.Value.Count : maxLength;
		}

		if (maxLength == 0)
			yield break;

		List<Cell> [] newArray = new List<Cell>[maxLength];
		for (int i = 0; i < newArray.Length; i++)
			newArray [i] = new List<Cell> ();

		foreach (KeyValuePair<float, List<Cell>> kvp in dic_All) {
			int startIndex = 0;
			foreach (Cell c in kvp.Value) {
				newArray[startIndex].Add(c);
				startIndex += 1;
			}
		}

		for (int i = 0; i < newArray.Length; i++) {
			int maxDelayCount = 0;
			foreach (Cell cell in newArray[i]) {
				List<Cell> pathList = GetPathOnlyDownPath (cell);
				int pathCount = pathList.Count;
				maxDelayCount = pathCount > maxDelayCount ? pathCount : maxDelayCount;
				StartCoroutine(SlideDown(cell, pathList));
			}
			yield return new WaitForSeconds (moveTime * maxDelayCount);
		}
	}

	List<Cell> GetPathOnlyDownPath (Cell srcCell)
	{
		List<Cell> list_DownPath = new List<Cell> ();
		FindLoopOnlyDownPath (list_DownPath, srcCell);
		if (list_DownPath.Count == 0) {
			return null;
		}
		list_DownPath.Insert (0, srcCell);
		return list_DownPath;
	}

	IEnumerator SlideDown (Cell srcCell, List<Cell> pathList)
	{
		for (int i = 0; i < pathList.Count-1; i++) {
			MoveCell (pathList [i], pathList [i+1]);
			yield return new WaitForSeconds (moveTime);
		}

		if(list_LoopCell.Contains(pathList[pathList.Count-1]) == false)
			list_LoopCell.Add (pathList[pathList.Count-1]);
	}

	//Idx로 셀을 가져옵니다.
	Cell GetCell (Vector3 idx)
	{
		foreach (Cell cell in cells) {
			if (cell == null)
				continue;
			if (cell.posIdx == idx)
				return cell;
		}
		return null;
	}

	//아래 비어있는 블록을 채워줍니다.
	IEnumerator SlideAllFill ()
	{
		int leftRow = IngameDefine.mapRow / 2;
		for (int i = leftRow; i < mapHeight; i++) {
			int startLeft = (mapHeight - i) * -1;
			for (int j = 0; j < i; j++) {
				//Debug.Log ("### GetCell Idx = " + new Vector3 (startLeft, (mapHeight - i) + j, j));
				Cell cell = GetCell (new Vector3(startLeft, (mapHeight - i) + j, j));
				if(cell.blockType != BlockType.NONE)
					yield return StartCoroutine(SlideFill (cell));
			}
		}

		int rightRow = mapRow / 2;
		for(int i = rightRow; i < mapHeight; i++)
		{
			int startRight = mapHeight - i;
			for (int j = 0; j < i; j++) {
				//Debug.Log ("### GetCell Idx = " + new Vector3(startRight, j, (mapHeight - i) + j));
				Cell cell = GetCell (new Vector3(startRight, j, (mapHeight - i) + j));
				if(cell.blockType != BlockType.NONE)
					yield return StartCoroutine(SlideFill (cell));
			}
		}

		for (int i = 0; i < mapHeight; i++) {
			//Debug.Log ("### GetCell Idx = " + new Vector3(0, i, i));
			Cell cell = GetCell (new Vector3(0, i, i));
			if(cell.blockType != BlockType.NONE)
				yield return StartCoroutine(SlideFill (cell));
		}
	}

	IEnumerator SlideFill (Cell srcCell)
	{
		List<Cell> list_DownPath = new List<Cell> ();
		FindLoopDownPath (list_DownPath, srcCell);
		if (list_DownPath.Count == 0) {
			yield break;
		}

		list_DownPath.Insert (0, srcCell);
		for (int i = 0; i < list_DownPath.Count-1; i++) {
			MoveCell (list_DownPath [i], list_DownPath [i+1]);
			yield return new WaitForSeconds (moveTime);
		}

		if(list_LoopCell.Contains(list_DownPath[list_DownPath.Count-1]) == false)
			list_LoopCell.Add (list_DownPath[list_DownPath.Count-1]);
	}

	//새로운 블록을 공급합니다.
	IEnumerator SupplyNewBlock ()
	{
		if (list_BurstCell.Count <= 0)
			yield break;

		List<Cell> findHigh = new List<Cell> ();
		List<Cell> list_Empty = new List<Cell> ();
		foreach (Cell cell in cells) {
			if (cell.posIdx.x == 0)
				findHigh.Add (cell);
			if (cell.blockType == BlockType.NONE)
				list_Empty.Add (cell);
		}

		//Debug.Log ("Highest Idx = " + findHigh [0].posIdx);   // 0, 5, 5
		findHigh.Sort ((Cell cellOne, Cell cellTwo) => cellTwo.posIdx.y.CompareTo (cellOne.posIdx.y));

		foreach (Cell empty in list_Empty)
		{
			List<Cell> list_SupplyRoot = new List<Cell> ();
			list_SupplyRoot.Add (findHigh [0]);
			FindLoopDownPath (list_SupplyRoot, list_SupplyRoot [0]);
			foreach (Cell cell in list_Empty) {
				if (cell.posIdx == list_SupplyRoot [list_SupplyRoot.Count-1].posIdx) {
					if(cell.img_Block == null)
						cell.img_Block = GameObject.Instantiate (blockPrefab, cell.trBlock.transform).GetComponent<Image> ();

					cell.img_Block.transform.position = new Vector3 (0f, 2.5f, 0f);
					BlockType getBlockType = GetRandomBlockType ();
					cell.InitBlockHP (getBlockType);
					cell.SetBlockType (getBlockType);
					yield return new WaitForSeconds (supplyTime);

					foreach (Cell c in list_SupplyRoot) {
						cell.img_Block.transform.position = c.transform.position;
						yield return new WaitForSeconds (supplyTime);
					}
					list_LoopCell.Add(list_SupplyRoot[list_SupplyRoot.Count-1]);
					break;
				}
			}
		}
	}

	//6시 방향으로 비어있는 공간 탐색.
	void FindLoopOnlyDownPath (List<Cell> pathList, Cell startCell)
	{
		Cell checkCell = GetSixDirectionCell (startCell);
		if (checkCell != null) {
			if (checkCell.blockType == BlockType.NONE) {
				pathList.Add (checkCell);
				FindLoopOnlyDownPath (pathList, checkCell);
			}
		}
	}

	//아래 3방향으로 비어있는 공간 탐색.
	void FindLoopDownPath (List<Cell> pathList, Cell startCell)
	{
		Cell checkCell = GetSixDirectionCell (startCell);
		if (checkCell != null) {
			if (checkCell.blockType == BlockType.NONE) {
				pathList.Add (checkCell);
				FindLoopDownPath (pathList, checkCell);
				return;
			}
		}
		checkCell = GetEightDirectionCell (startCell);
		if (checkCell != null) {
			if (checkCell.blockType == BlockType.NONE) {
				pathList.Add (checkCell);
				FindLoopDownPath (pathList, checkCell);
				return;
			}
		}
		checkCell = GetFourDirectionCell (startCell);
		if (checkCell != null) {
			if (checkCell.blockType == BlockType.NONE) {
				pathList.Add (checkCell);
				FindLoopDownPath (pathList, checkCell);
				return;
			}
		}
	}

	//체력이 2 이상인 블록 데미지 처리.
	void DealDamageBlock ()
	{	
		foreach (Cell cell in list_BurstCell) 
		{
			List<Cell> list = new List<Cell> ();
			Cell getCell = GetTwentyDirectionCell (cell);
			if ((getCell != null) && (getCell.blockType == BlockType.TOP))
				list.Add (getCell);
			getCell = GetTwoDirectionCell (cell);
			if ((getCell != null) && (getCell.blockType == BlockType.TOP))
				list.Add (getCell);
			getCell = GetFourDirectionCell (cell);
			if ((getCell != null) && (getCell.blockType == BlockType.TOP))
				list.Add (getCell);
			getCell = GetSixDirectionCell (cell);
			if ((getCell != null) && (getCell.blockType == BlockType.TOP))
				list.Add (getCell);
			getCell = GetEightDirectionCell (cell);
			if ((getCell != null) && (getCell.blockType == BlockType.TOP))
				list.Add (getCell);
			getCell = GetTenDirectionCell (cell);
			if ((getCell != null) && (getCell.blockType == BlockType.TOP))
				list.Add (getCell);

			for(int i=0; i<list.Count; i++) {
				if (list_DealDamageCell.Contains (list[i]) == false)
					list_DealDamageCell.Add (list[i]);
			}
		}

		foreach (Cell damage in list_DealDamageCell) {
			damage.DealDamageHP ();
			if (damage.HP <= 0) {
				missionCount = Mathf.Max(0, missionCount - 1);
				ComputeBlockScore (damage.blockType);
				specialBlockCount = specialBlockCount > 0 ? specialBlockCount - 1 : 0;
				damage.blockType = BlockType.NONE;
				damage.HP = 1;

				if(list_BurstCell.Contains(damage) == false)
					list_BurstCell.Add (damage);

				if(damage.img_Block != null)
					GameObject.Destroy (damage.img_Block.gameObject);
			} else {
				damage.SetBlockType (damage.blockType);	
			}
		}
		list_DealDamageCell.Clear ();
	}

	void ComputeBlockScore (BlockType _blockType)
	{
		score += _blockType < BlockType.TOP ? IngameDefine.normalBlockScore : IngameDefine.specialBlockScore;
	}

	void refreshScoreUI ()
	{
		txt_Score.text = string.Format("{0:#,###; #,##;0}", score);
	}

	void refreshMoveCountUI ()
	{
		txt_Move.text = moveCount.ToString();
	}

	void refreshMissionCountUI ()
	{
		txt_Mission.text = missionCount.ToString();
	}

	public void EmptyComment ()
	{
		txt_Comment.text = "게임중...";
	}

	//블록매칭 후 버스트 루프 루틴.
	IEnumerator BurstLoopRoutine ()
	{
		yield return new WaitForSeconds (burstDelayTime);
		touchEnable = false;	
		foreach (Cell cell in list_BurstCell) {
			ComputeBlockScore (cell.blockType);
			cell.blockType = BlockType.NONE;
			if(cell.img_Block != null)
				GameObject.Destroy (cell.img_Block.gameObject);
		}
		yield return new WaitForSeconds (burstDelayTime);
		DealDamageBlock ();
		refreshScoreUI ();
		refreshMissionCountUI ();
		yield return new WaitForSeconds (0.2f);
		yield return StartCoroutine (SlideAllDown ());
		yield return StartCoroutine (SlideAllFill ());
		yield return StartCoroutine (SupplyNewBlock ());

		list_BurstCell.Clear ();

		foreach (Cell cell in list_LoopCell) {
			GetListBurstCellAxisX (cell);
			GetListBurstCellAxisY (cell);
			GetListBurstCellAxisZ (cell);
		}

		list_LoopCell.Clear ();

		if (list_BurstCell.Count > 0) {
			yield return StartCoroutine (BurstLoopRoutine ());
		} else {
			touchEnable = CheckGameState () == GameState.PLAY ? true : false;
			if (gameState >= GameState.CLEAR)
				yield break;

			if (CheckPossibleBoard () == false) {
				yield return StartCoroutine(DelaySuffleBoard());
			}
		}
	}

	//딜레이 후 보드를 섞어줍니다. (타이머용)
	IEnumerator DelaySuffleBoard ()
	{
		txt_SuffleTimer.gameObject.SetActive (true);
		txt_SuffleTimer.text = "3초";
		yield return new WaitForSeconds (1f);
		txt_SuffleTimer.text = "2초";
		yield return new WaitForSeconds (1f);
		txt_SuffleTimer.text = "1초";
		yield return new WaitForSeconds (1f);
		txt_SuffleTimer.gameObject.SetActive (false);
		ShuffleBoard (mapHeight, mapRow);
	}

	GameState CheckGameState ()
	{
		if (missionCount <= 0) {
			gameState = GameState.CLEAR;
			btn_Restart.SetActive (true);
			txt_Comment.text = "!!! GAME CLEAR !!!";
			txt_Comment.transform.localScale = new Vector3 (2f, 2f, 2f);
		} else if (moveCount <= 0) {
			gameState = GameState.FAIL;
			btn_Restart.SetActive (true);
			txt_Comment.text = "GAME FAIL ...";
		}
		else
			gameState = GameState.PLAY;

		return gameState;
	}

	//셀 기준 X축으로 터질 수 있는 블록확보.
	void GetListBurstCellAxisX (Cell cell)
	{
		if ((cell.blockType == BlockType.NONE) || (cell.blockType == BlockType.TOP))
			return;

		List<Cell> list_BurstAxisXCell = new List<Cell> ();
		List<Cell> list_EqualAxisXCell = new List<Cell> ();

		foreach (Cell c in cells) {
			if (cell.posIdx.x == c.posIdx.x)
				list_EqualAxisXCell.Add (c);
		}

		list_EqualAxisXCell.Sort ((Cell cellOne, Cell cellTwo) => cellOne.posIdx.y.CompareTo (cellTwo.posIdx.y));

		int matchBlockCount = 0;
		bool matchBlockExist = false;

		for (int i = 0; i < list_EqualAxisXCell.Count; i++) {
			if (cell.blockType == list_EqualAxisXCell [i].blockType) {
				list_BurstAxisXCell.Add (list_EqualAxisXCell [i]);
				matchBlockCount += 1;
				if (matchBlockCount >= 3) {
					matchBlockExist = true;
				}
			} else {
				if (matchBlockExist == true)
					break;
				matchBlockCount = 0;
				list_BurstAxisXCell.Clear ();
			}
		}

		if (matchBlockExist == true) {
			foreach (Cell c in list_BurstAxisXCell) {
				if(list_BurstCell.Contains(c) == false)
					list_BurstCell.Add (c);
			}
		}
	}

	//셀 기준 Y축으로 터질 수 있는 블록확보.
	void GetListBurstCellAxisY (Cell cell)
	{
		if ((cell.blockType == BlockType.NONE) || (cell.blockType == BlockType.TOP))
			return;

		List<Cell> list_BurstAxisYCell = new List<Cell> ();
		List<Cell> list_EqualAxisYCell = new List<Cell> ();

		foreach (Cell c in cells) {
			if (cell.posIdx.y == c.posIdx.y)
				list_EqualAxisYCell.Add (c);
		}

		list_EqualAxisYCell.Sort ((Cell cellOne, Cell cellTwo) => cellOne.posIdx.x.CompareTo (cellTwo.posIdx.x));

		int matchBlockCount = 0;
		bool matchBlockExist = false;

		for (int i = 0; i < list_EqualAxisYCell.Count; i++) {
			if (cell.blockType == list_EqualAxisYCell [i].blockType) {
				list_BurstAxisYCell.Add (list_EqualAxisYCell [i]);
				matchBlockCount += 1;
				if (matchBlockCount >= 3) {
					matchBlockExist = true;
				}
			} else {
				if (matchBlockExist == true)
					break;
				matchBlockCount = 0;
				list_BurstAxisYCell.Clear ();
			}
		}

		if (matchBlockExist == true) {
			foreach (Cell c in list_BurstAxisYCell) {
				if(list_BurstCell.Contains(c) == false)
					list_BurstCell.Add (c);
			}
		}
	}

	//셀 기준 Z축으로 터질 수 있는 블록확보.
	void GetListBurstCellAxisZ (Cell cell)
	{
		if ((cell.blockType == BlockType.NONE) || (cell.blockType == BlockType.TOP))
			return;

		List<Cell> list_BurstAxisZCell = new List<Cell> ();
		List<Cell> list_EqualAxisZCell = new List<Cell> ();

		foreach (Cell c in cells) {
			if (cell.posIdx.z == c.posIdx.z)
				list_EqualAxisZCell.Add (c);
		}

		list_EqualAxisZCell.Sort ((Cell cellOne, Cell cellTwo) => cellOne.posIdx.x.CompareTo (cellTwo.posIdx.x));

		int matchBlockCount = 0;
		bool matchBlockExist = false;

		for (int i = 0; i < list_EqualAxisZCell.Count; i++) {
			if (cell.blockType == list_EqualAxisZCell [i].blockType) {
				list_BurstAxisZCell.Add (list_EqualAxisZCell [i]);
				matchBlockCount += 1;
				if (matchBlockCount >= 3) {
					matchBlockExist = true;
				}
			} else {
				if (matchBlockExist == true)
					break;
				matchBlockCount = 0;
				list_BurstAxisZCell.Clear ();
			}
		}

		if (matchBlockExist == true) {
			foreach (Cell c in list_BurstAxisZCell) {
				if(list_BurstCell.Contains(c) == false)
					list_BurstCell.Add (c);
			}
		}
	}

	#region Button Clicked
	public void Clicked_Restart ()
	{
		SceneManager.LoadScene ("Main");
	}
	#endregion Button Clicked
}


