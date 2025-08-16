using UnityEngine;
using UnityEngine.UI; // Namespace untuk komponen UI
using DG.Tweening;

namespace IslamicGame.Environment
{
    [RequireComponent(typeof(Image))]
    public class CloudMoverUI : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 1f;
        public bool moveHorizontally = true;
        public bool loopMovement = true;

        [Header("Animation Settings")]
        public bool randomizeSpeed = true;
        public float minSpeed = 0.5f;
        public float maxSpeed = 2f;
        public bool fadeInOnStart = true;
        public float fadeDuration = 1f;

        private Image uiImage;
        private RectTransform rectTransform;
        private float actualSpeed;

        // Variabel untuk seamless loop pada UI
        private RectTransform parentRectTransform;
        private float objectWidth;
        private float objectHeight;

        void Start()
        {
            uiImage = GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();
            // Dapatkan RectTransform dari parent untuk mengetahui batas pergerakan
            parentRectTransform = transform.parent.GetComponent<RectTransform>();

            // Mendapatkan ukuran dari RectTransform
            objectWidth = rectTransform.rect.width;
            objectHeight = rectTransform.rect.height;

            if (randomizeSpeed)
                actualSpeed = Random.Range(minSpeed, maxSpeed);
            else
                actualSpeed = moveSpeed;

            if (!loopMovement)
            {
                // Anda mungkin perlu menyesuaikan 'moveDistance' untuk UI
                float moveDistance = 400f; // Contoh jarak dalam pixel
                MovePingPong(moveDistance);
            }

            if (fadeInOnStart)
            {
                // Menggunakan DOFade pada Image UI. [1, 2, 6, 9]
                uiImage.canvasRenderer.SetAlpha(0f);
                uiImage.CrossFadeAlpha(1, fadeDuration, false);
            }
        }

        void Update()
        {
            if (loopMovement)
            {
                MoveAndWrap();
            }
        }

        void MoveAndWrap()
        {
            float parentWidth = parentRectTransform.rect.width;
            float parentHeight = parentRectTransform.rect.height;

            if (moveHorizontally)
            {
                // Gerakkan objek ke kanan menggunakan anchoredPosition
                rectTransform.anchoredPosition += Vector2.right * actualSpeed * Time.deltaTime * 100f; // Dikalikan 100 untuk kecepatan yang lebih sesuai di UI

                // Periksa apakah objek sudah melewati batas kanan parent
                // Posisi dihitung dari pivot, jadi kita perlu mempertimbangkan lebar objek
                if (rectTransform.anchoredPosition.x - (objectWidth * rectTransform.pivot.x) > parentWidth / 2)
                {
                    // Pindahkan ke sisi kiri parent
                    rectTransform.anchoredPosition = new Vector2(-(parentWidth / 2) - (objectWidth * (1 - rectTransform.pivot.x)), rectTransform.anchoredPosition.y);
                }
            }
            else // Pergerakan vertikal
            {
                // Gerakkan objek ke atas
                rectTransform.anchoredPosition += Vector2.up * actualSpeed * Time.deltaTime * 100f;

                // Periksa apakah objek sudah melewati batas atas parent
                if (rectTransform.anchoredPosition.y - (objectHeight * rectTransform.pivot.y) > parentHeight / 2)
                {
                    // Pindahkan ke sisi bawah parent
                    rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -(parentHeight / 2) - (objectHeight * (1 - rectTransform.pivot.y)));
                }
            }
        }

        void MovePingPong(float moveDistance)
        {
            Vector2 startPosition = rectTransform.anchoredPosition;
            Vector2 endPosition = moveHorizontally ?
                new Vector2(startPosition.x + moveDistance, startPosition.y) :
                new Vector2(startPosition.x, startPosition.y + moveDistance);

            float duration = moveDistance / (actualSpeed * 100f);

            // Menggunakan DOAnchorPos untuk menggerakkan RectTransform.
            Sequence sequence = DOTween.Sequence();
            sequence.Append(rectTransform.DOAnchorPos(endPosition, duration).SetEase(Ease.InOutSine));
            sequence.Append(rectTransform.DOAnchorPos(startPosition, duration).SetEase(Ease.InOutSine));
            sequence.SetLoops(-1);
        }

        public void SetSpeed(float newSpeed)
        {
            actualSpeed = newSpeed;
            DOTween.Kill(transform);

            if (!loopMovement)
            {
                float moveDistance = 400f;
                MovePingPong(moveDistance);
            }
        }

        void OnDestroy()
        {
            DOTween.Kill(transform);
        }
    }
}