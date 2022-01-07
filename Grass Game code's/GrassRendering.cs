using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Random = System.Random;

namespace MiddleGames.Misc
{
    [ExecuteInEditMode]
    public class GrassRendering : MonoBehaviour, IRenderer
    {
        public Material grassMaterial;
        public Material stackMaterial;

        public Vector2 GrassSize = new Vector2(80, 100);
        public float width;
        public Color _grassColor;
        public Transform targetPos;
        public int grassAmount = 30_000;
        [NonSerialized]
        public float playerRange;

        // at Stacks prefabs
        public int packIndex;
        public int GetPackIndex() => packIndex;

        private Property[] matrices;

        public int loadIndex = -1;
        
        public Vector2 anchor;

        ComputeBuffer computeBuffer, argsBuffer;
        public enum State { none, cut, paint}
        public State state;

        public bool CanDeform = true;
        
        Bounds bounds;

        public Transform fenceParent;

        private const int targetFps = 60;
        
        public static float particleDuration;

        [ContextMenu("reset")]
        private void Reset() {
            transform.localScale = new Vector3(50, 30, 65);
        }

        private void Start()
        {
            Application.targetFrameRate = 30;

            // for disabling frustum culling because without this at some point maybe grasses dont drawn
            Camera.main.cullingMatrix = Matrix4x4.Ortho(-1000, 1000, -1000, 1000, 0.001f, 2000) * Matrix4x4.Translate(Vector3.forward * -2000 / 2f) * Camera.main.worldToCameraMatrix;
  
            UpdateMatrices();
            if (loadIndex != -1) LoadColors(loadIndex);
            grassMaterial.SetVector("playerPos", Vector3.zero);
        }


        [ContextMenu("Test")]
        public unsafe void Test() 
        {
        }

        private Mesh _grassMesh;
        private Mesh GetGrassMeshCache()
        {
            if (!_grassMesh)
            {
                //if not exist, create a 3 vertices hardcode triangle grass mesh
                _grassMesh = new Mesh();

                //single grass (vertices)
                Vector3[] verts = new Vector3[6]
                {
                    new Vector3(-width, 0),
                    new Vector3(+width, 0),
                    new Vector3(-0.0f, 1),
                    new Vector3(-0.0f, 1),
                    new Vector3(+width, 0),
                    new Vector3(-width, 0)
                };

                Vector2[] uvs = new Vector2[6]
                {
                    new Vector2(0, 0),
                    new Vector2(0, 0),
                    new Vector2(0, 1),
                    new Vector2(0, 1),
                    new Vector2(0, 0),
                    new Vector2(0, 0)
                };

                //single grass (Triangle index)
                int[] trinagles = new int[6] { 0, 1, 2, 3, 4, 5 }; //order to fit Cull Back in grass shader

                _grassMesh.SetVertices(verts);
                _grassMesh.SetUVs(0, uvs);
                _grassMesh.SetTriangles(trinagles, 0);
            }

            return _grassMesh;
        }
        public void OnValidate()
        {
            if (!Application.isPlaying)
            {
                UpdateMatrices();
            }
        }
        public void SaveColors(int index)
        {
            using FileStream stream = new FileStream(@$"E:\Development\Grass Game\Assets\Colors{index}.txt", FileMode.Create);
            using StreamWriter writer = new StreamWriter(stream);
            foreach (Property p in matrices)
            {
                writer.WriteLine($"{p.color.r} {p.color.g} {p.color.b}");
            }
        }
        [ContextMenu("LoadColors")]
        public void LoadColors()
        {
            LoadColors(0);
        }
        private void LoadColors(int index)
        { 
            using FileStream stream = new FileStream(@$"E:\Development\Grass Game\Assets\Colors{index}.txt", FileMode.Open);
            using StreamReader writer = new StreamReader(stream);

            for (int i = 0; i < matrices.Length; i++)
            {
                Property p = matrices[i];
                string[] values = writer.ReadLine().Split(' ');
                var property = p;
                property.color = new Color(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), 1);
                matrices[i] = property;
            }

            if (computeBuffer != null) computeBuffer.Release();
            computeBuffer = new ComputeBuffer(matrices.Length, Property.SizeInBytes);
            computeBuffer.SetData(matrices);
            grassMaterial.SetBuffer("meshProperties", computeBuffer);
        }
        public void UpdateMatrices()
        { 
            Vector3 range = transform.localScale * 0.5f;
            
            matrices = new Property[grassAmount];
            
            Random rand = new Random(888);

            _grassMesh = null;

            for (int j = 0; j < matrices.Length; j++)
            {
                Vector3 origin = transform.position;
                origin += NextRandomRange(rand) * range.x * transform.right;
                origin += NextRandomRange(rand) * range.z * transform.forward;

                matrices[j] = new Property(_grassColor, origin,
                    Quaternion.Euler(0, NextRandomRange(rand, 0, 360), 0),
                    Vector3.one * NextRandomRange(rand, GrassSize.x, GrassSize.y));
            }
            
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            min.y = 0; max.y = 2;
            
            // calculate grass bounds for rendering and colission detection
            for (int i = 0; i < matrices.Length; i++)
            {
                if (matrices[i].matrix.m03 > max.x) max.x = matrices[i].matrix.m03;
                if (matrices[i].matrix.m23 > max.z) max.z = matrices[i].matrix.m23;
                if (matrices[i].matrix.m03 < min.x) min.x = matrices[i].matrix.m03;
                if (matrices[i].matrix.m23 < min.z) min.z = matrices[i].matrix.m23;
            }
            
            bounds = new Bounds() { min = min , max = max };
            args[0] = (uint)6; // 6 vertex
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            computeBuffer = new ComputeBuffer(matrices.Length, Property.SizeInBytes);
            
            // bounds = new Bounds(transform.position + (transform.localScale / 2), transform.localScale);
            UpdateArgs();
        }
        public float NextRandomRange(Random rand)
        {
            float sample = (float)rand.NextDouble();
            return (0.5f * sample) + ((-0.5f) * (1f - sample));
        }
        public float NextRandomRange(Random rand, in float min, in float max)
        {
            float sample = (float)rand.NextDouble();
            return (max * sample) + (min * (1f - sample));
        }

        Vector3 oldPos;
        private void LateUpdate()
        {
#if UNITY_EDITOR
            if (transform.position != oldPos) {
                OnValidate();
                oldPos = transform.position;
            }
#endif
            if (grassAmount < 5) return; // 5 is random number for finish
            
            Graphics.DrawMeshInstancedIndirect(GetGrassMeshCache(), 0, grassMaterial, bounds, argsBuffer, 0, null, ShadowCastingMode.Off, true, 0, null, LightProbeUsage.Off);
            
            if (CanDeform == false) return;

            if (particleDuration < 0) {
                GrassManager.instance.SetParticleEmission(false);
                grassMaterial.SetVector("playerPos", Vector3.zero);
                
            }
            else {
                GrassManager.instance.SetParticleEmission(true);
                grassMaterial.SetVector("playerPos", targetPos.position);
            }

            particleDuration -= Time.deltaTime;
        }

        
        public unsafe void Deform()
        {
            if (grassAmount < 2 || Stack.instance.IsFull()) return;

            Vector2 playerPos = new Vector2(targetPos.position.x, targetPos.position.z) + anchor;

            int cuttedGrassAmount = 0;

            fixed (float* ptr = &matrices[0].matrix.m03)
            {
                float* end = ptr + (matrices.Length * 20 - 16);
                float* curr = ptr;
                
                float diff_x, diff_z;
                
                while (curr < end)
                {
                    diff_x = (*curr) - playerPos.x;      
                    curr += 2;
                    diff_z = (*curr) - playerPos.y;      
                                     
                    
                    if (Mathf.Sqrt(diff_x * diff_x + diff_z * diff_z) < playerRange)
                    {                                                              
                        *curr = -64;
                        cuttedGrassAmount++;
                        particleDuration = 0.25f;
                    }
                    curr += 18; // +2 end matrix + 4 jump color + 11 for m03
                }
            }

            grassAmount -= cuttedGrassAmount;

            if (grassAmount <= 800) {
                // 30 is random number for finish
                // Stage Complated Code Here
                grassAmount = 1;
                DisposeGrasses();
                OnStageCompleted();
                Stack.instance.OnItemCollected(cuttedGrassAmount);
                return;
            }
            
            Stack.instance.OnItemCollected(cuttedGrassAmount);

            UpdateArgs();
        }

        private void DisposeGrasses()
        { 
            Array.Clear(matrices, 0, matrices.Length);
            Array.Resize(ref matrices, 1);

            computeBuffer.SetData(matrices);
            grassMaterial.SetBuffer("meshProperties", computeBuffer);

            computeBuffer.Dispose();
            argsBuffer.Dispose();

            matrices = null;
            GC.Collect();
        }

        public void OnStageCompleted()
        {
            for (short i = 0; i < fenceParent.childCount; i++)
            {
                fenceParent.GetChild(i).GetComponent<BoxCollider>().isTrigger = true;
            }
            CanDeform = false;
            GrassManager.NextStage();
            Amazing.Show();
        }

        readonly uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        public void UpdateArgs()
        { 
            // Arguments for drawing mesh. 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.
            args[1] = (uint)matrices.Length;
            
            argsBuffer.SetData(args);
            computeBuffer.SetData(matrices);
            
            grassMaterial.SetBuffer("meshProperties", computeBuffer);
            grassMaterial.SetVector("playerRange", new Vector4(playerRange, anchor.x, anchor.y, 0) );
        }

        public unsafe void Paint(Vector3 position, in Color color, in float paintDist)
        {
            position.x += anchor.x;
            position.z += anchor.y;

            fixed (float* ptr = &matrices[0].matrix.m03)
            {
                float* end = ptr + (matrices.Length * 20 - 16); // 20 = number of floats in a property
                float* curr = ptr;

                float diff_x, diff_z;

                while (curr < end)
                {
                    diff_x = (*curr) - position.x;
                    curr += 2;
                    diff_z = (*curr) - position.y;

                    if (Mathf.Sqrt(diff_x * diff_x + diff_z * diff_z) < playerRange)
                    {
                        *(curr - 18) = color.r;
                        *(curr - 17) = color.g;
                        *(curr - 16) = color.b;
                    }
                    curr += 18; // +2 end matrix + 4 jump color + 11 for m03
                }
            }

            if (computeBuffer != null) computeBuffer.Release();
            computeBuffer = new ComputeBuffer(matrices.Length, Property.SizeInBytes);
            computeBuffer.SetData(matrices);
            grassMaterial.SetBuffer("meshProperties", computeBuffer);
            grassMaterial.SetFloat("playerRange", playerRange);
        }
        
        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct Property
        {
            public Color color;
            public Matrix4x4 matrix;

            public const int SizeInBytes = 16 + 64 ;
            public Property(in Color color, in Vector3 pos, in Quaternion rotation, in Vector3 scale)
            {
                this.color = color;
                this.matrix = Matrix4x4.TRS(pos, rotation, scale);
            }
        }
    }
    

#if UNITY_EDITOR
    [CustomEditor(typeof(GrassRendering))]
    public class GrassEditor : Editor
    {
        public int saveIndex;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            saveIndex = EditorGUILayout.IntField(saveIndex);

            if (GUILayout.Button("Save Colors"))
            {
                ((GrassRendering)target).SaveColors(saveIndex);
            }
        }
    }
#endif
}
