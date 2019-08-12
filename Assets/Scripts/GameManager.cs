using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;
using System;

public class GameManager : Singleton<GameManager> {
	
	//Objects.
	Transform cellRoot;

	GameObject cellPrefab;
	GameObject blockPrefab;

	List<Cell> cells = new List<Cell>();
	List<Cell> list_BurstCell = new List<Cell> ();
	List<Cell> list_LoopCell = new List<Cell>();

	int specialBlockCount = 0;
	public bool touchEnable = true;

	void Start ()
	{
		cellRoot = GameObject.Find ("Canvas/CellRoot").transform;

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

		int leftRow = mapRow / 2;
		for(int i = leftRow; i<mapHeight; i++)
		{
			int startLeft = (mapHeight - i) * -1;
			for (int j = 0; j < i; j++) {
				Cell cell = GameObject.Instantiate (cellPrefab, cellRoot).GetComponent<Cell>();
				cell.transform.name = string.Format ("Cell( {0}, {1}, {2} )", startLeft, (mapHeight - i) + j, j);
				cell.SetCoordinate (new Vector3(startLeft, (mapHeight - i) + j, j));
				cell.SetBlockType (GetBlockRandomType ());
				cells.Add (cell);
			}
		}

		for (int i = 0; i < mapHeight; i++) {
			Cell cell = GameObject.Instantiate (cellPrefab, cellRoot).GetComponent<Cell>();
			cell.transform.name = string.Format ("Cell( {0}, {1}, {2} )", 0, i, i);
			cell.SetCoordinate (new Vector3(0, i, i));
			cell.SetBlockType (GetBlockRandomType ());
			cells.Add (cell);
		}

		int rightRow = mapRow / 2;
		for(int i = rightRow; i<mapHeight; i++)
		{
			int startRight = mapHeight - i;
			for (int j = 0; j < i; j++) {
				Cell cell = GameObject.Instantiate (cellPrefab, cellRoot).GetComponent<Cell>();
				cell.transform.name = string.Format ("Cell( {0}, {1}, {2} )", startRight, j, (mapHeight - i) + j);
				cell.SetCoordinate (new Vector3(startRight, j, (mapHeight - i) + j));
				cell.SetBlockType (GetBlockRandomType ());
				cells.Add (cell);
			}
		}

		Debug.Log (" ### Special Block Count = " + specialBlockCount);

		if (ExistMatchingBlock () == true) {
			ShuffleBoard (mapHeight, mapRow);
		}
	}

	void ShuffleBoard (int mapHeight, int mapRow)
	{
		specialBlockCount = 0;
		foreach (Cell cell in cells) {
			cell.SetBlockType (GetBlockRandomType ());
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

	void GetMatchingBlock ()
	{
		
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
		BlockType originSrcBlockType = srcCell.blockType;
		srcCell.blockType = targetCell.blockType;
		targetCell.blockType = originSrcBlockType;

		srcCell.SetBlockType (srcCell.blockType);
		srcCell.SetBlockHP (srcCell.blockType);
		targetCell.SetBlockType (targetCell.blockType);
		targetCell.SetBlockHP (targetCell.blockType);
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
		
	IEnumerator SlideAllDown ()
	{
		cells.Sort ((Cell cellOne, Cell cellTwo) => cellOne.posIdx.y.CompareTo (cellTwo.posIdx.y));
		foreach (Cell cell in cells) {
			if (cell.blockType == BlockType.NONE) 
				continue;
			yield return StartCoroutine(SlideDown (cell));
		}
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
			yield return new WaitForSeconds (0.3f);
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

		Debug.Log ("Highest Idx = " + findHigh [0].posIdx);   // 0, 5, 5
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
					cell.SetBlockType (GetBlockRandomType ());
					yield return new WaitForSeconds (0.3f);

					foreach (Cell c in list_SupplyRoot) {
						cell.img_Block.transform.position = c.transform.position;
						yield return new WaitForSeconds (0.3f);
					}
					break;
				}
			}
			yield return new WaitForSeconds (0.3f);
		}
		yield return null;
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

	void GetSupplyPath (List<Cell> emptyList)
	{
		List<Cell> list = new List<Cell>();
		foreach (Cell cell in cells) {
			if (cell.posIdx.x == 0)
				list.Add (cell);
		}
		list.Sort ((Cell cellOne, Cell cellTwo) => cellTwo.posIdx.y.CompareTo (cellOne.posIdx.y));
		GetSupplyPathAxisX (list [0]);
	}

	List<Vector3> GetSupplyPathAxisX (Cell highestCell)
	{
		return null;
	}

	List<Vector3> GetSupplyPathAxisY ()
	{
		return null;
	}

	IEnumerator BurstLoopRoutine ()
	{
		yield return new WaitForSeconds (0.5f);

		foreach (Cell cell in list_BurstCell) {
			Debug.Log ("@@@ Burst PosIdx = " + cell.posIdx + " ,,, BlockType = " + cell.blockType);
			cell.blockType = BlockType.NONE;
			GameObject.Destroy (cell.img_Block.gameObject);
		}
		yield return new WaitForSeconds (0.2f);
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
		}
	}

	IEnumerator ResetCell (Cell srcCell, Cell targetCell)
	{
		yield return new WaitForSeconds (0.5f);
		SwapCell (srcCell, targetCell);
		touchEnable = true;
	}

	void GetListBurstCellAxisX (Cell cell)
	{
		if (cell.blockType == BlockType.NONE)
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
				list_BurstCell.Add (c);
			}
		}
	}

	void GetListBurstCellAxisY (Cell cell)
	{
		if (cell.blockType == BlockType.NONE)
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
				list_BurstCell.Add (c);
			}
		}
	}

	void GetListBurstCellAxisZ (Cell cell)
	{
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
				list_BurstCell.Add (c);
			}
		}
	}
}


