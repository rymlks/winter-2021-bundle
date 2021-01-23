using System;
using System.Collections;
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

        private struct RoadDeduplicationMetadata
        {
            public int pixelSize;
            public int pixelUniqueContribution;
            public bool isBeingKept;
            public Road road;
        }

        private List<LineRenderer> _roadLineRenderers;
        public Camera textureCamera;

        public override void SortAndSumRoads()
        {
            base.SortAndSumRoads();
            StartCoroutine(DeDupeRoads(this.roads));
        }

        void Update()
        {
        }
        
        private IEnumerator DeDupeRoads(List<Road> roadsToDedupe)
        {
            //first loop: pixel size each road alone
            //second loop: all roads so far, compared with all roads so far plus the ith road
            float ratioThatGetsYouAPass = 0.5f;
            List<RoadDeduplicationMetadata> dedupeResult = new List<RoadDeduplicationMetadata>(roadsToDedupe.Count);
            //pixel size of roads
            disableRoadRenderers(roadsToDedupe);
            InitializeTextureCamera();
            Texture2D imageRoad = Texture2D.whiteTexture;
            for (int i = 0; i < roadsToDedupe.Count; i++)
            {
                roadsToDedupe[i].Render(true);
                imageRoad = TakeImageFromCamera(textureCamera);
                RoadDeduplicationMetadata roadDeduplicationMetadata = new RoadDeduplicationMetadata();
                roadDeduplicationMetadata.pixelSize = countPixelsNotOfColor(imageRoad, Color.white);
                roadDeduplicationMetadata.road = roadsToDedupe[i];
                dedupeResult.Add(roadDeduplicationMetadata);
                roadsToDedupe[i].Render(false);
                Debug.Log("Road " + i + " of " + roadsToDedupe.Count + " imaging finished, size: " + dedupeResult[i].pixelSize);
                yield return null;
            }
            Destroy(imageRoad);
            //unique contribution of each road to the map of unexcluded roads
            dedupeResult.Sort((m1, m2) => m2.pixelSize.CompareTo(m1.pixelSize));
            disableRoadRenderers(roadsToDedupe);
            Texture2D imageAllRoads = Texture2D.whiteTexture;
            Texture2D imageAllRoadsPlusJth = Texture2D.whiteTexture;
            for (int j = 0; j < dedupeResult.Count; j++)
            {
                imageAllRoads = TakeImageFromCamera(textureCamera);
                dedupeResult[j].road.Render(true);
                imageAllRoadsPlusJth = TakeImageFromCamera(textureCamera);
                RoadDeduplicationMetadata jthRoadDeduplicationMetadata = dedupeResult[j];
                jthRoadDeduplicationMetadata.pixelUniqueContribution = (countPixelsNotOfColor(imageAllRoadsPlusJth, Color.white) -
                                                countPixelsNotOfColor(imageAllRoads, Color.white));

                jthRoadDeduplicationMetadata.isBeingKept = (jthRoadDeduplicationMetadata.pixelUniqueContribution != 0 &&
                                  (float) jthRoadDeduplicationMetadata.pixelUniqueContribution / jthRoadDeduplicationMetadata.pixelSize > ratioThatGetsYouAPass);
                if (!jthRoadDeduplicationMetadata.isBeingKept)
                {
                    dedupeResult[j].road.Render(false);
                }
                Debug.Log("Road "+ j + " of " + dedupeResult.Count + " unique pixels: " + jthRoadDeduplicationMetadata.pixelUniqueContribution + " and total size: " + jthRoadDeduplicationMetadata.pixelSize + ", conclusion: " + (jthRoadDeduplicationMetadata.isBeingKept ? "keeping" : "discarding"));
                yield return null;
            }

            EvaluateDedupe(imageAllRoads, dedupeResult);
            PrintRoadIdWhitelist(dedupeResult);
            Destroy(imageAllRoads);
            Destroy(imageAllRoadsPlusJth);
            DestroyTextureCamera();
        }

        private void PrintRoadIdWhitelist(List<RoadDeduplicationMetadata> dedupeResult)
        {
            IEnumerable<int> whitelistedRoadIds = dedupeResult.Where(result => result.isBeingKept).Select(result => result.road.ID);
            string ids = string.Join(",", whitelistedRoadIds);
            Debug.Log(ids);
        }

        private IEnumerator DeDupeRoadsSubtractively(List<Road> roadsToDedupe)
        {
            //first loop: pixel size of roads
            //second loop: all roads so far, compared with all roads so far minus the ith road
            //This subtractive algorithm says 0 unique pixels for every single road.  I believe this means every road has an exact duplicate.
            int sizeThatGetsYouAPass = 100;
            float ratioThatGetsYouAPass = 0.5f;
            
            disableRoadRenderers(roadsToDedupe);
            List<int> roadSizesPixels = new List<int>(roadsToDedupe.Count);
            List<int> roadUniqueContributionsPixels = new List<int>(roadsToDedupe.Count);
            List<bool> isRoadBeingKept = new List<bool>(roadsToDedupe.Count);
            //pixel size of roads
            InitializeTextureCamera();
            Texture2D imageRoad = Texture2D.whiteTexture;
            for (int i = 0; i < roadsToDedupe.Count; i++)
            {
                roadsToDedupe[i].Render(true);
                imageRoad = TakeImageFromCamera(textureCamera);
                roadSizesPixels.Add(countPixelsNotOfColor(imageRoad, Color.white));
                roadsToDedupe[i].Render(false);
                Debug.Log("Road " + i + " of " + roadsToDedupe.Count + " imaging finished, size: " + roadSizesPixels[i]);
                yield return null;
            }
            Destroy(imageRoad);
            //unique contribution of each road to the map of unexcluded roads
            enableRoadRenderers(roadsToDedupe);
            Texture2D imageAllRoads = Texture2D.whiteTexture;
            Texture2D imageAllRoadsMinusJth = Texture2D.whiteTexture;
            for (int j = 0; j < roadsToDedupe.Count; j++)
            {
                imageAllRoads = TakeImageFromCamera(textureCamera);
                roadsToDedupe[j].Render(false);
                imageAllRoadsMinusJth = TakeImageFromCamera(textureCamera);
                roadUniqueContributionsPixels.Add(countPixelsNotOfColor(imageAllRoads, Color.white) -
                                                countPixelsNotOfColor(imageAllRoadsMinusJth, Color.white));
               
                isRoadBeingKept.Add(roadSizesPixels[j] > sizeThatGetsYouAPass || roadUniqueContributionsPixels[j] != 0 &&
                                  (float) roadUniqueContributionsPixels[j] / roadSizesPixels[j] > ratioThatGetsYouAPass);
                if (isRoadBeingKept[j])
                {
                    roadsToDedupe[j].Render(true);
                }
                Debug.Log("Road "+ j + " of " + roadsToDedupe.Count + " unique pixels: " + roadUniqueContributionsPixels[j] + " and total size: " + roadSizesPixels[j] + ", conclusion: " + (isRoadBeingKept[j] ? "keeping" : "discarding"));
                yield return null;
            }

            EvaluateDedupe(imageAllRoads, new List<RoadDeduplicationMetadata>());
            Destroy(imageAllRoads);
            Destroy(imageAllRoadsMinusJth);
            DestroyTextureCamera();
        }

        private void EvaluateDedupe(Texture2D dedupedRoadsImage, List<RoadDeduplicationMetadata> results)
        {
            int pixelsInDisabledRoads = results.Where(result => !result.isBeingKept).Sum(result => result.pixelSize);
            int pixelsInDedupedImage = countPixelsNotOfColor(dedupedRoadsImage, Color.white);
            enableRoadRenderers(roads);
            Texture2D dupedUpRoadsImage = TakeImageFromCamera(textureCamera);
            int pixelsInDupedImage = countPixelsNotOfColor(dupedUpRoadsImage, Color.white);
            Debug.Log("duped roads total " + pixelsInDupedImage + "pixels; deduped " + pixelsInDedupedImage + "; " +
                      (((float) pixelsInDedupedImage / pixelsInDupedImage) * 100) + "% accuracy");
            Debug.Log(pixelsInDisabledRoads + " pixels' worth of roads disabled; " +(pixelsInDisabledRoads / pixelsInDedupedImage * 100) + "% of final map size");


        }

        private void enableRoadRenderers(List<Road> toEnable)
        {
             toEnable.ForEach(road => road.Render(true));
        }

        private void disableRoadRenderers(List<Road> toDisable)
        {
            toDisable.ForEach(road => road.Render(false));
        }

        private IEnumerator DeDupeRoadsNSquared(List<Road> roadsToDedupe)
        {
            this._roadLineRenderers = GameObject.Find("Roads").GetComponentsInChildren<LineRenderer>().ToList();
            InitializeTextureCamera();
            for(int i = 0; i < roadsToDedupe.Count; i++){
                Texture2D imageRoad = getImageOfRoad(roadsToDedupe[i]);
                for (int j = i + 1; j < roadsToDedupe.Count; j++)
                {
                    if (checkBoundsIntersection(roadsToDedupe[i], roadsToDedupe[j]))
                    {
                        Texture2D imageOther = getImageOfRoad(roadsToDedupe[j]);
                        CheckImageCollision(imageRoad, imageOther);
                        Destroy(imageOther);
                    } 
                    yield return null;
                }
                Destroy(imageRoad);
                Debug.Log("Road " + i + " dedupe finished.");
            }
            DestroyTextureCamera();
        }

        private static bool checkBoundsIntersection(Road road, Road other)
        {
            return road.GetBounds().Intersects(other.GetBounds());
            //consider using Colliders instead, since roads come with those
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

        private void CheckImageCollision(Texture2D imageRoad, Texture2D imageOther)
        {
            Debug.Log("Nonwhite pixels in road1 image: " + countPixelsNotOfColor(imageRoad, Color.white) + ", total " + imageRoad.GetPixels().Length);
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