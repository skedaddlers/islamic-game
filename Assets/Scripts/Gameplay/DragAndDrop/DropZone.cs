using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace IslamicGame.Gameplay
{
    public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Drop Settings")]
        public string acceptedTag = "";
        public int acceptedId = -1;
        public bool onlyOneObject = true;

        [Header("Visual Feedback")]
        public bool highlightOnHover = true;
        public Color hoverColor = new Color(1f, 1f, 0f, 0.5f);
        public float hoverScale = 1.05f;

        private DraggableObject currentObject;
        private Color originalColor;
        private Vector3 originalScale;
        private UnityEngine.UI.Image image;

        public delegate void OnDropEvent(DraggableObject droppedObject);
        public event OnDropEvent onObjectDropped;

        void Start()
        {
            image = GetComponent<UnityEngine.UI.Image>();
            if (image != null)
                originalColor = image.color;
            originalScale = transform.localScale;
        }

        public bool CanAcceptDrop(DraggableObject dragObject)
        {
            // Check if zone already has an object
            if (onlyOneObject && currentObject != null)
                return false;

            // // Check tag
            // if (!string.IsNullOrEmpty(acceptedTag) && !dragObject.CompareTag(acceptedTag))
            //     return false;

            // Check ID (if using ID system)
            if (acceptedId >= 0)
            {
                var idComponent = dragObject.GetComponent<ObjectID>();
                if (idComponent == null || idComponent.id != acceptedId)
                    return false;
            }

            return true;
        }

        public void OnDrop(PointerEventData eventData)
        {
            // Handled in DraggableObject
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!highlightOnHover) return;

            // Check if dragging
            if (eventData.pointerDrag != null)
            {
                DraggableObject dragObject = eventData.pointerDrag.GetComponent<DraggableObject>();
                if (dragObject != null && CanAcceptDrop(dragObject))
                {
                    // Show valid drop feedback
                    if (image != null)
                        image.DOColor(hoverColor, 0.2f);
                    transform.DOScale(originalScale * hoverScale, 0.2f);
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!highlightOnHover) return;

            // Reset visual feedback
            if (image != null)
                image.DOColor(originalColor, 0.2f);
            transform.DOScale(originalScale, 0.2f);
        }

        public void OnObjectDropped(DraggableObject droppedObject)
        {
            if (currentObject != null && currentObject != droppedObject)
            {
                currentObject.ReturnToOriginal();
            }

            currentObject = droppedObject;
            onObjectDropped?.Invoke(droppedObject);
        }

        public void RemoveObject()
        {
            currentObject = null;
        }

        void CheckPuzzleCompletion()
        {
            // Override in specific implementations
        }
        public DraggableObject GetCurrentObject()
        {
            return currentObject;
        }

        public bool HasObject()
        {
            return currentObject != null;
        }
    }


    

    
    
    // Helper component for ID-based matching
    public class ObjectID : MonoBehaviour
    {
        public int id;
    }
}