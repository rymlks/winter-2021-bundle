using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;

namespace DefaultNamespace
{
    public class RoadDedupingPotholeController : PotholeController
    {
        private List<LineRenderer> _roadLineRenderers;
        public Camera textureCamera;

        public override void SortAndSumRoads()
        {
            base.SortAndSumRoads();
            DeDupeRoads();
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Slash))
            {
                DeDupeRoads();
            }
        }

        private void DeDupeRoads()
        {
            this._roadLineRenderers = GameObject.Find("Roads").GetComponentsInChildren<LineRenderer>().ToList();
            InitializeTextureCamera();
            /*for(int i = 0; i < this.roads.Count; i++){
                for (int j = i + 1; j < this.roads.Count; j++)
                {
                    CheckRoadDuplication(this.roads[i], this.roads[j]);
                    if(i*j >= 100) return;
                }
            }*/
            CheckRoadDuplication(roads[2], roads[3]);
            CheckRoadDuplication(roads[0], roads[1]);

            //DestroyTextureCamera();
        }

        private void DestroyTextureCamera()
        {
            Destroy(textureCamera.targetTexture);
            Destroy(textureCamera);
        }

        private RenderTexture InitializeTextureCamera()
        {
            textureCamera.CopyFrom(Camera.main);
            RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            textureCamera.targetTexture = renderTexture;
            return renderTexture;
        }

        private void CheckRoadDuplication(Road road, Road other)
        {
            Texture2D imageRoad = getImageOfRoad(road);
            Debug.Log("Nonwhite pixels in road1 image: " + countPixelsNotOfColor(imageRoad, Color.white) + ", total " + imageRoad.GetPixels().Length);
            Texture2D imageOther = getImageOfRoad(other);
            Debug.Log("Nonwhite pixels in road2 image: " + countPixelsNotOfColor(imageOther, Color.white) + ", total " + imageOther.GetPixels().Length);
            Debug.Log("Number of nonwhite pixels in common: " + countPixelsNotOfColorInCommon(imageRoad, imageOther, Color.white));
            Debug.Log("Recommendation: " + getRoadCollisionRecommendation(countPixelsNotOfColor(imageRoad, Color.white), countPixelsNotOfColor(imageOther, Color.white), countPixelsNotOfColorInCommon(imageRoad, imageOther, Color.white)));
        }

        private int countPixelsNotOfColorInCommon(Texture2D image, Texture2D other, Color exclusionColor)
        {
            int inCommon = 0;
            Color[] imagePixels = image.GetPixels();
            Color[] otherPixels = other.GetPixels();
            for (int i = 0; i < imagePixels.Length; i++)
            {
                if (imagePixels[i] == otherPixels[i] && imagePixels[i] != exclusionColor)
                {
                    inCommon++;
                }
            }

            return inCommon;
        }

        private String getRoadCollisionRecommendation(int relevantPixelCountImageOne, int relevantPixelCountImageTwo, int sizeInCommon)
        {
            if (sizeInCommon <= 0)
            {
                return "NO_ACTION";
            }
            else if (sizeInCommon >= relevantPixelCountImageOne)
            {
                return "REMOVE_FIRST_ROAD";
            }
            else if(sizeInCommon >= relevantPixelCountImageTwo)
            {
                return "REMOVE_SECOND_ROAD";
            }
            else
            {
                //there is overlap but neither road is 100% covered by the other
                return "STUDY_FURTHER";
            }
        }

        private int countPixelsNotOfColor(Texture2D texture, Color color)
        {
            return texture.GetPixels().Count(pixel => pixel != color);
        }

        private Texture2D getImageOfRoad(Road road)
        {
            enableOnlyOneLineRenderer(road);
            textureCamera.Render();
            return TakeImageFromCamera(textureCamera);
        }

        private void enableOnlyOneLineRenderer(Road toRender)
        {
            foreach (LineRenderer lineRenderer in this._roadLineRenderers)
            {
                lineRenderer.enabled = false;
            }

            toRender.GetComponentInChildren<LineRenderer>().enabled = true;
        }
        
        // Take a "screenshot" of a camera's Render Texture.
        Texture2D TakeImageFromCamera(Camera camera)
        {
            var savedRT = RenderTexture.active;
            RenderTexture.active = camera.targetTexture;

            camera.Render();

            Texture2D image = new Texture2D(camera.targetTexture.width, camera.targetTexture.height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
            image.Apply();

            RenderTexture.active = savedRT;
            return image;
        }
    }
}