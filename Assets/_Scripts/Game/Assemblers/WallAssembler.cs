using CosmicShore.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CosmicShore
{

    public class WallAssembler : MonoBehaviour
    {
        public struct BondMate
        {
            public WallAssembler Mate;
            public SiteType Substrate;
            public SiteType Bondee;
        }

        public enum SiteType
        {
            Top,
            Right,
            Bottom,
            Left
        }

        //public bool IsActive = false;
        private Vector3 scale; // Scale of the TrailBlock
        private Vector3 BondSiteTop;
        private Vector3 BondSiteRight;
        private Vector3 BondSiteBottom;
        private Vector3 BondSiteLeft;

        private Vector3 globalBondSiteTop; // Global position of Bond Site A
        private Vector3 globalBondSiteRight; // Global position of Bond Site B
        private Vector3 globalBondSiteBottom;
        private Vector3 globalBondSiteLeft;

        [HideInInspector] public BondMate TopMate;
        [HideInInspector] public WallAssembler RightMate;
        [HideInInspector] public BondMate BottomMate;
        [HideInInspector] public WallAssembler LeftMate;

        [HideInInspector] public bool TopIsBonded = false;
        [HideInInspector] public bool RightIsBonded = false;
        [HideInInspector] public bool BottomIsBonded = false;
        [HideInInspector] public bool LeftIsBonded = false;

        [HideInInspector] public HashSet<WallAssembler> MateList = new();
        public TrailBlock WallBlock;

        private float snapDistance = .2f;
        float separationDistance = 2f;
        [SerializeField] int colliderTheshold = 25;
        [SerializeField] float radius = 40f;

        void Start()
        {
            WallBlock = GetComponent<TrailBlock>();
            scale = WallBlock.TargetScale;
            CalculateGlobalBondSites();
        }

        public void StartBonding()
        {
            StartCoroutine(LookForMates());
        }

        public void ClearMateList()
        {
            foreach (var mate in MateList)
            {
                mate.MateList.Remove(this);
            }
            MateList.Clear();
            TopIsBonded = false;
            RightIsBonded = false;
            BottomIsBonded = false;
            LeftIsBonded = false;
        }

        public void ReplaceMateList(WallAssembler newWallAssembler)
        {
            foreach (var mate in MateList)
            {
                mate.MateList.Add(newWallAssembler);
            }
        }

        IEnumerator LookForMates()
        {
            while (true)
            {
                if (WallBlock == null)
                {
                    yield return new WaitForSeconds(1f);
                    continue;
                }
                if (TopMate.Mate == null)
                {
                    TopMate = FindClosestMate(globalBondSiteTop, SiteType.Top);

                    if (TopMate.Mate)
                    {
                        TopMate.Mate.RightMate = this;
                        MateList.Add(TopMate.Mate);
                        TopMate.Mate.MateList.Add(this);
                        updateTopMate = StartCoroutine(UpdateTopMate());
                    }
                }
                yield return new WaitForSeconds(1f);
                if (BottomMate.Mate == null)
                {
                    BottomMate = FindClosestMate(globalBondSiteBottom, SiteType.Bottom);
                    if (BottomMate.Mate)
                    {
                        BottomMate.Mate.LeftMate = this;
                        MateList.Add(BottomMate.Mate);
                        BottomMate.Mate.MateList.Add(this);
                        updateBottomMate = StartCoroutine(UpdateBottomMate());
                    }
                }
                yield return new WaitForSeconds(1f);
                if (TopIsBonded && BottomIsBonded)
                {
                    //Debug.Log("Bonded Top and Bottom");
                    StopAllCoroutines();
                    WallBlock.Grow();
                    if (TopMate.Mate.MateList.Count < 2) TopMate.Mate.StartBonding();
                    if (BottomMate.Mate.MateList.Count < 2) BottomMate.Mate.StartBonding();

                }
            }
        }

        Coroutine updateTopMate;
        IEnumerator UpdateTopMate()
        {
            while (true)
            {
                yield return null;
                if (TopMate.Mate != null)
                {
                    RotateMate(TopMate, false);
                    MoveMateToSite(TopMate, globalBondSiteTop);
                }
            }
        }

        Coroutine updateBottomMate;
        IEnumerator UpdateBottomMate()
        {
            while (true)
            {
                yield return null;

                if (BottomMate.Mate != null)
                {
                    RotateMate(BottomMate, false);
                    MoveMateToSite(BottomMate, globalBondSiteBottom);
                }
            }
        }

        void CalculateBondSites()
        {
            // Using the bond site calculations from WallAssembler
            BondSiteTop = ((scale.y / 2) + separationDistance) * transform.up;
            BondSiteRight = ((scale.y / 2) - (scale.x / 2)) * transform.up + (((scale.x / 2) + separationDistance) * transform.right);
            BondSiteBottom = -((scale.y / 2) + separationDistance) * transform.up;
            BondSiteLeft = -(((scale.y / 2) - (scale.x / 2)) * transform.up) - ((scale.x / 2) + separationDistance) * transform.right;
        }
        public void CalculateGlobalBondSites()
        {
            CalculateBondSites();

            globalBondSiteTop = transform.position + BondSiteTop;
            globalBondSiteRight = transform.position + BondSiteRight;
            globalBondSiteBottom = transform.position + BondSiteBottom;
            globalBondSiteLeft = transform.position + BondSiteLeft;
        }


        // this method so if checks if this is in each struct in the list
        private bool IsMate(WallAssembler mateComponent)
        {
            return mateComponent.MateList == null ? false : mateComponent.MateList.Count > 0;
        }

        // this method generalize both of the methods above
        private BondMate FindClosestMate(Vector3 bondSite, SiteType siteType)
        {
            float closestDistance = float.MaxValue;
            WallAssembler closest = null;
            SiteType bondee = SiteType.Right;
            var colliders = Physics.OverlapSphere(bondSite, radius); // Adjust radius as needed
            if (colliders.Length < colliderTheshold) return new BondMate { Mate = null};
            foreach (var potentialMate in colliders) // Adjust radius as needed
            {
                WallAssembler mateComponent = potentialMate.GetComponent<WallAssembler>();
                if (mateComponent == null)
                {
                    var trailBlock = potentialMate.GetComponent<TrailBlock>();
                    if (trailBlock != null)
                    {
                        Boid boid = trailBlock.GetComponentInParent<Boid>();
                        if (boid != null)
                        {
                            trailBlock.transform.parent = WallBlock.transform.parent;
                            boid.isKilled = true;
                        }
                        trailBlock.TargetScale = scale;
                        trailBlock.MaxScale = WallBlock.MaxScale;
                        trailBlock.GrowthVector = WallBlock.GrowthVector;
                        trailBlock.Steal(WallBlock.Player, WallBlock.Team);
                        trailBlock.ChangeSize();
                        mateComponent = trailBlock.transform.gameObject.AddComponent<WallAssembler>();
                    }
                    else continue;
                }

                if (IsMate(mateComponent) && mateComponent != this)
                {
                    if (Vector3.Distance(transform.position, mateComponent.transform.position) < snapDistance
                        && mateComponent.WallBlock.TrailBlockProperties.TimeCreated > WallBlock.TrailBlockProperties.TimeCreated) 
                    {
                        mateComponent.StopAllCoroutines();
                        mateComponent.ReplaceMateList(this);
                        mateComponent.ClearMateList();
                    }
                    if (siteType == SiteType.Top && (bondSite - mateComponent.globalBondSiteRight).sqrMagnitude < snapDistance)
                    {
                        //Debug.Log("ReFound MateRight");
                        mateComponent.WallBlock.ActivateSuperShield();
                        return new BondMate { Mate = mateComponent, Substrate = siteType, Bondee = SiteType.Right };
                    }
                    if (siteType == SiteType.Bottom && (bondSite - mateComponent.globalBondSiteLeft).sqrMagnitude < snapDistance)
                    {
                        //Debug.Log("ReFound MateLeft");
                        mateComponent.WallBlock.ActivateSuperShield();
                        return new BondMate { Mate = mateComponent, Substrate = siteType, Bondee = SiteType.Left };
                    }
                }
                if (!IsMate(mateComponent) && mateComponent != this)
                {
                    if (siteType == SiteType.Top)
                    {
                        float distance = (bondSite - mateComponent.globalBondSiteRight).sqrMagnitude;
                        if (distance < closestDistance)
                        {
                            //Debug.Log("Found MateRight");
                            closestDistance = distance;
                            closest = mateComponent;
                            bondee = SiteType.Right;
                        }
                    }
                    else if (siteType == SiteType.Bottom)
                    {
                        float distance = (bondSite - mateComponent.globalBondSiteLeft).sqrMagnitude;
                        if (distance < closestDistance)
                        {
                            //Debug.Log("Found MateLeft");
                            closestDistance = distance;
                            closest = mateComponent;
                            bondee = SiteType.Left;
                        }
                    }
                }
            }
            return new BondMate { Mate = closest, Substrate = siteType,  Bondee = bondee };
        }
  
        private void MoveMateToSite(BondMate mate, Vector3 bondSite)
        {
            {
                var initialPosition = mate.Bondee == SiteType.Right ? mate.Mate.globalBondSiteRight : mate.Mate.globalBondSiteLeft;
                var directionToMate = bondSite - initialPosition;
                
                CalculateGlobalBondSites();
                mate.Mate.transform.position += directionToMate * Time.deltaTime;
                mate.Mate.CalculateGlobalBondSites();
                if (directionToMate.sqrMagnitude < snapDistance)
                {
                    //Debug.Log("Snapped");
                    RotateMate(mate, true);
                    CalculateBondSites();
                    mate.Mate.transform.position = bondSite - (mate.Bondee == SiteType.Right ? mate.Mate.BondSiteRight : mate.Mate.BondSiteLeft);
                    if (mate.Substrate == SiteType.Top)
                    {
                        StopCoroutine(updateTopMate);
                        TopIsBonded = true;
                    }
                    else if (mate.Substrate == SiteType.Bottom)
                    {
                        StopCoroutine(updateBottomMate);
                        BottomIsBonded = true;
                    }
                }
                
            }
        }
        
        private void RotateMate(BondMate mate, bool isSnapping)
        {
            int signRight = mate.Bondee == SiteType.Right ? 1 : -1;
            int signTop = mate.Substrate == SiteType.Top ? 1 : -1;
            Quaternion targetRotation = Quaternion.LookRotation(transform.forward, signRight * signTop * transform.right);
            mate.Mate.transform.rotation = isSnapping? 
                targetRotation : 
                Quaternion.Slerp(mate.Mate.transform.rotation, targetRotation, Time.deltaTime); // Adjust rotation speed as needed
            mate.Mate.CalculateGlobalBondSites();
            CalculateGlobalBondSites();
        }

    }
}