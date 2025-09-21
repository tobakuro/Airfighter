using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AspectRatioSetter : MonoBehaviour
{

        [SerializeField]
        Camera targetCamera;
        [SerializeField]
        Vector2 targetSize = new(16f, 9f);
        [SerializeField]
        bool autoSetOnStart = true;

        public Camera TargetCamera { get { return targetCamera; } set { targetCamera = value; } }
        public Vector2 TargetSize { get { return targetSize; } set { targetSize = value; } }

        void Awake()
        {
            if (autoSetOnStart)
            {
                SetAspectRatio();
            }
        }

        public void SetAspectRatio()
        {
            if (targetCamera == null)
            {
                return;
            }

            float currentRatio = (float)Screen.width / Screen.height;
            float targetRatio = targetSize.x / targetSize.y;
            float scaleHeight = currentRatio / targetRatio;
            Rect rect = targetCamera.rect;

            if (scaleHeight < 1f)
            {
                rect.width = 1f;
                rect.height = scaleHeight;
                rect.x = 0;
                rect.y = (1f - scaleHeight) / 2f;
            }
            else
            {
                float scaleWidth = 1f / scaleHeight;
                rect.width = scaleWidth;
                rect.height = 1f;
                rect.x = (1f - scaleWidth) / 2f;
                rect.y = 0;
            }

            targetCamera.rect = rect;
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }