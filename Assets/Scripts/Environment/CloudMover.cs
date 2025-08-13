// using UnityEngine;
// using DG.Tweening;

// namespace IslamicGame.Environment
// {
//     public class CloudMover : MonoBehaviour
//     {
//         [Header("Movement Settings")]
//         public float moveSpeed = 1f;
//         public float moveDistance = 10f;
//         public bool moveHorizontally = true;
//         public bool loopMovement = true;

//         [Header("Animation Settings")]
//         public bool randomizeSpeed = true;
//         public float minSpeed = 0.5f;
//         public float maxSpeed = 2f;
//         public bool fadeAtEdges = true;
//         public float fadeDuration = 1f;

//         private Vector3 startPosition;
//         private SpriteRenderer spriteRenderer;
//         private float actualSpeed;

//         void Start()
//         {
//             startPosition = transform.position;
//             spriteRenderer = GetComponent<SpriteRenderer>();

//             if (randomizeSpeed)
//                 actualSpeed = Random.Range(minSpeed, maxSpeed);
//             else
//                 actualSpeed = moveSpeed;

//             StartMovement();
//         }

//         void StartMovement()
//         {
//             if (loopMovement)
//             {
//                 MoveLoop();
//             }
//             else
//             {
//                 MovePingPong();
//             }
//         }

//         void MoveLoop()
//         {
//             Vector3 endPosition = moveHorizontally ?
//                 new Vector3(startPosition.x + moveDistance, startPosition.y, startPosition.z) :
//                 new Vector3(startPosition.x, startPosition.y + moveDistance, startPosition.z);

//             float duration = moveDistance / moveSpeed;

//             // Move to end position
//             transform.DOMove(endPosition, duration)
//                 .SetEase(Ease.Linear)
//                 .OnComplete(() =>
//                 {
//                     // Reset position instantly
//                     transform.position = startPosition;
//                     // Start again
//                     MoveLoop();
//                 });

//             if (fadeAtEdges && spriteRenderer != null)
//             {
//                 // Fade in at start
//                 spriteRenderer.DOFade(1f, fadeDuration);

//                 // Fade out near end
//                 DOVirtual.DelayedCall(duration - fadeDuration, () =>
//                 {
//                     spriteRenderer.DOFade(0f, fadeDuration);
//                 });
//             }
//         }

//         void MovePingPong()
//         {
//             Vector3 endPosition = moveHorizontally ?
//                 new Vector3(startPosition.x + moveDistance, startPosition.y, startPosition.z) :
//                 new Vector3(startPosition.x, startPosition.y + moveDistance, startPosition.z);

//             float duration = moveDistance / actualSpeed;

//             // Create ping-pong movement
//             Sequence sequence = DOTween.Sequence();
//             sequence.Append(transform.DOMove(endPosition, duration).SetEase(Ease.InOutSine));
//             sequence.Append(transform.DOMove(startPosition, duration).SetEase(Ease.InOutSine));
//             sequence.SetLoops(-1);
//         }

//         public void SetSpeed(float newSpeed)
//         {
//             actualSpeed = newSpeed;
//             DOTween.Kill(transform);
//             StartMovement();
//         }

//         void OnDestroy()
//         {
//             DOTween.Kill(transform);
//         }
//     }
// }

using UnityEngine;
using DG.Tweening;

namespace IslamicGame.Environment
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class CloudMover : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 1f;
        // 'moveDistance' tidak lagi digunakan untuk 'loopMovement', karena sekarang pergerakannya berkelanjutan.
        // public float moveDistance = 10f; 
        public bool moveHorizontally = true;
        public bool loopMovement = true;
        
        [Header("Animation Settings")]
        public bool randomizeSpeed = true;
        public float minSpeed = 0.5f;
        public float maxSpeed = 2f;
        // Opsi 'fadeAtEdges' diubah menjadi 'fadeInOnStart' agar lebih cocok untuk loop yang seamless.
        public bool fadeInOnStart = true;
        public float fadeDuration = 1f;
        
        private SpriteRenderer spriteRenderer;
        private float actualSpeed;
        
        // Variabel untuk seamless loop
        private Camera mainCamera;
        private Vector2 screenBounds;
        private float objectWidth;
        private float objectHeight;
        
        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            mainCamera = Camera.main;

            // Menghitung batas layar dalam world units
            screenBounds = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, mainCamera.transform.position.z));
            
            // Mendapatkan ukuran sprite untuk memastikan ia benar-benar di luar layar sebelum berpindah posisi
            objectWidth = spriteRenderer.bounds.extents.x; // extents adalah setengah dari ukuran bounds
            objectHeight = spriteRenderer.bounds.extents.y;
            
            if (randomizeSpeed)
                actualSpeed = Random.Range(minSpeed, maxSpeed);
            else
                actualSpeed = moveSpeed;

            // Jika tidak dalam mode loop, gunakan logika PingPong yang lama
            if (!loopMovement)
            {
                // Anda mungkin perlu menambahkan kembali 'moveDistance' jika menggunakan mode ini
                float moveDistance = 10f; 
                MovePingPong(moveDistance);
            }

            // Opsi fade-in saat pertama kali muncul
            if (fadeInOnStart)
            {
                var color = spriteRenderer.color;
                color.a = 0;
                spriteRenderer.color = color;
                spriteRenderer.DOFade(1, fadeDuration);
            }
        }

        void Update()
        {
            // Jalankan pergerakan wrapping hanya jika loopMovement aktif
            if (loopMovement)
            {
                MoveAndWrap();
            }
        }

        void MoveAndWrap()
        {
            if (moveHorizontally)
            {
                // Gerakkan objek ke kanan (atau ke arah positif sumbu x)
                transform.Translate(Vector3.right * actualSpeed * Time.deltaTime);

                // Periksa apakah objek sudah melewati batas kanan layar
                if (transform.position.x > screenBounds.x + objectWidth)
                {
                    // Jika ya, pindahkan posisinya ke sisi kiri layar
                    transform.position = new Vector3(-screenBounds.x - objectWidth, transform.position.y, transform.position.z);
                }
            }
            else // Pergerakan vertikal
            {
                // Gerakkan objek ke atas (atau ke arah positif sumbu y)
                transform.Translate(Vector3.up * actualSpeed * Time.deltaTime);

                // Periksa apakah objek sudah melewati batas atas layar
                if (transform.position.y > screenBounds.y + objectHeight)
                {
                    // Jika ya, pindahkan posisinya ke sisi bawah layar
                    transform.position = new Vector3(transform.position.x, -screenBounds.y - objectHeight, transform.position.z);
                }
            }
        }
        
        void MovePingPong(float moveDistance)
        {
            Vector3 startPosition = transform.position;
            Vector3 endPosition = moveHorizontally ? 
                new Vector3(startPosition.x + moveDistance, startPosition.y, startPosition.z) :
                new Vector3(startPosition.x, startPosition.y + moveDistance, startPosition.z);
            
            float duration = moveDistance / actualSpeed;
            
            // Buat sekuens pergerakan bolak-balik (ping-pong)
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOMove(endPosition, duration).SetEase(Ease.InOutSine));
            sequence.Append(transform.DOMove(startPosition, duration).SetEase(Ease.InOutSine));
            sequence.SetLoops(-1);
        }
        
        public void SetSpeed(float newSpeed)
        {
            actualSpeed = newSpeed;
            // Walaupun tidak lagi menggunakan tween untuk loop, tetap ada baiknya mematikan tween
            // jika beralih antara mode loop dan ping-pong saat runtime.
            DOTween.Kill(transform);

            if (!loopMovement)
            {
                // Anda perlu menambahkan kembali 'moveDistance' jika menggunakan mode ini
                float moveDistance = 10f;
                MovePingPong(moveDistance);
            }
        }
        
        void OnDestroy()
        {
            // Praktik yang baik untuk mematikan semua tween saat objek dihancurkan
            DOTween.Kill(transform);
        }
    }
}
