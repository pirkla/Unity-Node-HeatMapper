using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace pirklaHeatMap
{
    public class HeatMapAffector : MonoBehaviour
    {

        public static List<HeatMapAffector> AllHeatMapAffectors;
        public event EventHandler AffectorUpdate;
        public event EventHandler AffectorRemoved;
        public AnimationCurve HeatAffectCurve = new AnimationCurve(new Keyframe(0, 20), new Keyframe(3, 0));
        public int Layer = 0;
        void Awake()
        {
            if (AllHeatMapAffectors == null)
                AllHeatMapAffectors = new List<HeatMapAffector>();
            AllHeatMapAffectors.Add(this);

            Offset = UnityEngine.Random.Range(-OffsetRange, OffsetRange);

        }
        private float heatElapsedTime;
        [SerializeField]
        private float heatCastCheckTime = .1f;
        private float Offset;
        [SerializeField]
        private float OffsetRange = .01f;
        void Update()
        {
            heatElapsedTime += Time.deltaTime;
            if (heatElapsedTime >= heatCastCheckTime)
            {
                heatElapsedTime = Offset;

                UpdateHeatMap(new EventArgs());
            }
        }

        private void OnDisable()
        {
            AffectorRemove(new EventArgs());
        }

        void UpdateHeatMap(EventArgs e)
        {
            if (AffectorUpdate != null)
            {
                AffectorUpdate(this, e);
            }
        }
        void AffectorRemove(EventArgs e)
        {
            if (AffectorRemoved != null)
            {
                AffectorRemoved(this, e);
            }
        }
    }
}