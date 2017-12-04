using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace pirklaHeatMap
{
    public class HeatMap : MonoBehaviour
    {
        public GameObject HeatNodePrefab;
        public static HeatMap MainHeatMap;

        public Dictionary<HeatMapAffector, HeatNode> AffectorNodeDict;

        public Dictionary<Vector2, HeatNode> GraphNodeDict;

        public int Interval = 5;
        public int Width = 100;
        public int Height = 100;
        public int[] DisplayLayers = new int[] { 0, 1 };
        [HideInInspector]
        public float LoadingPercentage = 0;
        [HideInInspector]
        public bool IsLoading = false;
        public int InstantiateBatchSize = 300;
        public int NeighborIterateBatchSize = 25;

        public event EventHandler HeatMapUpdateHandler;
        public event EventHandler AffectorRemoved;

        [HideInInspector]
        public List<HeatNode> ConnectedNodes;

        public void GenerateGrid()
        {
            StartCoroutine(GridGenerator());
        }
        private IEnumerator GridGenerator()
        {
            LoadingPercentage = 0;
            IsLoading = true;
            ConnectedNodes = new List<HeatNode>();
            int row = 0;
            int collumn = 0;
            for (int i = 0; i <= Width; i += Interval, collumn += 1)
            {
                for (int n = 0; n <= Height; n += Interval, row += 1)
                {
                    GameObject obj;
                    if (HeatNodePrefab)
                    {
                        obj = Instantiate(HeatNodePrefab);
                    }
                    else
                    {
                        obj = new GameObject();
                    }

                    obj.transform.SetParent(transform);
                    obj.transform.position = new Vector3(i, 0, n);
                    HeatNode newNode = obj.AddComponent<HeatNode>();
                    newNode.OnCreate(collumn, row, new Vector3(i, 0, n), DisplayLayers);

                    ConnectedNodes.Add(newNode);

                    if ((i + n) % InstantiateBatchSize == 0)
                        yield return null;
                }
                LoadingPercentage = ((float)i / Width) * 50;
                row = 0;
            }
            yield return null;
            StartCoroutine(FindNeighbors());
            yield return null;
        }
        private IEnumerator FindNeighbors()
        {
            IsLoading = true;
            for (int i = 0; i < ConnectedNodes.Count; i++)
            {
                LoadingPercentage = ((float)i / ConnectedNodes.Count) * 50 + 50;
                ConnectedNodes[i].FindNeighbors(ConnectedNodes);
                if ((i % NeighborIterateBatchSize == 0))
                {
                    yield return null;
                }
            }
            LoadingPercentage = 100;
            IsLoading = false;
            yield return null;
        }

        public void RemoveGrid()
        {
            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in transform)
            {
                children.Add(child.gameObject);
                ConnectedNodes = new List<HeatNode>();
            }
            children.ForEach(n => DestroyImmediate(n));
        }

        void Start()
        {
            AffectorNodeDict = new Dictionary<HeatMapAffector, HeatNode>();
            GraphNodeDict = new Dictionary<Vector2, HeatNode>();
            if (MainHeatMap == null)
            {
                MainHeatMap = this;
            }
            foreach (HeatNode node in HeatNode.AllHeatNodes)
            {
                GraphNodeDict.Add(new Vector2(node.Collumn, node.Row), node);
            }
            foreach (HeatMapAffector affector in HeatMapAffector.AllHeatMapAffectors)
            {
                affector.AffectorUpdate += OnAffectorUpdate;
                affector.AffectorRemoved += OnAffectorRemoved;
            }

            HeatMapImage = new Texture2D((Width / Interval) + 1, (Height / Interval) + 1);
            HeatMapImage.wrapMode = TextureWrapMode.Clamp;
            if (HeatMapRawImageToReplace)
                HeatMapRawImageToReplace.texture = HeatMapImage;
        }
        void Update()
        {
            ChangeHeatMapColor();

        }

        void OnAffectorUpdate(object sender, EventArgs e)
        {
            HeatMapAffector affector = sender as HeatMapAffector;
            int collumn = Mathf.RoundToInt(affector.transform.position.x / Interval);
            int row = Mathf.RoundToInt(affector.transform.position.z / Interval);
            HeatNode newNode;
            GraphNodeDict.TryGetValue(new Vector2(collumn, row), out newNode);
            if (newNode)
            {
                HeatNode oldNode;
                if (!AffectorNodeDict.TryGetValue(affector, out oldNode))
                {
                    AffectorNodeDict.Add(affector, newNode);
                }
                else
                {
                    AffectorNodeDict[affector] = newNode;
                }

                if (oldNode != newNode)
                {
                    if (oldNode)
                    {
                        oldNode.Decreaseheat(affector.Layer, affector.HeatAffectCurve.Evaluate(0));
                        oldNode.NeighborNodes[0].DecreaseHeat(affector.Layer, affector.HeatAffectCurve.Evaluate(1));
                        oldNode.NeighborNodes[1].DecreaseHeat(affector.Layer, affector.HeatAffectCurve.Evaluate(2));
                        oldNode.NeighborNodes[2].DecreaseHeat(affector.Layer, affector.HeatAffectCurve.Evaluate(3));
                    }
                    newNode.IncreaseHeat(affector.Layer, affector.HeatAffectCurve.Evaluate(0));
                    newNode.NeighborNodes[0].IncreaseHeat(affector.Layer, affector.HeatAffectCurve.Evaluate(1));
                    newNode.NeighborNodes[1].IncreaseHeat(affector.Layer, affector.HeatAffectCurve.Evaluate(2));
                    newNode.NeighborNodes[2].IncreaseHeat(affector.Layer, affector.HeatAffectCurve.Evaluate(3));

                }
                oldNode = newNode;

            }
        }
        void OnAffectorRemoved(object sender, EventArgs e)
        {
            HeatMapAffector affector = sender as HeatMapAffector;
            affector.AffectorUpdate -= OnAffectorUpdate;
            affector.AffectorRemoved -= OnAffectorRemoved;
        }

        public void HideBoxes()
        {
            foreach (HeatNode node in ConnectedNodes)
            {
                if (node.MyRenderer)
                    node.MyRenderer.enabled = false;
            }
        }
        public void ShowBoxes()
        {
            foreach (HeatNode node in ConnectedNodes)
            {
                if (node.MyRenderer)
                    node.MyRenderer.enabled = true;
            }
        }

        public void ChangeHeatMapColor()
        {

            // TODO evaluate this for performance...may need to decrease call frequency
            foreach (HeatNode node in ConnectedNodes)
            {
                HeatMapImage.SetPixel(node.Collumn, node.Row, node.MyColor);
            }
            HeatMapImage.Apply();
        }
        // TODO create new class for HeatMap visualizer
        Texture2D HeatMapImage;
        public RawImage HeatMapRawImageToReplace;
    }
}
