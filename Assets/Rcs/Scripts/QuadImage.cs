using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class QuadImage : MonoBehaviour
{
    [Header("Elements")]
    public Image ParentImage;
    public Image FillImage;
    
    public void Init(MondrianDataGenerator.Quad quad, int pixelsPerUnit, int mondrianSize)
    {
        transform.localPosition = new Vector3(quad.X * pixelsPerUnit, -quad.Y * pixelsPerUnit, 0.0f);
        ParentImage.GetComponent<RectTransform>().sizeDelta = new Vector2(quad.Size.Width, quad.Size.Height) * pixelsPerUnit;


        //int nbrPixelMondrian = mondrianSize * pixelsPerUnit;

        //transform.localPosition = new Vector3(nbrPixelMondrian / 2.0f - (quad.X * pixelsPerUnit), nbrPixelMondrian / 2.0f - quad.Y * pixelsPerUnit, 0.0f);

    }

    public void SetColor(Color color)
    {
        FillImage.color = color;
    }

    public void SetTransitionColor(Color color)
    {
        FillImage.DOColor(color, Random.value).SetEase(Ease.InOutSine);
    }
}
