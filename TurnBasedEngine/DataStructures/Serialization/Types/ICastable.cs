﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICastable<T> {
    T Cast();
}