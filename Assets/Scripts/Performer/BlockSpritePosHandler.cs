using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.EventSystems;

public class BlockSpritePosHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public interface IInput
    {
        void OnBeginDrag(BlockSprite bs, Vector2 pos);
        void OnDrag(BlockSprite bs, Vector2 pos);
        void OnEndDrag(BlockSprite bs, Vector2 pos);
    }

    private IInput __input;
    private bool __isMoved;

    public BlockSprite attachedSprite
    {
        private set;
        get;
    }

    void OnEnable()
    {
        __isMoved = false;
    }

    public void SetAttachedSprite(BlockSprite bs)
    {
        attachedSprite = bs;
        
        bs.transform.SetParent(transform, false);
        bs.transform.localPosition = Vector3.zero;
    }

    public void ApplyInput(IInput input)
    {
        __input = input;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (attachedSprite == null)
            return;

        __isMoved = true;

        if (__input != null)
            __input.OnBeginDrag(attachedSprite, eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (attachedSprite == null)
            return;

        __isMoved = false;

        if (__input != null)
            __input.OnEndDrag(attachedSprite, eventData.position);
    }

    void Update()
    {
        if (__isMoved && __input != null)
            __input.OnDrag(attachedSprite, Input.mousePosition );
    }
}