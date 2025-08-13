using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using IslamicGame.Core;

namespace IslamicGame.Gameplay
{
    public class DraggableObject : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Drag Settings")]
        public bool canDrag = true;
        public bool returnToOriginalPosition = true;
        public float returnDuration = 0.3f;

        [Header("Visual Feedback")]
        public bool scaleOnDrag = true;
        public float dragScale = 1.1f;
        public bool changeColorOnDrag = false;
        public Color dragColor = Color.yellow;


        [SerializeField]
        protected Vector3 originalPosition { get; set; }
        [SerializeField]
        protected Vector3 originalScale { get; set; }
        private Color originalColor;
        private Canvas canvas;
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Transform originalParent;
        private DropZone currentDropZone;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            var image = GetComponent<UnityEngine.UI.Image>();
            if (image != null)
                originalColor = image.color;

            originalParent = transform.parent;
        }

        public void Initialize(Vector3 startPosition, Vector3 startScale)
        {
            originalPosition = startPosition;
            originalScale = startScale;
            transform.localPosition = startPosition;
            transform.localScale = startScale;

            // Debug.Log($"Initialized DraggableObject at position {startPosition} with scale {startScale}");
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (!canDrag || IsDragging()) return;
            transform.DOScale(originalScale * dragScale, 0.2f);
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (!canDrag || IsDragging()) return;
            transform.DOScale(originalScale, 0.2f);
        }


        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!canDrag) return;

            canvasGroup.blocksRaycasts = false;

            if (currentDropZone != null)
            {
                currentDropZone.RemoveObject();
                currentDropZone = null;
            }

            transform.SetParent(canvas.transform, true);


            // Visual feedback
            if (scaleOnDrag)
                transform.DOScale(originalScale * dragScale, 0.2f);

            if (changeColorOnDrag)
            {
                var image = GetComponent<UnityEngine.UI.Image>();
                if (image != null)
                    image.DOColor(dragColor, 0.2f);
            }

            // Play drag sound
            AudioManager.Instance.PlaySound("drag_start");
        }


        public void OnDrag(PointerEventData eventData)
        {
            if (!canDrag) return;

            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (!canDrag) return;

            canvasGroup.blocksRaycasts = true;

            // Check if dropped on a valid zone
            GameObject dropObject = eventData.pointerCurrentRaycast.gameObject;
            DropZone dropZone = (dropObject != null) ? dropObject.GetComponent<DropZone>() : null;

            if (dropObject != null && dropZone != null && dropZone.CanAcceptDrop(this))
            {
                if (dropZone != null && dropZone.CanAcceptDrop(this))
                {
                    // Successful drop
                    currentDropZone = dropZone;
                    dropZone.OnObjectDropped(this);

                    transform.SetParent(dropZone.transform, true);

                    // Snap to drop zone
                    rectTransform.DOAnchorPos(Vector2.zero, 0.3f).SetEase(Ease.OutBack);

                    AudioManager.Instance.PlaySound("drop_success");
                }
                else
                {
                    // Invalid drop
                    ReturnToOriginal();
                    AudioManager.Instance.PlaySound("drop_fail");
                }

                transform.DOScale(originalScale, 0.2f);
            }
            else if (returnToOriginalPosition)
            {
                ReturnToOriginal();
            }

            // Reset visual feedback
            transform.DOScale(originalScale, 0.2f);

            if (changeColorOnDrag)
            {
                var image = GetComponent<UnityEngine.UI.Image>();
                if (image != null)
                    image.DOColor(originalColor, 0.2f);
            }
        }

        public void ReturnToOriginal()
        {
            currentDropZone = null;
            transform.SetParent(originalParent, true);
            // Kembalikan POSISI dan SKALA secara bersamaan
            rectTransform.DOAnchorPos(originalPosition, returnDuration).SetEase(Ease.OutBack);
            transform.DOScale(originalScale, returnDuration).SetEase(Ease.OutBack);
            // Debug.Log($"Returning to original position {originalPosition} and scale {originalScale}");
        }

        public void SetDraggable(bool draggable)
        {
            canDrag = draggable;
        }

        public void ResetPosition()
        {
            rectTransform.anchoredPosition = originalPosition;
            transform.localScale = originalScale;
            currentDropZone = null;
        }

        private bool IsDragging()
        {
            return canvasGroup.blocksRaycasts == false;
        }
        
        public DropZone GetCurrentDropZone()
        {
            return currentDropZone;
        }
    }
}