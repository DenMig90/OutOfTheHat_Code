using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "EyeBramble")]
public class EyeBrambleScriptableObject : ScriptableObject
{
    public Sprite coverSprite;
    public Sprite armSprite;
    public RuntimeAnimatorController handAnimController;
    public RuntimeAnimatorController eyeLidAnimController;
}
