using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;

    private void Start() {
        DontDestroyOnLoad(this.gameObject);
    }
}
