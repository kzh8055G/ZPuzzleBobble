﻿
using System;

public enum EBubbleColor
{
	Red = 0,
	Green = 1,
	Blue = 2,
	Yellow = 3,
	Orange = 4,
	Gray = 5,
	Purple = 6,

	None,       // level edit mode 에서 erase 로 쓴다

}

[Flags]
public enum EDirection
{
	None = 0,

	Up = 1,
	Down = 2,
	Left = 4,
	Right = 8,
}
