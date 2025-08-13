using UnityEngine;

namespace IslamicGame.Environment
{
    public class ParallaxBackground : MonoBehaviour
    {
        [Header("Parallax Layers")]
        public Transform[] backgroundLayers;
        public float[] parallaxScales;
        public bool smoothing = true;
        
        [Header("Auto Scroll")]
        public bool autoScroll = false;
        public float autoScrollSpeed = 1f;
        
        private Transform cam;
        private Vector3 previousCamPos;
        
        void Start()
        {
            cam = Camera.main.transform;
            previousCamPos = cam.position;
            
            // Ensure we have parallax scales for each layer
            if (parallaxScales.Length != backgroundLayers.Length)
            {
                parallaxScales = new float[backgroundLayers.Length];
                for (int i = 0; i < parallaxScales.Length; i++)
                {
                    parallaxScales[i] = i * 0.5f;
                }
            }
        }
        
        void Update()
        {
            if (autoScroll)
            {
                AutoScrollBackground();
            }
            else
            {
                UpdateParallax();
            }
        }
        
        void UpdateParallax()
        {
            for (int i = 0; i < backgroundLayers.Length; i++)
            {
                float parallax = (previousCamPos.x - cam.position.x) * parallaxScales[i];
                float backgroundTargetPosX = backgroundLayers[i].position.x + parallax;
                
                Vector3 backgroundTargetPos = new Vector3(
                    backgroundTargetPosX,
                    backgroundLayers[i].position.y,
                    backgroundLayers[i].position.z
                );
                
                if (smoothing)
                {
                    backgroundLayers[i].position = Vector3.Lerp(
                        backgroundLayers[i].position,
                        backgroundTargetPos,
                        Time.deltaTime * 10f
                    );
                }
                else
                {
                    backgroundLayers[i].position = backgroundTargetPos;
                }
            }
            
            previousCamPos = cam.position;
        }
        
        void AutoScrollBackground()
        {
            for (int i = 0; i < backgroundLayers.Length; i++)
            {
                float scrollSpeed = autoScrollSpeed * (1f - parallaxScales[i]);
                backgroundLayers[i].position += Vector3.left * scrollSpeed * Time.deltaTime;
                
                // Reset position for infinite scrolling
                if (backgroundLayers[i].position.x < -20f) // Adjust based on your background width
                {
                    backgroundLayers[i].position = new Vector3(
                        20f,
                        backgroundLayers[i].position.y,
                        backgroundLayers[i].position.z
                    );
                }
            }
        }
    }
}
