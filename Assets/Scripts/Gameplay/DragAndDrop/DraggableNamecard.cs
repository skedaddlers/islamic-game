using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

namespace IslamicGame.Gameplay
{
    public class DraggableNameCard : DraggableObject
    {
        [Header("Card Elements")]
        public TextMeshProUGUI arabicText;
        public TextMeshProUGUI englishText;
        public Image cardBackground;
        public Image glowEffect;

        public ProphetParentsGame.NameOption nameData { get; private set; }

        public delegate void OnCardDropped(DraggableNameCard card, DropZone zone);
        public event OnCardDropped onDropped;

        public void SetupCard(ProphetParentsGame.NameOption data)
        {
            nameData = data;

            if (arabicText != null)
                arabicText.text = data.arabicName;

            if (englishText != null)
                englishText.text = data.englishName;

            // Set tag based on correct answer
            if (data.isCorrectFather)
                gameObject.tag = "FatherName";
            else if (data.isCorrectMother)
                gameObject.tag = "MotherName";
            else
                gameObject.tag = "WrongName";

            // Add glow effect for hover
            if (glowEffect != null)
            {
                glowEffect.DOFade(0f, 0f);
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            if (canDrag && glowEffect != null)
            {
                glowEffect.DOFade(0.5f, 0.2f);
            }

            // Scale up slightly
            transform.DOScale(1.05f, 0.2f);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            if (glowEffect != null)
            {
                glowEffect.DOFade(0f, 0.2f);
            }

            // Scale back to normal
            transform.DOScale(1f, 0.2f);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);

            // Check if dropped on a zone
            GameObject dropObj = eventData.pointerCurrentRaycast.gameObject;
            if (dropObj != null)
            {
                Debug.Log($"Dropped on {dropObj.name}");
                DropZone zone = dropObj.GetComponent<DropZone>();
                if (zone != null)
                {
                    onDropped?.Invoke(this, zone);
                }
            }
        }   
    }
}