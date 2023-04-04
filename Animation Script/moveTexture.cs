using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveTexture : MonoBehaviour
{
    [SerializeField] float scrollSpeedY;
    [SerializeField] float scrollSpeedX;
    private MeshRenderer rend;
    void Start()
    {
        rend = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        rend.material.mainTextureOffset = new Vector2(Time.realtimeSinceStartup * scrollSpeedX, Time.realtimeSinceStartup * scrollSpeedY);
    }
}
