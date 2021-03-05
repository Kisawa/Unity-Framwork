﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFsm
{
    void Init(FsmUtility fsmUtility);
    void Enter(object sender = null);
    void Leave();
    void Update();
    void FixedUpdate();
}