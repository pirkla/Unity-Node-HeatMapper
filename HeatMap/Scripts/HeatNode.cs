using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace pirklaHeatMap
{
    public class HeatNode : MonoBehaviour
    {

        public static List<HeatNode> AllHeatNodes;
        public static List<HeatNode> AllActiveHeatNodes;
        public int Collumn;
        public int Row;
        public Vector3 Position;

        public Color HeatColor1 = Color.blue;
        public Color HeatColor2 = Color.green;
        public Color HeatColor3 = Color.yellow;
        public Color HeatColor4 = Color.red;

        public Dictionary<int, float> HeatLayerDict;
        public float MaxHeat = 100;

        public Color MyColor;
        public Renderer MyRenderer;
        public int[] DisplayLayers;
        public NeighborNodeArray[] NeighborNodes = new NeighborNodeArray[6];
        public Dictionary<int, float>[] CumulativeHeatArray = new Dictionary<int, float>[6];


        void Awake()
        {
            if (AllHeatNodes == null)
                AllHeatNodes = new List<HeatNode>();
            AllHeatNodes.Add(this);
        }
        private void Start()
        {
            AllActiveHeatNodes = new List<HeatNode>();
            HeatLayerDict = new Dictionary<int, float>();
            SetColor();
        }

        public void OnCreate(int collumn, int row, Vector3 position, int[] displayLayers)
        {
            Collumn = collumn;
            Row = row;
            Position = position;
            MyRenderer = GetComponent<Renderer>();
            DisplayLayers = displayLayers;
            MyRenderer.enabled = false;
            name = "Node C:" + collumn + " R:" + row;
        }
        public void FindNeighbors(List<HeatNode> heatNodeList)
        {
            for (int i = 1; i <= NeighborNodes.Length; i++)
            {
                NeighborNodes[i - 1] = new NeighborNodeArray(FindNodesAtDistance(i, heatNodeList));
            }
        }
        public void SetCumulativeHeat(int layer, int distance)
        {
            float heatAccumulator = 0;
            for (int i = 0; i < distance; i++)
            {
                foreach (HeatNode node in NeighborNodes[i].ContainedNodes)
                {
                    float newNodeHeat = 0;
                    node.HeatLayerDict.TryGetValue(layer, out newNodeHeat);
                    heatAccumulator += newNodeHeat;
                }
            }
            if (CumulativeHeatArray[distance] == null)
                CumulativeHeatArray[distance] = new Dictionary<int, float>();
            if (!CumulativeHeatArray[distance].ContainsKey(layer))
            {
                CumulativeHeatArray[distance].Add(layer, 0);
            }
            CumulativeHeatArray[distance][layer] = heatAccumulator;
        }

        public void IncreaseHeat(int layer, float amount)
        {
            if (!HeatLayerDict.ContainsKey(layer))
            {
                HeatLayerDict.Add(layer, 0);
            }
            HeatLayerDict[layer] += amount;
            if (!AllActiveHeatNodes.Contains(this))
            {
                AllActiveHeatNodes.Add(this);
            }
            SetColor();
            SetCumulativeHeat(layer, 3);
            //Debug.Log(CumulativeHeatArray[3][layer]);
        }
        public void Decreaseheat(int layer, float amount)
        {
            if (!HeatLayerDict.ContainsKey(layer))
            {
                HeatLayerDict.Add(layer, 0);
            }
            HeatLayerDict[layer] -= amount;
            if (HeatLayerDict[layer] <= 0)
            {
                HeatLayerDict[layer] = 0;
                AllActiveHeatNodes.Remove(this);
            }
            SetColor();
            SetCumulativeHeat(layer, 3);

        }
        private void SetColor()
        {
            float colorVal = 0;
            foreach (int layer in DisplayLayers)
            {
                float valHolder;
                HeatLayerDict.TryGetValue(layer, out valHolder);
                if (valHolder > colorVal)
                    colorVal = valHolder;

            }
            colorVal /= MaxHeat;
            colorVal *= 3;
            Color color;
            if (colorVal < 1)
                color = Color.Lerp(HeatColor1, HeatColor2, colorVal);
            else if (colorVal < 2)
                color = Color.Lerp(HeatColor2, HeatColor3, colorVal - 1);
            else
                color = Color.Lerp(HeatColor3, HeatColor4, colorVal - 2);
            MyColor = color;

            if (MyRenderer)
                MyRenderer.material.color = color;

        }
        public List<HeatNode> FindNodesAtDistance(int distanceOutward, List<HeatNode> fromList)
        {
            List<HeatNode> retNodes = new List<HeatNode>();
            int countNeg = 0;
            int countPos = 0;
            for (int i = -distanceOutward; i <= 0; i++)
            {
                HeatNode posNode = fromList.Find(k => k.Collumn == Collumn + i && k.Row == Row + countNeg);

                if (posNode)
                    retNodes.Add(posNode);
                if (countNeg != 0)
                {
                    HeatNode negNode = fromList.Find(k => k.Collumn == Collumn + i && k.Row == Row - countNeg);
                    if (negNode)
                        retNodes.Add(negNode);
                }
                countNeg++;
            }
            for (int i = distanceOutward; i > 0; i--)
            {
                HeatNode posBox = fromList.Find(k => k.Collumn == Collumn + i && k.Row == Row + countPos);
                if (posBox)
                    retNodes.Add(posBox);
                if (countPos != 0)
                {
                    HeatNode negBox = fromList.Find(k => k.Collumn == Collumn + i && k.Row == Row - countPos);
                    if (negBox)
                        retNodes.Add(negBox);
                }

                countPos++;
            }
            return retNodes;
        }

        public static float CumulativeHeat(int layer, List<HeatNode> nodeList)
        {
            float retHeat = 0;
            foreach (HeatNode node in nodeList)
            {
                float nodeHeat;
                node.HeatLayerDict.TryGetValue(layer, out nodeHeat);
                retHeat += nodeHeat;
            }
            return retHeat;
        }
    }

    [System.Serializable]
    public class NeighborNodeArray
    {
        public List<HeatNode> ContainedNodes;
        public NeighborNodeArray(List<HeatNode> NeighborList)
        {
            ContainedNodes = NeighborList;
        }
        public void IncreaseHeat(int layer, float amount)
        {
            foreach (HeatNode node in ContainedNodes)
            {
                node.IncreaseHeat(layer, amount);
            }
        }
        public void DecreaseHeat(int layer, float amount)
        {
            foreach (HeatNode node in ContainedNodes)
            {
                node.Decreaseheat(layer, amount);
            }
        }

    }
}