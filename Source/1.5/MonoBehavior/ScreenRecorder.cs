using UnityEngine;
using System.Collections;
using System.IO;
using Verse;

namespace aRandomKiwi.ARS
{

    public class ScreenRecorder : MonoBehaviour
    {
        public static bool wantScreenShot = false;
        public static string saveName = "";
        public static bool screenshotSaved = false;

        // 4k = 3840 x 2160   1080p = 1920 x 1080
        public int captureWidth = 1920;
        public int captureHeight = 1080;

        // optional game object to hide during screenshots (usually your scene canvas hud)
        public GameObject hideGameObject;

        // optimize for many screenshots will not destroy any objects so future screenshots will be fast
        public bool optimizeForManyScreenshots = true;

        // configure with raw, jpg, png, or ppm (simple raw format)
        public enum Format { RAW, JPG, PNG, PPM };
        public Format format = Format.JPG;

        // folder to write output (defaults to data path)
        public string folder;

        // private vars for screenshot
        private Rect rect;
        private RenderTexture renderTexture;
        private Texture2D screenShot;

        // commands
        private bool captureScreenshot = false;

        // Renvois le chemin complet vers l'image de preview associée au nom de save
        private string getPath(string fn)
        {
            string ret = Utils.getBasePathRSPreviews();

            ret = Path.Combine(ret, fn) + ".jpg";

            return ret;
        }

        public void CaptureScreenshot()
        {
            captureScreenshot = true;
        }

        void Update()
        {
            if (wantScreenShot)
            {
                screenshotSaved = false;
                wantScreenShot = false;

                // hide optional game object if set
                if (hideGameObject != null) hideGameObject.SetActive(false);

                // create screenshot objects if needed
                if (renderTexture == null)
                {
                    // creates off-screen render texture that can rendered into
                    rect = new Rect(0, 0, captureWidth, captureHeight);
                    renderTexture = new RenderTexture(captureWidth, captureHeight, 24);
                    screenShot = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
                }

                // get main camera and manually render scene into rt
                Camera camera = this.GetComponent<Camera>(); // NOTE: added because there was no reference to camera in original script; must add this script to Camera
                camera.targetTexture = renderTexture;
                camera.Render();

                // read pixels will read from the currently active render texture so make our offscreen 
                // render texture active and then read the pixels
                RenderTexture.active = renderTexture;
                screenShot.ReadPixels(rect, 0, 0);

                // reset active camera texture and render texture
                camera.targetTexture = null;
                RenderTexture.active = null;

                // get our unique filename
                string filename = getPath(saveName);

                // pull in our file header/data bytes for the specified image format (has to be done from main thread)
                byte[] fileHeader = null;
                byte[] fileData = null;
                if (format == Format.RAW)
                {
                    fileData = screenShot.GetRawTextureData();
                }
                else if (format == Format.PNG)
                {
                    fileData = screenShot.EncodeToPNG();
                }
                else if (format == Format.JPG)
                {
                    fileData = screenShot.EncodeToJPG();
                }
                else // ppm
                {
                    // create a file header for ppm formatted file
                    string headerStr = string.Format("P6\n{0} {1}\n255\n", rect.width, rect.height);
                    fileHeader = System.Text.Encoding.ASCII.GetBytes(headerStr);
                    fileData = screenShot.GetRawTextureData();
                }

                // create new thread to save the image to file (only operation that can be done in background)
                new System.Threading.Thread(() =>
                {
                    // create file and write optional header with image bytes
                    var f = System.IO.File.Create(filename);
                    if (fileHeader != null) f.Write(fileHeader, 0, fileHeader.Length);
                    f.Write(fileData, 0, fileData.Length);
                    f.Close();

                    screenshotSaved = true;

                    //Purge du cache
                    string p = getPath(saveName);
                    if (p != null && Utils.cachedPreviews.ContainsKey(p))
                        Utils.cachedPreviews.Remove(p);

                    Debug.Log(string.Format("Wrote screenshot {0} of size {1}", filename, fileData.Length));
                }).Start();

                // unhide optional game object if set
                if (hideGameObject != null) hideGameObject.SetActive(true);

                // cleanup if needed
                if (optimizeForManyScreenshots == false)
                {
                    Destroy(renderTexture);
                    renderTexture = null;
                    screenShot = null;
                }
            }
        }
    }
}