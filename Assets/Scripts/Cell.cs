using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour {

	//Objects.
	public Transform trBlock;
	public Image img_Block;
	private Text txt_Pos;

	public BlockType blockType = BlockType.NONE;

	public Vector3 posIdx;
	[SerializeField]
	private int hp;

	private Vector3 btnDownPos;
	private Vector3 btnUpPos;

	void Awake ()
	{
		img_Block = trBlock.Find ("Block").GetComponent<Image> ();
		txt_Pos = transform.Find ("TXT_Pos").GetComponent<Text> ();
	}
		
	public int HP
	{
		get { return hp; }
		set { hp = value; }
	}

	public void InitBlockHP (BlockType _blockType)
	{
		HP = _blockType >= BlockType.TOP ? 2 : 1;
	}

	public void DealDamageHP()
	{
		HP -= 1;
	}
		
	public void SetBlockType(BlockType _blockType)
	{
		blockType = _blockType;
		string spriteName = string.Format ("Textures/Hex/img_{0}_{1}", _blockType, HP);
		img_Block.sprite = Resources.Load<Sprite>(spriteName);
	}
		
	public void SetCoordinate (Vector3 _posIdx)
	{
		posIdx = _posIdx;
		txt_Pos.text = string.Format ("{0},{1},{2}", (int)posIdx.x, (int)posIdx.y, (int)posIdx.z);
		transform.localPosition = new Vector3 (posIdx.x * (IngameDefine.cellWidth * (3f/4f)), posIdx.y * (IngameDefine.cellHeight * 0.5f) + posIdx.z * (IngameDefine.cellHeight * 0.5f), 0f);
	}

	DragDirection GetDirection ()
	{
		DragDirection dir = DragDirection.None;

		float angle = 180 * Mathf.Atan2 (btnUpPos.y - btnDownPos.y, btnUpPos.x - btnDownPos.x) / Mathf.PI;
		if (angle < 0)
			angle += 360;

		if (angle <= 120f && angle > 60f)
			dir = DragDirection.TwentyOClock;
		else if (angle <= 60f && angle > 0f)
			dir = DragDirection.TwoOClock;
		else if (angle <= 360f && angle > 300f)
			dir = DragDirection.FourOClock;
		else if (angle <= 300f && angle > 240f)
			dir = DragDirection.SixOClock;
		else if (angle <= 240f && angle > 180)
			dir = DragDirection.EightOClock;
		else if(angle <= 180f && angle > 120f)
			dir = DragDirection.TenOClock;

		//Debug.Log ("### Direction = " + dir + " ,,, Angle = " + angle);

		return dir;
	}

	#region EventTrigger
	public void BeginDrag ()
	{
		if (GameManager.instance.touchEnable == false) {
			Debug.Log (" Don't Touch !!!");
			return;
		}

		//Debug.Log ("BEGIN Pos = " + Input.mousePosition);
		btnDownPos = Input.mousePosition;
	}
		
	public void EndDrag ()
	{
		if (GameManager.instance.touchEnable == false) {
			Debug.Log (" Don't Touch !!!");
			return;
		}

		//Debug.Log ("END Pos = " + Input.mousePosition);
		btnUpPos = Input.mousePosition;

		DragDirection dir = GetDirection ();
		GameManager.instance.CheckEnableSwapCell (this, dir);
	}
	#endregion EventTrigger
}
