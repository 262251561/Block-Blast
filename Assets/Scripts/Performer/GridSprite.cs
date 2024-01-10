using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
public class GridSprite : MonoBehaviour
{
    private Sprite __attachedSprite;
    private Vector3 __localScale;
    private SpriteRenderer __attachRender;

    public Vector3 sourceScale => __localScale;

    private void Awake()
    {
        __attachRender = gameObject.GetComponent<SpriteRenderer>();
        __localScale = transform.localScale;
    }

    private void OnEnable()
    {
        ApplyAlpha(1.0f);
        transform.localScale = __localScale;
    }

    public void ApplyAlpha(float alpha)
    {
        var srcColor = __attachRender.color;
        srcColor.a = alpha;
        __attachRender.color = srcColor;
    }

    public void ApplyTempSprite(Sprite sp)
    {
        __attachRender.sprite = sp;
    }

    public void RestoreSprite()
    {
        __attachRender.sprite = __attachedSprite;
    }

    public void ApplySprite(Sprite sp)
    {
        __attachRender.sprite = sp;
        __attachedSprite = sp;
    }
}