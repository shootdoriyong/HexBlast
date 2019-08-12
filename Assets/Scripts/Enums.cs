using UnityEngine;

public enum BlockType {
	NONE = 0,
	WHITE,
	PURPLE,
	BLUE,
	GREEN,
	YELLOW,
	ORANGE,
	RED,
	TOP,
}

//드래그 방향으로 시계방향을 사용.
public enum DragDirection {
	None,
	TwentyOClock,
	TwoOClock,
	FourOClock,
	SixOClock,
	EightOClock,
	TenOClock,
}

public enum TouchState {
	Enable,
	Disable
}

public enum GameState {
	START,
	CLEAR,
	GAMEOVER,
}
