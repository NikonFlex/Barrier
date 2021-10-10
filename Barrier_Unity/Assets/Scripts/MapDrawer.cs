using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MapDrawer : MonoBehaviour
{
   // Draws a red triangle using pixels as coordinates to paint on.
   public Material mat;
   Vector3 startVertex;
   Vector3 mousePos;

   public float radius = 200;


   public void OnEnable()
   {
      Debug.Log("OnEnable");
      //RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
      Camera.onPostRender += OnPostRenderCallback;
   }
   public void OnDisable()
   {
      Debug.Log("OnDisable");
      Camera.onPostRender -= OnPostRenderCallback;
      //RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
   }

   void Update()
   {
      mousePos = Input.mousePosition;
      // Press space to update startVertex
      if (Input.GetKeyDown(KeyCode.Space))
      {
         startVertex = new Vector3(mousePos.x / Screen.width, mousePos.y / Screen.height, 0);
      }
   }


   static Material lineMaterial;
   static void CreateLineMaterial()
   {
      if (!lineMaterial)
      {
         // Unity has a built-in shader that is useful for drawing
         // simple colored things.
         Shader shader = Shader.Find("Hidden/Internal-Colored");
         lineMaterial = new Material(shader);
         lineMaterial.hideFlags = HideFlags.HideAndDontSave;
         // Turn on alpha blending
         lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
         lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
         // Turn backface culling off
         lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
         // Turn off depth writes
         lineMaterial.SetInt("_ZWrite", 0);
      }
   }

   void OnPostRenderCallback(Camera cam)
   {
      Debug.Log("Camera callback: Camera name is " + cam.name);
      OnRenderObject();
      //onPostRender();
   }
   private void onPostRender()
   {
      Debug.Log("OnPostRender");
      if (!mat)
      {
         Debug.LogError("Please Assign a material on the inspector");
         return;
      }
      GL.PushMatrix();
      mat.SetPass(0);
      GL.LoadOrtho();
      GL.Begin(GL.QUADS);
     // GL.Color(Color.red);
      GL.Vertex3(0, 0.5f, 0);
      GL.Vertex3(0.5f, 1, 0);
      GL.Vertex3(1, 0.5f, 0);
      GL.Vertex3(0.5f, 0, 0);

      //GL.Color(Color.cyan);
      GL.Vertex3(0, 0, 0);
      GL.Vertex3(0, 0.25f, 0);
      GL.Vertex3(0.25f, 0.25f, 0);
      GL.Vertex3(0.25f, 0, 0);
      GL.End();
      GL.PopMatrix();
   }

   public void OnRenderObject()
   {
      CreateLineMaterial();

      var rectTransform = transform as RectTransform;
      Vector2 size = Vector2.Scale(rectTransform.rect.size, transform.lossyScale);
      Vector2 scale = new Vector2(1f/(float)Screen.width, 1f/ (float)Screen.height);
      float x = transform.position.x + rectTransform.anchoredPosition.x;
      float y = Screen.height - rectTransform.position.y - rectTransform.anchoredPosition.y;

      //var screenRect = new Rect(scale*((Vector2)rectTransform.position - (size * rectTransform.pivot)), scale*size);
      //var screenRect = new Rect(((Vector2)rectTransform.position - (size * rectTransform.pivot)), size);
      var screenRect = new Rect(x, y, size.x, size.y);

      // Apply the line material
      lineMaterial.SetPass(0);

      GL.PushMatrix();
      // Set transformation matrix for drawing to
      // match our transform
      //GL.MultMatrix(transform.localToWorldMatrix);
      //GL.LoadOrtho();
      GL.LoadPixelMatrix();


      // Draw lines
      GL.Begin(GL.LINES);
      int lineCount = 10;
      for (int i = 0; i < lineCount; ++i)
      {
         float a = i / (float)lineCount;
         float angle = a * Mathf.PI * 2;
         // Vertex colors change from red to green
         GL.Color(new Color(a, 1 - a, 0, 0.8F));
         // One vertex at transform position
         GL.Vertex3(screenRect.center.x, screenRect.y, 0);
         // Another vertex at edge of circle
         GL.Vertex3(screenRect.center.x + Mathf.Cos(angle) * radius,
            screenRect.center.y + Mathf.Sin(angle) * radius, 0);
      }

      GL.Begin(GL.QUADS);
      GL.Color(Color.red);
      GL.Vertex3(screenRect.x, screenRect.center.y*0.5f, 0);
      GL.Vertex3(screenRect.x*0.5f, screenRect.y+screenRect.height, 0);
      GL.Vertex3(screenRect.x+ screenRect.width, screenRect.center.y*0.5f, 0);
      GL.Vertex3(screenRect.x*0.5f, screenRect.y, 0);

      GL.End();
      GL.PopMatrix();
   }
}