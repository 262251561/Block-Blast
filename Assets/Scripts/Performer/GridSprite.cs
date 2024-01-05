using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
public class GridSprite : MonoBehaviour
{
    private SpriteRenderer __attachRender;

    private void Awake()
    {
        __attachRender = gameObject.GetComponent<SpriteRenderer>();
    }

    public void ApplySprite(Sprite sp)
    {
        __attachRender.sprite = sp;
    }
}