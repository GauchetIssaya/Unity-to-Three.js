﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;


#endif


namespace UniGLTF
{

    public class gltfExporter : IDisposable
    {

        const string CONVERT_HUMANOID_KEY = UniGLTFVersion.UNIGLTF_VERSION + "/Export";

#if UNITY_EDITOR
        // [MenuItem(CONVERT_HUMANOID_KEY, true, 1)]
        private static bool ExportValidate()
        {

            return Selection.activeObject != null && Selection.activeObject is GameObject;
        }

        //[MenuItem(CONVERT_HUMANOID_KEY, false, 1)]
        /*private static void ExportFromMenu()
        {
            var go = Selection.activeObject as GameObject;
            var path = EditorUtility.SaveFilePanel(
                    "Save glb",
                    "",
                    go.name + ".glb",
                    "glb");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
Debug.Log(path);
            var gltf = new glTF();
            using (var exporter = new gltfExporter(gltf))
            {
                exporter.Prepare(go);
                exporter.Export();
            }
            var bytes = gltf.ToGlbBytes();
            File.WriteAllBytes(path, bytes);

            if (path.StartsWithUnityAssetPath())
            {
                AssetDatabase.ImportAsset(path.ToUnityRelativePath());
                AssetDatabase.Refresh();
            }
        }*/
#endif

        glTF glTF;

        public bool UseSparseAccessorForBlendShape { get; set; }

        public GameObject Copy { get; protected set; }

        public struct MeshWithRenderer
        {
            public Mesh Mesh;
            public Renderer Rendererer;
        }

        public List<Mesh> Meshes { get; private set; }

        public List<Transform> Nodes { get; private set; }

        public List<Material> Materials { get; private set; }

        public TextureExportManager TextureManager;

        protected virtual IMaterialExporter CreateMaterialExporter()
        {
            return new MaterialExporter();
        }

        public gltfExporter(glTF gltf)
        {
            glTF = gltf;

            glTF.asset = new glTFAssets
            {
                generator = "UniGLTF-" + UniGLTFVersion.VERSION,
                version = "2.0",
            };
        }

        /*public static glTF Export(GameObject go)
        {
            var gltf = new glTF();
            using (var exporter = new gltfExporter(gltf))
            {
                exporter.Prepare(go);
                exporter.Export();
            }
            ///////////////////////////////////////////////////////////////////////////////////////////
            
            
            
            
            
            
            
            return gltf;
        }*/

        public List<Transform> childs = new List<Transform>();



        public void FindEveryChild(Transform parent)
        {
            int count = parent.childCount;
            for (int i = 0; i < count; i++)
            {
                childs.Add(parent.GetChild(i));
            }
        }

        public List<Mesh> meshes;
        public bool exportAnimations;

        public virtual void Prepare(GameObject go, bool animations, bool inactive)
        {
            exportAnimations = animations;
            //////////////////////////////////////////////////////////////////////
            meshes = new List<Mesh>();
            // foreach (Transform child in go.transform)
            // {
            //     Debug.Log(child.name);
            // }





            FindEveryChild(go.transform);
            for (int i = 0; i < childs.Count; i++)
            {
                FindEveryChild(childs[i]);
            }

            foreach (var child in childs)
            {
                if (!inactive && child.gameObject.activeInHierarchy)
                {
                    if (child.TryGetComponent(out MeshFilter meshFilter))
                    {

                        meshes.Add(meshFilter.sharedMesh);
                    }
                }

                else
                {
                    if (child.TryGetComponent(out MeshFilter meshFilter))
                    {

                        meshes.Add(meshFilter.sharedMesh);
                    }
                }
            }





            Copy = GameObject.Instantiate(go);
            Debug.Log("eiôpfiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii");
            // Left handed to Right handed
            Copy.transform.ReverseZ();
        }

        public void Export(bool exportActive)
        {
            FromGameObject(glTF, Copy, exportActive, UseSparseAccessorForBlendShape);
        }

        public void Dispose()
        {
            if (Application.isEditor)
            {
                GameObject.DestroyImmediate(Copy);
            }
            else
            {
                GameObject.Destroy(Copy);
            }
        }

        #region Export

       /// <summary>
       /// Components
       /// </summary>
         
       public class GLTFCamera
       {
            
           public string name { get; set; } 
           public string projection { get; set; } 
           public float fov { get; set; } 
           public float aspect { get; set; } 
           
           public Vector2 clippingPlanes { get; set; }

           public int id { get; set; }
           
           public bool main { get; set; }
           public bool active { get; set; }


           public GLTFCamera(Camera camera)
           { 
               name = "Camera";
               
               if (!camera.orthographic)
                   projection = "perspective";
               else
                   projection = "orthographique";
               
               fov = camera.fieldOfView;
               aspect = camera.aspect;
               active = camera.isActiveAndEnabled;

               clippingPlanes = new Vector2(camera.nearClipPlane,camera.farClipPlane);
               
               if (camera.gameObject.CompareTag("MainCamera"))
                   main = true;
               else
                   main = false;


           }
       }
         public class GLTFLight 
         {
            
             public string name { get; set; } 
             public string type { get; set; } 
             public float intensity { get; set; } 
             public float range { get; set; } 
             public Vector4 color { get; set; } 
             
             public int id { get; set; }
             public bool active { get; set; }


             public GLTFLight(Light light)
             { 
                 name = "Light";
                 type = light.type.ToString();
                 intensity = light.intensity;
                 active = light.isActiveAndEnabled;
                 if (type == "Point" || type == "Spot")
                 {
                     range = light.range;
                 }
                 
                 // if (type == "Directional")
                 // {
                 //     Debug.Log("instantiating" + light.gameObject.transform.root);
                 //     GameObject target = new GameObject(light.gameObject.name+" Target");
                 //     Debug.Log("instantiating "+target);
                 //     target.transform.SetParent(light.gameObject.transform);
                 //     target.transform.localPosition += new Vector3(0,0,1);
                 // }
                 
                 
                 //color = new Vector4(light.color.r*255.0f, light.color.g*255.0f, light.color.b*255.0f, light.color.a*255.0f)  ;
                 color = light.color;
                 Debug.Log(color);


             }
         }
         
         public class GLTFBody 
        {
            
            public string name { get; set; } 
            public int id { get; set; }
            
            public bool isBody { get; set; }
            public float mass { get; set; }
            public int isKinematic { get; set; }
            
            public string collider { get; set; }
            public Vector3 extents { get; set; }
            public Vector3 center { get; set; }
            
            
            public GLTFBody(Rigidbody rb)
            { 
                // or set properties in default constructor to generate sample data
                name = "RigidBody";
                
                
                
                isBody = true;
                
                if (rb.isKinematic == true)
                {
                    isKinematic = 2;
                    mass = 0;

                }

                else
                {
                    isKinematic = 1;
                    mass = rb.mass;
                }
                
                if (rb.TryGetComponent(out BoxCollider bc))
                {
                    collider = "box";
                    center = bc.bounds.center;
                    extents = bc.bounds.extents;
                    
                }
                
                if (rb.TryGetComponent(out CapsuleCollider cc))
                {
                    collider = "box";
                    center = cc.bounds.center;
                    extents = cc.bounds.extents;
                }
                
                
                
                else if (rb.TryGetComponent(out SphereCollider sc))
                {
                    collider = "sphere";
                    center = sc.bounds.center;
                    
                    extents = new Vector3(sc.radius,sc.radius,sc.radius);
                }
            }
        }

         public class ComponentContainer
         {
             public List<GLTFBody> Rigidbodies { get; set; }
             public List<GLTFLight> Lights { get; set; }
             public List<GLTFCamera> Cameras { get; set; }

             public ComponentContainer()
             {
                 Rigidbodies = new List<GLTFBody>();
                 Lights = new List<GLTFLight>();
                 Cameras = new List<GLTFCamera>();
             }
         }
         public ComponentContainer GetComponents(GameObject go)
        {

            ComponentContainer componentContainer = new ComponentContainer();
            


            Rigidbody[] rigidBodies;
            
            rigidBodies = go.GetComponents<Rigidbody>();

            if (rigidBodies.Length > 0)
            {

                foreach (var body in rigidBodies)
                {
                    if (body.TryGetComponent(out Collider collider))
                    {
                        //Debug.Log(collider.GetType());
                        if (collider.GetType() != typeof(MeshCollider))
                        {
                            var rigid = new GLTFBody(body);
                            componentContainer.Rigidbodies.Add(rigid);
                        }
                    }
                }
  
            }
            
            Light[] lights;
            
            lights = go.GetComponents<Light>();

            if (lights.Length > 0)
            {

                foreach (var light in lights)
                {
                    var gltfL = new GLTFLight(light);
                    componentContainer.Lights.Add(gltfL);
                }
  
            }
            
            Camera[] cameras;
            
            cameras = go.GetComponents<Camera>();

            if (cameras.Length > 0)
            {

                foreach (var camera in cameras)
                {
                    var gltf = new GLTFCamera(camera);
                    componentContainer.Cameras.Add(gltf);
                }
  
            }
            
            
            
            
            return componentContainer;
        }
        




         glTFNode ExportNode(Transform x, List<Transform> nodes, List<Mesh> meshes, List<SkinnedMeshRenderer> skins)
        {

            var container =  GetComponents(x.gameObject);
            
            var node = new glTFNode
            {
                name = x.name,
            
                children = x.transform.GetChildren().Select(y => nodes.IndexOf(y)).ToArray(),
                rotation = x.transform.localRotation.ToArray(),
                translation = x.transform.localPosition.ToArray(),
                scale = x.transform.localScale.ToArray(),
                //Components
                components = container,

            };

            var meshFilter = x.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                node.mesh = meshes.IndexOf(meshFilter.sharedMesh);
            }

            var skinnredMeshRenderer = x.GetComponent<SkinnedMeshRenderer>();
            if (skinnredMeshRenderer != null)
            {
                node.mesh = meshes.IndexOf(skinnredMeshRenderer.sharedMesh);
                node.skin = skins.IndexOf(skinnredMeshRenderer);
            }

            return node;
        }

        static glTFMesh ExportPrimitives(glTF gltf, int bufferIndex,
            string rendererName,
            Mesh mesh, Material[] materials,
            List<Material> unityMaterials)
        {
            var positions = mesh.vertices.Select(y => y.ReverseZ()).ToArray();
            var positionAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, positions, glBufferTarget.ARRAY_BUFFER);
            gltf.accessors[positionAccessorIndex].min = positions.Aggregate(positions[0], (a, b) => new Vector3(Mathf.Min(a.x, b.x), Math.Min(a.y, b.y), Mathf.Min(a.z, b.z))).ToArray();
            gltf.accessors[positionAccessorIndex].max = positions.Aggregate(positions[0], (a, b) => new Vector3(Mathf.Max(a.x, b.x), Math.Max(a.y, b.y), Mathf.Max(a.z, b.z))).ToArray();

            var normalAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, mesh.normals.Select(y => y.ReverseZ()).ToArray(), glBufferTarget.ARRAY_BUFFER);
#if GLTF_EXPORT_TANGENTS
            var tangentAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, mesh.tangents.Select(y => y.ReverseZ()).ToArray(), glBufferTarget.ARRAY_BUFFER);
#endif
            var uvAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, mesh.uv.Select(y => y.ReverseUV()).ToArray(), glBufferTarget.ARRAY_BUFFER);
            var colorAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, mesh.colors, glBufferTarget.ARRAY_BUFFER);

            var boneweights = mesh.boneWeights;
            var weightAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, boneweights.Select(y => new Vector4(y.weight0, y.weight1, y.weight2, y.weight3)).ToArray(), glBufferTarget.ARRAY_BUFFER);
            var jointsAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, boneweights.Select(y => new UShort4((ushort)y.boneIndex0, (ushort)y.boneIndex1, (ushort)y.boneIndex2, (ushort)y.boneIndex3)).ToArray(), glBufferTarget.ARRAY_BUFFER);

            var attributes = new glTFAttributes
            {
                POSITION = positionAccessorIndex,
            };
            if (normalAccessorIndex != -1)
            {
                attributes.NORMAL = normalAccessorIndex;
            }
#if GLTF_EXPORT_TANGENTS
            if (tangentAccessorIndex != -1)
            {
                attributes.TANGENT = tangentAccessorIndex;
            }
#endif
            if (uvAccessorIndex != -1)
            {
                attributes.TEXCOORD_0 = uvAccessorIndex;
            }
            if (colorAccessorIndex != -1)
            {
                attributes.COLOR_0 = colorAccessorIndex;
            }
            if (weightAccessorIndex != -1)
            {
                attributes.WEIGHTS_0 = weightAccessorIndex;
            }
            if (jointsAccessorIndex != -1)
            {
                attributes.JOINTS_0 = jointsAccessorIndex;
            }

            var gltfMesh = new glTFMesh(mesh.name);
            for (int j = 0; j < mesh.subMeshCount; ++j)
            {
                var indices = TriangleUtil.FlipTriangle(mesh.GetIndices(j)).Select(y => (uint)y).ToArray();
                var indicesAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, indices, glBufferTarget.ELEMENT_ARRAY_BUFFER);

                if (j >= materials.Length)
                {
                    Debug.LogWarningFormat("{0}.materials is not enough", rendererName);
                    break;
                }

                gltfMesh.primitives.Add(new glTFPrimitives
                {
                    attributes = attributes,
                    indices = indicesAccessorIndex,
                    mode = 4, // triangels ?
                    material = unityMaterials.IndexOf(materials[j])
                });
            }
            return gltfMesh;
        }

        static bool UseSparse(
            bool usePosition, Vector3 position,
            bool useNormal, Vector3 normal,
            bool useTangent, Vector3 tangent
            )
        {
            var useSparse =
            (usePosition && position != Vector3.zero)
            || (useNormal && normal != Vector3.zero)
            || (useTangent && tangent != Vector3.zero)
            ;
            return useSparse;
        }

        static gltfMorphTarget ExportMorphTarget(glTF gltf, int bufferIndex,
            Mesh mesh, int j,
            bool useSparseAccessorForMorphTarget)
        {
            var blendShapeVertices = mesh.vertices;
            var usePosition = blendShapeVertices != null && blendShapeVertices.Length > 0;

            var blendShapeNormals = mesh.normals;
            var useNormal = usePosition && blendShapeNormals != null && blendShapeNormals.Length == blendShapeVertices.Length;

            var blendShapeTangents = mesh.tangents.Select(y => (Vector3)y).ToArray();
            //var useTangent = usePosition && blendShapeTangents != null && blendShapeTangents.Length == blendShapeVertices.Length;
            var useTangent = false;

            var frameCount = mesh.GetBlendShapeFrameCount(j);
            mesh.GetBlendShapeFrameVertices(j, frameCount - 1, blendShapeVertices, blendShapeNormals, null);

            var blendShapePositionAccessorIndex = -1;
            var blendShapeNormalAccessorIndex = -1;
            var blendShapeTangentAccessorIndex = -1;
            if (useSparseAccessorForMorphTarget)
            {
                var accessorCount = blendShapeVertices.Length;
                var sparseIndices = Enumerable.Range(0, blendShapeVertices.Length)
                    .Where(x => UseSparse(
                        usePosition, blendShapeVertices[x],
                        useNormal, blendShapeNormals[x],
                        useTangent, blendShapeTangents[x]))
                    .ToArray()
                    ;

                if (sparseIndices.Length == 0)
                {
                    usePosition = false;
                    useNormal = false;
                    useTangent = false;
                }
                else
                {
                    Debug.LogFormat("Sparse {0}/{1}", sparseIndices.Length, mesh.vertexCount);
                }
                /*
                var vertexSize = 12;
                if (useNormal) vertexSize += 12;
                if (useTangent) vertexSize += 24;
                var sparseBytes = (4 + vertexSize) * sparseIndices.Length;
                var fullBytes = (vertexSize) * blendShapeVertices.Length;
                Debug.LogFormat("Export sparse: {0}/{1}bytes({2}%)",
                    sparseBytes, fullBytes, (int)((float)sparseBytes / fullBytes)
                    );
                    */

                var sparseIndicesViewIndex = -1;
                if (usePosition)
                {
                    sparseIndicesViewIndex = gltf.ExtendBufferAndGetViewIndex(bufferIndex, sparseIndices);

                    blendShapeVertices = sparseIndices.Select(x => blendShapeVertices[x].ReverseZ()).ToArray();
                    blendShapePositionAccessorIndex = gltf.ExtendSparseBufferAndGetAccessorIndex(bufferIndex, accessorCount,
                        blendShapeVertices,
                        sparseIndices, sparseIndicesViewIndex,
                        glBufferTarget.ARRAY_BUFFER);
                }

                if (useNormal)
                {
                    blendShapeNormals = sparseIndices.Select(x => blendShapeNormals[x].ReverseZ()).ToArray();
                    blendShapeNormalAccessorIndex = gltf.ExtendSparseBufferAndGetAccessorIndex(bufferIndex, accessorCount,
                        blendShapeNormals,
                        sparseIndices, sparseIndicesViewIndex,
                        glBufferTarget.ARRAY_BUFFER);
                }

                if (useTangent)
                {
                    blendShapeTangents = sparseIndices.Select(x => blendShapeTangents[x].ReverseZ()).ToArray();
                    blendShapeTangentAccessorIndex = gltf.ExtendSparseBufferAndGetAccessorIndex(bufferIndex, accessorCount,
                        blendShapeTangents, sparseIndices, sparseIndicesViewIndex,
                        glBufferTarget.ARRAY_BUFFER);
                }
            }
            else
            {
                for (int i = 0; i < blendShapeVertices.Length; ++i) blendShapeVertices[i] = blendShapeVertices[i].ReverseZ();
                if (usePosition)
                {
                    blendShapePositionAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex,
                        blendShapeVertices,
                        glBufferTarget.ARRAY_BUFFER);
                }

                if (useNormal)
                {
                    for (int i = 0; i < blendShapeNormals.Length; ++i) blendShapeNormals[i] = blendShapeNormals[i].ReverseZ();
                    blendShapeNormalAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex,
                        blendShapeNormals,
                        glBufferTarget.ARRAY_BUFFER);
                }

                if (useTangent)
                {
                    for (int i = 0; i < blendShapeTangents.Length; ++i) blendShapeTangents[i] = blendShapeTangents[i].ReverseZ();
                    blendShapeTangentAccessorIndex = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex,
                        blendShapeTangents,
                        glBufferTarget.ARRAY_BUFFER);
                }
            }

            if (blendShapePositionAccessorIndex != -1)
            {
                gltf.accessors[blendShapePositionAccessorIndex].min = blendShapeVertices.Aggregate(blendShapeVertices[0], (a, b) => new Vector3(Mathf.Min(a.x, b.x), Math.Min(a.y, b.y), Mathf.Min(a.z, b.z))).ToArray();
                gltf.accessors[blendShapePositionAccessorIndex].max = blendShapeVertices.Aggregate(blendShapeVertices[0], (a, b) => new Vector3(Mathf.Max(a.x, b.x), Math.Max(a.y, b.y), Mathf.Max(a.z, b.z))).ToArray();
            }

            return new gltfMorphTarget
            {
                POSITION = blendShapePositionAccessorIndex,
                NORMAL = blendShapeNormalAccessorIndex,
                TANGENT = blendShapeTangentAccessorIndex,
            };
        }

        static void ExportMeshes(glTF gltf, int bufferIndex,
            List<MeshWithRenderer> unityMeshes, List<Material> unityMaterials,
            bool useSparseAccessorForMorphTarget)
        {

            for (int i = 0; i < unityMeshes.Count; ++i)
            {
                var x = unityMeshes[i];
                var mesh = x.Mesh;
                var materials = x.Rendererer.sharedMaterials;

                var gltfMesh = ExportPrimitives(gltf, bufferIndex,
                    x.Rendererer.name,
                    mesh, materials, unityMaterials);

                for (int j = 0; j < mesh.blendShapeCount; ++j)
                {
                    var morphTarget = ExportMorphTarget(gltf, bufferIndex,
                        mesh, j,
                        useSparseAccessorForMorphTarget);

                    //
                    // all primitive has same blendShape
                    //
                    for (int k = 0; k < gltfMesh.primitives.Count; ++k)
                    {
                        gltfMesh.primitives[k].targets.Add(morphTarget);
                        gltfMesh.primitives[k].extras.targetNames.Add(mesh.GetBlendShapeName(j));
                    }
                }

                gltf.meshes.Add(gltfMesh);
            }
        }
        
        
        public void FromGameObject(glTF gltf, GameObject go,bool exportActive, bool useSparseAccessorForMorphTarget = false)
        {
            var bytesBuffer = new ArrayByteBuffer(new byte[50 * 1024 * 1024]);
            var bufferIndex = gltf.AddBuffer(bytesBuffer);

            GameObject tmpParent = null;
            if (go.transform.childCount == 0)
            {
                tmpParent = new GameObject("tmpParent");
                go.transform.SetParent(tmpParent.transform, true);
                go = tmpParent;
            }

            try
            {

                Nodes = go.transform.Traverse()
                    .Skip(1) // exclude root object for the symmetry with the importer
                    .ToList();

                if (!exportActive)
                {
                    foreach (var node in Nodes.ToList())
                    {
                        if (!node.gameObject.activeInHierarchy)
                        {
                            Nodes.Remove(node);
                        }
                    }
      
                }
                

                #region Materials and Textures
                Materials = Nodes.SelectMany(x => x.GetSharedMaterials()).Where(x => x != null).Distinct().ToList();
                var unityTextures = Materials.SelectMany(x => TextureIO.GetTextures(x)).Where(x => x.Texture != null).Distinct().ToList();

                TextureManager = new TextureExportManager(unityTextures.Select(x => x.Texture));

                var materialExporter = CreateMaterialExporter();
                gltf.materials = Materials.Select(x => materialExporter.ExportMaterial(x, TextureManager)).ToList();

                for (int i = 0; i < unityTextures.Count; ++i)
                {
                    var unityTexture = unityTextures[i];
                    TextureIO.ExportTexture(gltf, bufferIndex, TextureManager.GetExportTexture(i), unityTexture.TextureType);
                }
                #endregion


                #region Meshes
                var unityMeshes = Nodes
                    .Select(x => new MeshWithRenderer
                    {
                        Mesh = x.GetSharedMesh(),
                        Rendererer = x.GetComponent<Renderer>(),
                    })
                    .Where(x =>
                    {
                        
                        if (x.Mesh == null)
                        {
                            return false;
                        }
                        if (x.Rendererer.sharedMaterials == null
                        || x.Rendererer.sharedMaterials.Length == 0)
                        {
                            return false;
                        }

                        return true;
                    })
                    .ToList();

                var a  = new List<Mesh>();
                var b  = new List<MeshWithRenderer>();
                
                foreach (var mesh in unityMeshes)
                {

                    if (!a.Contains(mesh.Mesh))
                    {
                        a.Add(mesh.Mesh);
                        b.Add(mesh);
                    }
                }

                
                unityMeshes = new List<MeshWithRenderer>();
                unityMeshes = b;
                
                // foreach (var mesh in unityMeshes)
                // {
                //     Debug.Log(mesh.Mesh);
                // }
                
                
                //////Problem with material assign prolly here
                
                ExportMeshes(gltf, bufferIndex, unityMeshes, Materials, useSparseAccessorForMorphTarget);
                Meshes = unityMeshes.Select(x => x.Mesh).ToList();
                #endregion

                #region Skins
                var unitySkins = Nodes
                    .Select(x => x.GetComponent<SkinnedMeshRenderer>()).Where(x =>
                        x != null
                        && x.bones != null
                        && x.bones.Length > 0)
                    .ToList();
                gltf.nodes = Nodes.Select(x => ExportNode(x, Nodes, unityMeshes.Select(y => y.Mesh).ToList(), unitySkins)).ToList();
                
                gltf.scenes = new List<gltfScene>
                {
                    new gltfScene
                    {
                        nodes = go.transform.GetChildren().Select(x => Nodes.IndexOf(x)).ToArray(),
                    }
                    
                    
                };

  

                foreach (var x in unitySkins)
                {
                    var matrices = x.sharedMesh.bindposes.Select(y => y.ReverseZ()).ToArray();
                    var accessor = gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, matrices, glBufferTarget.NONE);

                    var skin = new glTFSkin
                    {
                        inverseBindMatrices = accessor,
                        joints = x.bones.Select(y => Nodes.IndexOf(y)).ToArray(),
                        skeleton = Nodes.IndexOf(x.rootBone),
                    };
                    var skinIndex = gltf.skins.Count;
                    gltf.skins.Add(skin);

                    foreach (var z in Nodes.Where(y => y.Has(x)))
                    {
                        var nodeIndex = Nodes.IndexOf(z);
                        var node = gltf.nodes[nodeIndex];
                        node.skin = skinIndex;
                    }
                }
                
         
                #endregion

#if UNITY_EDITOR
                #region Animations

                if (exportAnimations)
                {
                    List<Transform> animators = new List<Transform>();

                    AnimationExporter.aniCLips.Clear();
                    childs.Clear();

                    if (go.TryGetComponent(out Animator ThisAssnim) ||
                        go.TryGetComponent(out Animation ThisOtherssAnim))
                    {
                        Debug.Log(go);

                        animators.Add(go.transform);

                        //  Debug.Log(meshFilter.sharedMesh);
                    }


                    FindEveryChild(go.transform);
                    for (int i = 0; i < childs.Count; i++)
                    {
                        FindEveryChild(childs[i]);
                    }

                    foreach (var child in childs)
                    {

                        if (child.TryGetComponent(out Animator ThisAnim) ||
                            child.TryGetComponent(out Animation ThisOtherAnim))
                        {
                            animators.Add(child);
                        }
                    }

                    foreach (Transform anim in animators)
                    {


                        var clips = new List<AnimationClip>();
                        var animator = anim.GetComponent<Animator>();
                        var animation = anim.GetComponent<Animation>();
                        if (animator != null)
                        {
                            clips = AnimationExporter.GetAnimationClips(animator);


                        }

                        if (animation != null)
                        {
                            clips = AnimationExporter.GetAnimationClips(animation);

                        }

                        if (clips.Any())
                        {

                            foreach (AnimationClip clip in clips)
                            {

                                if (clip.humanMotion)
                                {

                                }

                                var animationWithCurve = AnimationExporter.Export(clip, anim.transform, Nodes);

                                foreach (var kv in animationWithCurve.SamplerMap)
                                {
                                    var sampler = animationWithCurve.Animation.samplers[kv.Key];


                                    var inputAccessorIndex =
                                        gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, kv.Value.Input);
                                    sampler.input = inputAccessorIndex;

                                    var outputAccessorIndex =
                                        gltf.ExtendBufferAndGetAccessorIndex(bufferIndex, kv.Value.Output);
                                    sampler.output = outputAccessorIndex;

                                    // modify accessors
                                    var outputAccessor = gltf.accessors[outputAccessorIndex];

                                    var channel = animationWithCurve.Animation.channels.First(x => x.sampler == kv.Key);
                                    switch (glTFAnimationTarget.GetElementCount(channel.target.path))
                                    {
                                        case 1:
                                            outputAccessor.type = "SCALAR";
                                            //outputAccessor.count = ;
                                            break;
                                        case 3:
                                            outputAccessor.type = "VEC3";
                                            outputAccessor.count /= 3;
                                            //   outputAccessor.max 
                                            // outputAccessor.min

                                            break;

                                        case 4:
                                            outputAccessor.type = "VEC4";
                                            outputAccessor.count /= 4;
                                            break;


                                        default:
                                            throw new NotImplementedException();

                                    }



                                }

                                gltf.animations.Add(animationWithCurve.Animation);


                            }
                        }
                    }


                }


                #endregion
#endif
            }
            finally
            {
                if (tmpParent != null)
                {
                    tmpParent.transform.GetChild(0).SetParent(null);
                    if (Application.isPlaying)
                    {
                        GameObject.Destroy(tmpParent);
                    }
                    else
                    {
                        GameObject.DestroyImmediate(tmpParent);
                    }
                }
            }
        }
        #endregion
    }
}
