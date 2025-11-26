using UnityEngine;
using System.Collections;

public class SimpleAnim : MonoBehaviour
{
    public Texture2D[] frames;  // Arrastra aquí tus PNGs
    public float speed = 0.1f;  // Tiempo entre frames
    
    private Renderer rend;
    private int currentFrame = 0;
    
    void Start()
    {
        rend = GetComponent<Renderer>();
        StartCoroutine(PlayAnimation());
    }
    
    IEnumerator PlayAnimation()
    {
        while (true)
        {
            if (frames.Length > 0 && frames[currentFrame] != null)
            {
                rend.material.mainTexture = frames[currentFrame];
            }
            
            currentFrame = (currentFrame + 1) % frames.Length;
            yield return new WaitForSeconds(speed);
        }
    }
}