using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using IslamicGame.Core;

namespace IslamicGame.Gameplay
{
    public class ClickOrderManager : MonoBehaviour
    {
        [Header("UI Elements")]
        public Transform itemsContainer;
        public GameObject clickableItemPrefab;
        public TextMeshProUGUI instructionText;
        public GameObject successPanel;
        
        [Header("Order Display")]
        public Transform orderDisplayContainer;
        public bool showOrderNumbers = true;
        
        [Header("Settings")]
        public List<ClickableItem> correctOrder = new List<ClickableItem>();
        public float itemSpacing = 150f;
        public Color correctColor = Color.green;
        public Color wrongColor = Color.red;
        public Color highlightColor = Color.yellow;
        
        private List<ClickableItemUI> clickableItems = new List<ClickableItemUI>();
        private List<int> playerClickOrder = new List<int>();
        private int expectedNextIndex = 0;
        private bool isCompleted = false;
        
        void Start()
        {
            SetupClickableItems();
        }
        
        void SetupClickableItems()
        {
            for (int i = 0; i < correctOrder.Count; i++)
            {
                GameObject itemObj = Instantiate(clickableItemPrefab, itemsContainer);
                ClickableItemUI itemUI = itemObj.GetComponent<ClickableItemUI>();
                
                if (itemUI == null)
                    itemUI = itemObj.AddComponent<ClickableItemUI>();
                
                // Setup item data
                itemUI.Setup(correctOrder[i], i);
                itemUI.onItemClicked += OnItemClicked;
                
                // Position items
                RectTransform rect = itemObj.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(i * itemSpacing, 0);
                
                // Animate entrance
                itemObj.transform.localScale = Vector3.zero;
                itemObj.transform.DOScale(1f, 0.5f)
                    .SetEase(Ease.OutBack)
                    .SetDelay(i * 0.1f);
                
                clickableItems.Add(itemUI);
            }
        }
        
        void OnItemClicked(ClickableItemUI item)
        {
            if (isCompleted) return;
            
            if (item.itemIndex == expectedNextIndex)
            {
                // Correct order
                HandleCorrectClick(item);
            }
            else
            {
                // Wrong order
                HandleWrongClick(item);
            }
        }
        
        void HandleCorrectClick(ClickableItemUI item)
        {
            playerClickOrder.Add(item.itemIndex);
            expectedNextIndex++;
            
            // Visual feedback
            item.SetCorrect();
            item.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);
            
            // Show order number if enabled
            if (showOrderNumbers)
            {
                item.ShowOrderNumber(playerClickOrder.Count);
            }
            
            AudioManager.Instance.PlaySound("correct_click");
            
            // Check if completed
            if (expectedNextIndex >= correctOrder.Count)
            {
                CompleteSequence();
            }
        }
        
        void HandleWrongClick(ClickableItemUI item)
        {
            // Visual feedback
            item.SetWrong();
            item.transform.DOShakePosition(0.5f, 10f);
            
            AudioManager.Instance.PlaySound("wrong_click");
            
            // Reset after delay
            DOVirtual.DelayedCall(1f, ResetSequence);
        }
        
        void ResetSequence()
        {
            playerClickOrder.Clear();
            expectedNextIndex = 0;
            
            foreach (var item in clickableItems)
            {
                item.Reset();
            }
            
            // Show instruction
            if (instructionText != null)
            {
                instructionText.text = "Try again! Click in the correct order.";
                instructionText.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f);
            }
        }
        
        void CompleteSequence()
        {
            isCompleted = true;
            
            // Celebration animation
            foreach (var item in clickableItems)
            {
                item.transform.DORotate(new Vector3(0, 0, 360), 1f, RotateMode.FastBeyond360)
                    .SetEase(Ease.OutQuad);
                item.transform.DOScale(1.2f, 0.5f)
                    .SetEase(Ease.OutBounce);
            }
            
            // Show success panel
            if (successPanel != null)
            {
                successPanel.SetActive(true);
                successPanel.transform.localScale = Vector3.zero;
                successPanel.transform.DOScale(1f, 0.5f)
                    .SetEase(Ease.OutBack);
            }
            
            AudioManager.Instance.PlaySound("sequence_complete");
            GameManager.Instance.AddScore(200);
            
            // Proceed to next level after delay
            DOVirtual.DelayedCall(2f, () => {
                GameManager.Instance.ChangeGameState(GameState.Victory);
            });
        }
    }
    
    [System.Serializable]
    public class ClickableItem
    {
        public string itemName;
        public Sprite itemSprite;
        public string arabicText;
        public AudioClip itemSound;
    }
    
    public class ClickableItemUI : MonoBehaviour
    {
        public Image itemImage;
        public TextMeshProUGUI itemText;
        public TextMeshProUGUI orderNumberText;
        public Button button;
        
        public int itemIndex { get; private set; }
        private ClickableItem itemData;
        private Color originalColor;
        
        public delegate void OnItemClicked(ClickableItemUI item);
        public event OnItemClicked onItemClicked;
        
        public void Setup(ClickableItem data, int index)
        {
            itemData = data;
            itemIndex = index;
            
            if (itemImage != null && data.itemSprite != null)
                itemImage.sprite = data.itemSprite;
            
            if (itemText != null)
                itemText.text = data.itemName;
            
            if (button == null)
                button = GetComponent<Button>();
            
            button.onClick.AddListener(() => onItemClicked?.Invoke(this));
            
            originalColor = itemImage.color;
            
            if (orderNumberText != null)
                orderNumberText.gameObject.SetActive(false);
        }
        
        public void SetCorrect()
        {
            itemImage.DOColor(Color.green, 0.3f);
            button.interactable = false;
        }
        
        public void SetWrong()
        {
            itemImage.DOColor(Color.red, 0.3f);
        }
        
        public void Reset()
        {
            itemImage.DOColor(originalColor, 0.3f);
            button.interactable = true;
            if (orderNumberText != null)
                orderNumberText.gameObject.SetActive(false);
        }
        
        public void ShowOrderNumber(int number)
        {
            if (orderNumberText != null)
            {
                orderNumberText.text = number.ToString();
                orderNumberText.gameObject.SetActive(true);
                orderNumberText.transform.localScale = Vector3.zero;
                orderNumberText.transform.DOScale(1f, 0.3f)
                    .SetEase(Ease.OutBack);
            }
        }
    }
}
