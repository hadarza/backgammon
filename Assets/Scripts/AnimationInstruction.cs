using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationInstruction : MonoBehaviour
{
    [SerializeField] GameObject Instruction;
    public void OnClickStart()
    {
        Instruction.GetComponent<Animation>().Play();
    }
}
