using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectedPlayersList : MonoBehaviour
{
    [SerializeField] private Scrollbar scrollbar;
    // Start is called before the first frame update
    void Start()
    {
        scrollbar.size = 0.5f;
        scrollbar.value = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
