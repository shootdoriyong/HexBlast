using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using System;

public class GameManager : Singleton<GameManager> {
	
	//Objects.
	Transform 	cellRoot;
	Text 		txt_Comment;

	Text 		txt_MissionCount;
	Text 		txt_MoveCount;
	Text 		txt_Score;

	GameObject cellPrefab;
	GameObject blockPrefab;

	List<Cell> cells = new List<Cell>();
	List<Cell> list_BurstCell = new List<Cell> ();
	List<Cell> list_LoopCell = new List<Cell>();
	List<Cell> list_DealDamageCell = new List<Cell>();

	float moveTime = 0.02f;
	int specialBlockCount = 0;
	public bool touchEnable = true;

	int missionCount = 10;
	int moveCount = 30;
	int score = 0;

	void Start ()
	{
		cellRoot = GameObject.Find ("Canvas/CellRoot").transform;
		txt_Comment = GameObject.Find ("Canvas/Text").GetComponent<Text> ();

//		txt_MissionCount = GameObject.Find ("Canvas/Text_MissionCount").GetComponent<Text> ();
//		txt_MoveCount = GameObject.Find ("Canvas/Text_MoveCount").GetComponent<Text> ();
//		txt_Score = GameObject.Find ("Canvas/Text_Score").GetComponent<Text> ();

		cellPrefab = Resources.Load<GameObject>("Prefabs/Cell");
		blockPrefab = Resources.Load<GameObject>("Prefabs/Block");
		InitializeBoard (IngameDefine.maximumHeight, IngameDefine.mapRow);
	}

	void Update() 
	{
		#if (UNITY_ANDROID && !UNITY_EDITOR)
		{
			if (Input.touchCount > 0) 
			{ 
				if (Input.GetTouch(0).phase == TouchPhase.Began) 
				{ 

				} 
				else if (Input.GetTouch(0).phase == TouchPhase.Moved) 
				{ 

				} 
			} 
		}
		#else
		{
			if (Input.GetMouseButtonDown(0)) 
			{ 
//				Vector2 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
//				Ray2D ray = new Ray2D(wp, Vector2.zero);
//				RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero);
//
//				if(hit == null)
//				{
//					Debug.Log("### Hit NULL");
//				}
//				else
//				{
//					Debug.Log("Hit Name = " + hit.transform.name);
//				}
			} 
			else if (Input.GetMouseButtonUp(0)) 
			{ 
				
			} 
		}
		#endif
	}

	void InitializeBoard (int mapHeight, int mapRow)
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
				BlockType getBlockType = GetBlockRandomType ();
				cell.InitBlockHP (getBlockType);
				cell.SetBlockType (getBlockType);
				cells.Add (cell);
			}
		}

		for (int i = 0; i < mapHeight; i++) {
			Cell cell = GameObject.Instantiate (cellPrefab, cellRoot).GetComponent<Cell>();
			cell.transform.name = string.Format ("Cell( {0}, {1}, {2} )", 0, i, i);
			cell.SetCoordinate (new Vector3(0, i, i));
			BlockType getBlockType = GetBlockRandomType ();
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
				BlockType getBlockType = GetBlockRandomType ();
				cell.InitBlockHP (getBlockType);
				cell.SetBlockType (getBlockType);
				cells.Add (cell);
			}
		}

//		for(int i = rightRow; i<mapHeight; i++)
//		{
//			int startRight = mapHeight - i;
//			for (int j = 0; j < i; j++) {
//				Cell cell = GameObject.Instantiate (cellPrefab, cellRoot).GetComponent<Cell>();
//				cell.transform.name = string.Format ("Cell( {0}, {1}, {2} )", startRight, j, (mapHeight - i) + j);
//				cell.SetCoordinate (new Vector3(startRight, j, (mapHeight - i) + j));
//				BlockType getBlockType = GetBlockRandomType ();
//				cell.InitBlockHP (getBlockType);
//				cell.SetBlockType (getBlockType);
//				cells.Add (cell);
//			}
//		}

		Debug.Log (" ### Special Block Count = " + specialBlockCount);

		if (ExistMatchingBlock () == true) {
			txt_Comment.text = "이미 매칭중인 블록이 \n 존재하여 맵을 다시 셔플합니다.";
			ShuffleBoard (mapHeight, mapRow);
		}
	}

	void ShuffleBoard (int mapHeight, int mapRow)
	{
		specialBlockCount = 0;
		foreach (Cell cell in cells) {
			BlockType getBlockType = GetBlockRandomType ();
			cell.InitBlockHP (getBlockType);
			cell.SetBlockType (getBlockType);
		}
		Debug.Log (" ### Special Block Count = " + specialBlockCount);

		if (ExistMatchingBlock () == true) {
			ShuffleBoard (mapHeight, mapRow);
		}
	}

	BlockType GetBlockRandomType ()
	{
		int randomNum = specialBlockCount >= IngameDefine.maxSpecialBlockCount ? UnityEngine.Random.Range (1, 8) : UnityEngine.Random.Range (1, 9);
		BlockType blockType = BlockType.NONE;
		blockType = (BlockType)Enum.Parse (typeof(BlockType), randomNum.ToString());

		if (blockType == BlockType.TOP)
			specialBlockCount += 1;

		return blockType;
	}

	bool ExistMatchingBlock ()
	{
		foreach (Cell cell in cells) {
			if (cell.blockType == BlockType.NONE)
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
			if ((baseBlockType == list_EqualAxisXCell [i].blockType) && (baseBlockType != BlockType.NONE)) {
				matchBlockCount += 1;
				if (matchBlockCount >= 3)
					return true;
			}
			else {
				matchBlockCount = 1;
				baseBlockType = list_EqualAxisXCell [i].blockType;
			}
		}
		return false;
	}

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
			if ((baseBlockType == list_EqualAxisYCell [i].blockType) && (baseBlockType != BlockType.NONE)) {
				matchBlockCount += 1;
				if (matchBlockCount >= 3)
					return true;
			}
			else {
				matchBlockCount = 1;
				baseBlockType = list_EqualAxisYCell [i].blockType;
			}
		}
		return false;
	}

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
			if ((baseBlockType == list_EqualAxisZCell [i].blockType) && (baseBlockType != BlockType.NONE)) {
				matchBlockCount += 1;
				if (matchBlockCount >= 3)
					return true;
			}
			else {
				matchBlockCount = 1;
				baseBlockType = list_EqualAxisZCell [i].blockType;
			}
		}

		return false;
	}

	// 12시 방향에있는 셀 탐색.
	Cell GetTwentyDirectionCell (Cell srcCell)
	{
		foreach (Cell cell in cells) {
			if ((srcCell.posIdx.x == cell.posIdx.x) && (srcCell.posIdx.y + 1 == cell.posIdx.y)) {
				//Debug.Log ("--- Get TargetCell Idx = " + cell.posIdx + " ,,, BlockType = " + cell.blockType);
				return cell;
			}
		}
		return null;
	}

	// 2시 방향에있는 셀 탐색.
	Cell GetTwoDirectionCell (Cell srcCell)
	{
		foreach (Cell cell in cells) {
			if ((srcCell.posIdx.y == cell.posIdx.y) && (srcCell.posIdx.x + 1 == cell.posIdx.x)) {
				//Debug.Log ("--- Get TargetCell Idx = " + cell.posIdx + " ,,, BlockType = " + cell.blockType);
				return cell;
			}
		}
		return null;
	}

	// 4시 방향에있는 셀 탐색.
	Cell GetFourDirectionCell (Cell srcCell)
	{
		foreach (Cell cell in cells) {
			if ((srcCell.posIdx.z == cell.posIdx.z) && (srcCell.posIdx.x + 1 == cell.posIdx.x)) {
				//Debug.Log ("--- Get TargetCell Idx = " + cell.posIdx + " ,,, BlockType = " + cell.blockType);
				return cell;
			}
		}
		return null;
	}

	// 6시 방향에있는 셀 탐색.
	Cell GetSixDirectionCell (Cell srcCell)
	{
		foreach (Cell cell in cells) {
			if ((srcCell.posIdx.x == cell.posIdx.x) && (srcCell.posIdx.y - 1 == cell.posIdx.y)) {
				//Debug.Log ("--- Get TargetCell Idx = " + cell.posIdx + " ,,, BlockType = " + cell.blockType);
				return cell;
			}
		}
		return null;
	}

	// 8시 방향에있는 셀 탐색.
	Cell GetEightDirectionCell (Cell srcCell)
	{
		foreach (Cell cell in cells) {
			if ((srcCell.posIdx.y == cell.posIdx.y) && (srcCell.posIdx.x - 1 == cell.posIdx.x)) {
				//Debug.Log ("--- Get TargetCell Idx = " + cell.posIdx + " ,,, BlockType = " + cell.blockType);
				return cell;
			}
		}
		return null;
	}

	// 10시 방향에있는 셀 탐색.
	Cell GetTenDirectionCell (Cell srcCell)
	{
		foreach (Cell cell in cells) {
			if ((srcCell.posIdx.z == cell.posIdx.z) && (srcCell.posIdx.y + 1 == cell.posIdx.y)) {
				//Debug.Log ("--- Get TargetCell Idx = " + cell.posIdx + " ,,, BlockType = " + cell.blockType);
				return cell;
			}
		}
		return null;
	}

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

	void MoveCell(Cell srcCell, Cell targetCell)
	{
		if(targetCell.img_Block == null)
			targetCell.img_Block = GameObject.Instantiate (blockPrefab, targetCell.trBlock.transform).GetComponent<Image> ();
		
		targetCell.img_Block.transform.localPosition = Vector3.zero;
		SwapCell (srcCell, targetCell);
	}

	void SwapCell(Cell srcCell, Cell targetCell)
	{
		int originSrcHP = srcCell.HP;
		srcCell.HP = targetCell.HP;
		targetCell.HP = originSrcHP;

		BlockType originSrcBlockType = srcCell.blockType;
		srcCell.blockType = targetCell.blockType;
		targetCell.blockType = originSrcBlockType;

		srcCell.SetBlockType (srcCell.blockType);
		targetCell.SetBlockType (targetCell.blockType);;
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
		} else {
			StartCoroutine (ResetCell (srcCell, targetCell));	
		}
	}

	IEnumerator SlideAllOnlyDown ()
	{
		//Cell Sort.
//		cells.Sort ((Cell cellOne, Cell cellTwo) => cellOne.posIdx.x.CompareTo (cellTwo.posIdx.x));
//		List<int> list_RowHeight = new List<int> ();
//		int startX = (int)cells [0].posIdx.x;
//		int inRowNum = 0;
//		foreach (Cell cell in cells) {
//			if (startX == (int)cell.posIdx.x) {
//				inRowNum += 1;
//			}
//			else {
//				list_RowHeight.Add (inRowNum);
//				inRowNum = 1;
//				startX = (int)cell.posIdx.x;
//			}
//		}
//		if(inRowNum > 0)
//			list_RowHeight.Add (inRowNum);
//
//		for (int i = 0; i < list_RowHeight.Count; i++) {
//			Debug.Log ("### Height = " + list_RowHeight [i]);
//		}
			
		List<Cell> list_UpCell = new List<Cell> ();
		foreach (Cell burstCell in list_BurstCell) {
			foreach (Cell cell in cells) {
				if ((burstCell.posIdx.x == cell.posIdx.x) && (burstCell.posIdx.y < cell.posIdx.y)) {
					if (cell.blockType == BlockType.NONE) continue;
					if (list_UpCell.Contains (cell) == true) continue;
					list_UpCell.Add (cell);
				}
			}
		}
		if (list_UpCell.Count == 0)
			yield break;
		foreach (Cell cell in list_UpCell) {
			yield return StartCoroutine(SlideOnlyDown (cell));
		}
		yield return null;
	}
		
	IEnumerator SlideAllDown ()
	{
		//cells.Sort ((Cell cellOne, Cell cellTwo) => cellTwo.posIdx.x.CompareTo (cellOne.posIdx.x));
		foreach (Cell cell in cells) {
			if (cell.blockType == BlockType.NONE) 
				continue;
			yield return StartCoroutine(SlideDown (cell));
		}
	}

	IEnumerator SlideOnlyDown (Cell srcCell)
	{
		List<Cell> list_DownPath = new List<Cell> ();
		FindLoopOnlyDownPath (list_DownPath, srcCell);
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

	IEnumerator SlideDown (Cell srcCell)
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

					cell.img_Block.transform.position = new Vector3 (Screen.width * 0.5f, Screen.height * 0.95f, 0f);
					BlockType getBlockType = GetBlockRandomType ();
					cell.InitBlockHP (getBlockType);
					cell.SetBlockType (getBlockType);
					yield return new WaitForSeconds (moveTime);

					foreach (Cell c in list_SupplyRoot) {
						cell.img_Block.transform.position = c.transform.position;
						yield return new WaitForSeconds (moveTime);
					}
					list_LoopCell.Add(list_SupplyRoot[list_SupplyRoot.Count-1]);
					break;
				}
			}
			yield return new WaitForSeconds (moveTime);
		}
		yield return null;
	}

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

	IEnumerator DealDamageBlock ()
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

		yield return null;

		foreach (Cell damage in list_DealDamageCell) {
			Debug.Log ("DAMAGE Idx = " + damage.posIdx);
			damage.DealDamageHP ();
			if (damage.HP <= 0) {
				specialBlockCount = specialBlockCount > 0 ? specialBlockCount - 1 : 0;
				damage.blockType = BlockType.NONE;
				damage.HP = 1;
				if(damage.img_Block != null)
					GameObject.Destroy (damage.img_Block.gameObject);
			} else {
				damage.SetBlockType (damage.blockType);	
			}
		}

		list_DealDamageCell.Clear ();
	}

	IEnumerator BurstLoopRoutine ()
	{
		touchEnable = false;
		yield return new WaitForSeconds (0.5f);

		foreach (Cell cell in list_BurstCell) {
			Debug.Log ("@@@ Burst PosIdx = " + cell.posIdx + " ,,, BlockType = " + cell.blockType);
			cell.blockType = BlockType.NONE;
			if(cell.img_Block != null)
				GameObject.Destroy (cell.img_Block.gameObject);
		}
		yield return new WaitForSeconds (0.5f);
		yield return StartCoroutine (DealDamageBlock ());
		yield return new WaitForSeconds (0.5f);
		yield return StartCoroutine (SlideAllOnlyDown ());
		yield return StartCoroutine (SlideAllDown ());
		yield return StartCoroutine (SupplyNewBlock ());

		list_BurstCell.Clear ();

		foreach (Cell cell in list_LoopCell) {
			//Debug.Log ("### LoopCheck Idx = " + cell.posIdx);
			GetListBurstCellAxisX (cell);
			GetListBurstCellAxisY (cell);
			GetListBurstCellAxisZ (cell);
		}

		list_LoopCell.Clear ();

		if (list_BurstCell.Count > 0) {
			yield return StartCoroutine (BurstLoopRoutine ());
		} else {
			touchEnable = true;
		}
	}

	IEnumerator ResetCell (Cell srcCell, Cell targetCell)
	{
		yield return new WaitForSeconds (0.5f);
		SwapCell (srcCell, targetCell);
		touchEnable = true;
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
}


