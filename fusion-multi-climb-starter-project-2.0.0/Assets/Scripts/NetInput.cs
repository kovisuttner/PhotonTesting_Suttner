using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public enum InputButton
{
    Jump, 
}

public struct NetInput : INetworkInput
{
    public NetworkButtons Buttons;
    public Vector2 Direction;
}