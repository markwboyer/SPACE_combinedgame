using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FadeInOut : MonoBehaviour
{
    public RawImage img;
    public float speed = 0.8f;
    public enum FadeDirection
	{
		In, //Alpha = 1
		Out // Alpha = 0
	}

    // Start is called before the first frame update
    void Start()
    {
        // img.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator Fade(FadeDirection fadeDirection)
	{
		float alpha = (fadeDirection == FadeDirection.Out)? 1 : 0;
		float end_value = (fadeDirection == FadeDirection.Out)? 0 : 1;
        if (fadeDirection == FadeDirection.Out) {
			while (alpha >= end_value)
			{
				SetColorImg(ref alpha, fadeDirection);
				yield return null;
			}
			img.enabled = false; 
		} else {
			img.enabled = true; 
			while (alpha <= end_value)
			{
				SetColorImg(ref alpha, fadeDirection);
				yield return null;
			}
		}

    }

    private void SetColorImg(ref float alpha, FadeDirection fadeDirection)
	{
		img.color = new Color (img.color.r,img.color.g, img.color.b, alpha);
		alpha += Time.deltaTime * (1.0f / speed) * ((fadeDirection == FadeDirection.Out)? -1 : 1);
	}
}
