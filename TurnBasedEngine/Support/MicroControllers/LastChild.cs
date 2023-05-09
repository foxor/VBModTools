using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastChild : MonoBehaviour
{
    public void LateUpdate()
    {
        transform.SetAsLastSibling();
    }
}
