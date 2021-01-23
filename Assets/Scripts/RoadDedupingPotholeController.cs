using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;

public class RoadDedupingPotholeController : PotholeController
{
    private struct RoadDeduplicationMetadata
    {
        public int pixelSize;
        public int pixelUniqueContribution;
        public bool isBeingKept;
        public Road road;
    }

    [Serializable]
    private struct RoadDeduplicationWrittenData
    {
        public int pixelSize;
        public int pixelUniqueContribution;
        public bool isBeingKept;
        public int roadId;
    }

    private List<LineRenderer> _roadLineRenderers;
    public Camera textureCamera;
    public float ratioThatGetsYouAPass = 0.5f;

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
        //first loop: pixel size, each road alone
        //sort by size desc
        //second loop: pixel size, all roads so far, compared with all roads so far plus the ith road
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
            Debug.Log("Road " + i + " of " + roadsToDedupe.Count + " imaging finished, size: " +
                      dedupeResult[i].pixelSize);
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
            jthRoadDeduplicationMetadata.pixelUniqueContribution =
                (countPixelsNotOfColor(imageAllRoadsPlusJth, Color.white) -
                 countPixelsNotOfColor(imageAllRoads, Color.white));

            jthRoadDeduplicationMetadata.isBeingKept = shouldKeepRoad(
                jthRoadDeduplicationMetadata.pixelUniqueContribution, jthRoadDeduplicationMetadata.pixelSize);
            if (!jthRoadDeduplicationMetadata.isBeingKept)
            {
                dedupeResult[j].road.Render(false);
            }

            dedupeResult[j] = jthRoadDeduplicationMetadata;
            Debug.Log("Road " + j + " of " + dedupeResult.Count + " unique pixels: " +
                      jthRoadDeduplicationMetadata.pixelUniqueContribution + " and total size: " +
                      jthRoadDeduplicationMetadata.pixelSize + ", conclusion: " +
                      (jthRoadDeduplicationMetadata.isBeingKept ? "keeping" : "discarding"));
            yield return null;
        }
        Destroy(imageAllRoads);
        Destroy(imageAllRoadsPlusJth);
        //EvaluateDedupe(dedupeResult);
        PrintRoadIdWhitelist(dedupeResult);
        DestroyTextureCamera();
    }

    private void PrintRoadIdWhitelist(List<RoadDeduplicationMetadata> dedupeResult)
    {
        System.IO.File.WriteAllText("Assets/Dedupes/" + FindObjectOfType<ShapefileImport>().city+".dedupe.json",
            JsonHelper.ToJson(dedupeResult.Select(getCopyForWriting).ToArray(),
                true));
    }

    /*private void EvaluateDedupe(List<RoadDeduplicationMetadata> results)
    {
        int pixelsInDisabledRoads = results.Where(result => !result.isBeingKept).Sum(result => result.pixelSize);
        int pixelsInDedupedImage = countPixelsNotOfColor(dedupedRoadsImage, Color.white);
        enableRoadRenderers(roads);
        Texture2D dupedUpRoadsImage = TakeImageFromCamera(textureCamera);
        int pixelsInDupedImage = countPixelsNotOfColor(dupedUpRoadsImage, Color.white);
        Debug.Log("duped roads total " + pixelsInDupedImage + "pixels; deduped " + pixelsInDedupedImage + "; " +
                  (((float) pixelsInDedupedImage / pixelsInDupedImage) * 100) + "% accuracy");
        Debug.Log(pixelsInDisabledRoads + " pixels' worth of roads disabled; " +
                  (pixelsInDisabledRoads / pixelsInDedupedImage * 100) + "% of final map size");
    }*/

    private void enableRoadRenderers(List<Road> toEnable)
    {
        toEnable.ForEach(road => road.Render(true));
    }

    private void disableRoadRenderers(List<Road> toDisable)
    {
        toDisable.ForEach(road => road.Render(false));
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

    private bool shouldKeepRoad(int uniqueContributionPixels, int totalPixels)
    {
        return uniqueContributionPixels != 0 &&
               (float) uniqueContributionPixels / totalPixels > ratioThatGetsYouAPass;
    }

    private int countPixelsNotOfColor(Texture2D texture, Color color)
    {
        return texture.GetPixels().Count(pixel => pixel != color);
    }

    RoadDeduplicationWrittenData getCopyForWriting(RoadDeduplicationMetadata data)
    {
        var toReturn = new RoadDeduplicationWrittenData();
        toReturn.roadId = data.road.ID;
        toReturn.pixelSize = data.pixelSize;
        toReturn.isBeingKept = data.isBeingKept;
        toReturn.pixelUniqueContribution = data.pixelUniqueContribution;
        return toReturn;
    }

    // Take a "screenshot" of a camera's Render Texture.
    Texture2D TakeImageFromCamera(Camera camera)
    {
        var savedRT = RenderTexture.active;
        RenderTexture.active = camera.targetTexture;

        camera.Render();

        Texture2D image = new Texture2D(camera.targetTexture.width, camera.targetTexture.height, TextureFormat.RGB24,
            false);
        image.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0);
        image.Apply();

        RenderTexture.active = savedRT;
        return image;
    }
}