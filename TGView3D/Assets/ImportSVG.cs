using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VectorGraphics;
using UnityEngine;

public static class ImportSVG {


    //public static Sprite[] m_Sprites;

    public static void ImportAsMesh(string svg, ref GameObject go)
    {
       // UnityEngine.XR.XRSettings.enabled = false;
        /*string svg =
            @"<svg width=""283.9"" height=""283.9"" xmlns=""http://www.w3.org/2000/svg"">
                <line x1=""170.3"" y1=""226.99"" x2=""177.38"" y2=""198.64"" fill=""none"" stroke=""#888"" stroke-width=""1""/>
                <line x1=""205.73"" y1=""198.64"" x2=""212.81"" y2=""226.99"" fill=""none"" stroke=""#888"" stroke-width=""1""/>
                <line x1=""212.81"" y1=""226.99"" x2=""219.9"" y2=""255.33"" fill=""none"" stroke=""#888"" stroke-width=""1""/>
                <line x1=""248.25"" y1=""255.33"" x2=""255.33"" y2=""226.99"" fill=""none"" stroke=""#888"" stroke-width=""1""/>
                <path d=""M170.08,226.77c7.09-28.34,35.43-28.34,42.52,0s35.43,28.35,42.52,0"" transform=""translate(0.22 0.22)"" fill=""none"" stroke=""red"" stroke-width=""1.2""/>
                <circle cx=""170.3"" cy=""226.99"" r=""1.2"" fill=""blue"" stroke-width=""0.6""/>
                <circle cx=""212.81"" cy=""226.99"" r=""1.2"" fill=""blue"" stroke-width=""0.6""/>
                <circle cx=""255.33"" cy=""226.99"" r=""1.2"" fill=""blue"" stroke-width=""0.6""/>
                <circle cx=""177.38"" cy=""198.64"" r=""1"" fill=""black"" />
                <circle cx=""205.73"" cy=""198.64"" r=""1"" fill=""black"" />
                <circle cx=""248.25"" cy=""255.33"" r=""1"" fill=""black"" />
                <circle cx=""219.9"" cy=""255.33"" r=""1"" fill=""black"" />
            </svg>";

        */

        var tessOptions = new VectorUtils.TessellationOptions()
        {
            StepDistance = 100.0f,
            MaxCordDeviation = 0.5f,
            MaxTanAngleDeviation = 0.1f,
            SamplingStepSize = 0.01f
        };

        //Pfad zur Datei
        string svgFilePath = Application.dataPath+"/equation.svg";

        StreamReader sr = new StreamReader(svgFilePath);
        string svgText = sr.ReadToEnd();
       // print(svgText);
        sr.Close();
        sr.Dispose();

        // Dynamically import the SVG data, and tessellate the resulting vector scene.
        var sceneInfo = SVGParser.ImportSVG(new StringReader(svg));
        //var sceneInfo = SVGParser.ImportSVG(new StringReader(svgText));

        var geometryList = VectorUtils.TessellateScene(sceneInfo.Scene, tessOptions);

        var svgMesh = new Mesh();

        VectorUtils.FillMesh(svgMesh, geometryList, 100f,false);
        go.GetComponent<MeshFilter>().mesh = svgMesh;
       // go.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Unlit/Vector"));


        /*
        int NrOfLayers = sceneInfo.Scene.Root.Children.Count;

        m_Sprites = new Sprite[NrOfLayers];
        SVGParser.SceneInfo[] m_SIArray = new SVGParser.SceneInfo[NrOfLayers];
        List<VectorUtils.Geometry>[] m_Geoms = new List<VectorUtils.Geometry>[NrOfLayers];

        for (int i = 0; i < NrOfLayers; i++)
        {
            m_SIArray[i] = SVGParser.ImportSVG(new StringReader(svgText));
            int removed = 0;
            for (int c = 0; c < NrOfLayers; c++)
            {
                if (c != i)
                {
                    //print("at " + i + " removing index " + c);
                    m_SIArray[i].Scene.Root.Children.Remove(m_SIArray[i].Scene.Root.Children[c - removed]);
                    removed++;
                }
            }

            var fullBounds = VectorUtils.SceneNodeBounds(sceneInfo.Scene.Root);
            var localBounds = VectorUtils.SceneNodeBounds(sceneInfo.Scene.Root.Children[i]);
            var pivot = localBounds.position - fullBounds.position;

            var localSceneBounds = VectorUtils.SceneNodeBounds(m_SIArray[i].Scene.Root);



            Vector2 position = new Vector2(fullBounds.position.x, fullBounds.position.y);
            // position = new Vector2(fullBounds.center.x / fullBounds.width - (localBounds.position.x) / fullBounds.width, fullBounds.center.y / fullBounds.height - localBounds.position.y / fullBounds.height);
            //position = new Vector2((fullBounds.center.x - localBounds.position.x )/ fullBounds.width, 0);
            //position = new Vector2((fullBounds.position.x-localBounds.position.x-pivot.x) / fullBounds.width , 0);
            position = new Vector2(0, 0);

            print("FullBounds: " + fullBounds + " localBounds:" + localBounds);// + " localSceneBounds:"+ localSceneBounds);
            //print(i + ": " + position+" / pivot: "+pivot);

            //print(position.x * fullBounds.width + " , " + position.y * fullBounds.height);

            m_Geoms[i] = VectorUtils.TessellateScene(m_SIArray[i].Scene, tessOptions);
            m_Sprites[i] = VectorUtils.BuildSprite(m_Geoms[i], 1000.0f, VectorUtils.Alignment.TopLeft, position, 128, true);

            GameObject go = new GameObject();
            SpriteRenderer s = go.AddComponent<SpriteRenderer>();
            go.transform.parent = transform;
            // go.transform.position = new Vector3((localSceneBounds.x - fullBounds.width/2f)/1000f, (fullBounds.y + fullBounds.height/2f - localSceneBounds.y) /1000f , 0);
            go.transform.position = new Vector3((localBounds.x) / 1000f, (fullBounds.y - localBounds.y) / 1000f, 0);
            s.sprite = m_Sprites[i];
        }
        */


        /*

        for (int i = 0; i < sceneInfo.Scene.Root.Children.Count; i++)
        {
            print("ChildList: " + i + " " + sceneInfo.Scene.Root.Children[i]);

            SceneNode activeNode = sceneInfo.Scene.Root.Children[i];
            while (activeNode.Drawables==null)
            {
                activeNode = activeNode.Children[0];
            }


            for (int d = 0; d < activeNode.Drawables.Count; d++)
            { 
                IDrawable element = activeNode.Drawables[d];
                //print(element.GetType());
                if (element.GetType()== typeof(Unity.VectorGraphics.Path))
                {
                    Unity.VectorGraphics.Path p = (Unity.VectorGraphics.Path) element;
                    print(p.PathProps.Stroke.Color);
                }
                if (element.GetType() == typeof(Unity.VectorGraphics.Shape))
                {
                    Unity.VectorGraphics.Shape s = (Unity.VectorGraphics.Shape)element;

                    SolidFill sf = (SolidFill)s.Fill;
                    print(sf.Color);
                }

                print("ChildList: " + i + " " + activeNode.Drawables[d]);
            }


        }*/

        /*
        var geoms = VectorUtils.TessellateScene(sceneInfo.Scene, tessOptions);

        // Build a sprite with the tessellated geometry.
        var sprite = VectorUtils.BuildSprite(geoms, 1000.0f, VectorUtils.Alignment.TopLeft, Vector2.zero, 128, true);
        GetComponent<SpriteRenderer>().sprite = sprite;
        */
    }
    /*
    void OnDisable()
    {
      //  GameObject.Destroy(GetComponent<SpriteRenderer>().sprite);
    }

    // Update is called once per frame
    void Update ()
    {
		
	}*/
}
