using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framwork;

public class pool : IObjectPool
{
    public int Num;
    bool enable;
    public bool IsEnable => enable;

    public bool IsDestroy => false;

    public void Destroy()
    {
        
    }

    public void Disable()
    {
        enable = false;
    }

    public void Enable(object sender = null)
    {
        enable = true;
    }

    public void Init(object sender = null)
    {
        
    }
}
