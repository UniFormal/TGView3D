/************************************************************************************

Copyright   :   Copyright 2017 Oculus VR, LLC. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.4.1 (the "License");
you may not use the Oculus VR Rift SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

https://developer.oculus.com/licenses/sdk-3.4.1

Unless required by applicable law or agreed to in writing, the Oculus VR SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

using System.Collections.Generic;
using UnityEngine;


namespace OVRTouchSample
{
    /// <summary>
    /// Allows grabbing and throwing of objects with the DistanceGrabbable component on them.
    /// </summary>
    /// 



    [RequireComponent(typeof(Rigidbody))]
    public class DistanceGrabber : OVRGrabber
    {



        // Radius of sphere used in spherecast from hand along forward ray to find target object.
        [SerializeField]
        float m_spherecastRadius;

        // Distance below which no-snap objects won't be teleported, but will instead be left
        // where they are in relation to the hand.
        [SerializeField]
        float m_noSnapThreshhold = 0.05f;

        [SerializeField]
        bool m_useSpherecast;
        public bool UseSpherecast
        {
            get { return m_useSpherecast; }
            set 
            {
                m_useSpherecast = value;
                GrabVolumeEnable(!m_useSpherecast);
            }
        }

        // Public to allow changing in demo.
        [SerializeField]
        public bool m_preventGrabThroughWalls;

        [SerializeField]
        float m_objectPullVelocity = 10.0f;
        float m_objectPullMaxRotationRate = 360.0f; // max rotation rate in degrees per second

        bool m_movingObjectToHand = false;

        // Objects can be distance grabbed up to this distance from the hand.
        [SerializeField]
        public float m_maxGrabDistance;

        // Only allow grabbing objects in this layer.
        [SerializeField]
        int m_grabObjectsInLayer;
        [SerializeField]
        int m_obstructionLayer;

        [SerializeField]
        GameObject m_player;
        public DistanceGrabber m_otherHand;

        protected DistanceGrabbable m_target;
        // Tracked separately from m_target, because we support child colliders of a DistanceGrabbable.
        // MTF TODO: verify this still works!
        protected Collider m_targetCollider;

        OVRCameraRig m_camera;
        private TGraph.ReadJSON.MyGraph graph;

      
        private int handIndex = 0;
        public delegate void SelectedChange();
        public static event SelectedChange OnSelectionChanged;
        private bool ReadyToSnapTurn;
        Color baseColor = Color.white;
        Color selectedColor = Color.cyan;
        Color connectedColor = Color.yellow;
        Color targetColor = Color.red;

        protected override void Start()
        {

            
            base.Start();

            // Set up our max grab distance to be based on the player's max grab distance.
            // Adding a liberal margin of error here, because users can move away some from the 
            // OVRPlayerController, and also players have arms.
            // Note that there's no major downside to making this value too high, as objects
            // outside the player's grabbable trigger volume will not be eligible targets regardless.
            SphereCollider sc = m_player.GetComponentInChildren<SphereCollider>();
            m_maxGrabDistance = sc.radius + 3.0f;

            if(m_parentHeldObject == true)
            {
                Debug.LogError("m_parentHeldObject incompatible with DistanceGrabber. Setting to false.");
                m_parentHeldObject = false;
            }

            DistanceGrabber[] grabbers = FindObjectsOfType<DistanceGrabber>();
            for (int i = 0; i < grabbers.Length; ++i)
            {
                if (grabbers[i] != this) m_otherHand = grabbers[i];
            }
            Debug.Assert(m_otherHand != null);
            m_camera = GameObject.Find("OVRCameraRig").GetComponent<OVRCameraRig>();
        


        }

        void Update()
        {
            DistanceGrabbable target;
            Collider targetColl;
            FindTarget(out target, out targetColl);

            if (target != m_target)
            {
                if (m_target != null)
                {
                    //m_target.Highlight = m_otherHand.m_target == m_target;
                    m_target.Targeted = m_otherHand.m_target == m_target;
                }
                m_target = target;
                m_targetCollider = targetColl;
                if (m_target != null)
                {
                    //m_target.Highlight = true;
                    m_target.Targeted = true;
                }
            }

            /*
            if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickLeft) )
            {

                if (ReadyToSnapTurn)
                {
                    if (graph.currentTarget != -1 && graph.currentTarget != graph.nodes[graph.latestSelection].connectedNodes.Count)
                         graph.nodes[graph.nodes[graph.latestSelection].connectedNodes[graph.currentTarget]].nodeObject.GetComponent<MeshRenderer>().material.color = connectedColor;
                    graph.currentTarget = (graph.currentTarget - 1) % (graph.nodes[graph.latestSelection].connectedNodes.Count + 1);
                    if (OnSelectionChanged != null)
                        OnSelectionChanged();
                    ReadyToSnapTurn = false;

                }

                if (graph.currentTarget == graph.nodes[graph.latestSelection].connectedNodes.Count)
                {
                    m_camera.transform.rotation = Quaternion.identity;
                }
                else
                {
                    graph.nodes[graph.nodes[graph.latestSelection].connectedNodes[graph.currentTarget]].nodeObject.GetComponent<MeshRenderer>().material.color = targetColor;
                    Vector3 relativePos = graph.nodes[graph.nodes[graph.latestSelection].connectedNodes[graph.currentTarget]].pos - m_camera.transform.position;
                    relativePos.y = 0;
                    Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
                    m_camera.transform.rotation = rotation;
                }



            }
            else*/
            if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickRight) && graph!=null &&graph.latestSelection == graph.selectedNodes[handIndex])
            {

                if (ReadyToSnapTurn)
                {

                    Debug.Log(graph.currentTarget);
                    Debug.Log(graph.latestSelection);
                    Debug.Log(graph.nodes[graph.latestSelection].connectedNodes.Count);
                    if (graph.currentTarget != -1 && graph.currentTarget != graph.nodes[graph.latestSelection].connectedNodes.Count)
                        graph.nodes[graph.nodes[graph.latestSelection].connectedNodes[graph.currentTarget]].labelObject.GetComponent<TextMesh>().color =connectedColor;
                    graph.currentTarget = (graph.currentTarget + 1) % (graph.nodes[graph.latestSelection].connectedNodes.Count+1);
                    if (OnSelectionChanged != null)
                        OnSelectionChanged();
                    ReadyToSnapTurn = false;
                   
                }

                if (graph.currentTarget == graph.nodes[graph.latestSelection].connectedNodes.Count)
                {
                    m_camera.transform.rotation = Quaternion.identity;
                }
                else
                {
                    graph.nodes[graph.nodes[graph.latestSelection].connectedNodes[graph.currentTarget]].labelObject.GetComponent<TextMesh>().color = targetColor;
                    Vector3 relativePos = graph.nodes[graph.nodes[graph.latestSelection].connectedNodes[graph.currentTarget]].nodeObject.transform.position - m_camera.transform.position;
                    Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
                    rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, 0);
                    m_camera.transform.localRotation = rotation;
                }
            }

            else
            {
                ReadyToSnapTurn = true;
            }

            



        }




        protected override void GrabEnd()
        {
            if (m_grabbedObj != null)
            {
                m_grabbedObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

                graph.movingNodes.Remove(m_grabbedObj.transform.GetSiblingIndex());
           
                OVRPose localPose = new OVRPose { position = OVRInput.GetLocalControllerPosition(m_controller), orientation = OVRInput.GetLocalControllerRotation(m_controller) };
                OVRPose offsetPose = new OVRPose { position = m_anchorOffsetPosition, orientation = m_anchorOffsetRotation };
                localPose = localPose * offsetPose;

                OVRPose trackingSpace = transform.ToOVRPose() * localPose.Inverse();
                Vector3 linearVelocity = trackingSpace.orientation * OVRInput.GetLocalControllerVelocity(m_controller);
                Vector3 angularVelocity = trackingSpace.orientation * OVRInput.GetLocalControllerAngularVelocity(m_controller);

                GrabbableRelease(linearVelocity, angularVelocity);
            }


            // Re-enable grab volumes to allow overlap events
            GrabVolumeEnable(true);
        }

        protected override void GrabBegin()
        {
            DistanceGrabbable closestGrabbable = m_target;
            Collider closestGrabbableCollider = m_targetCollider;

            GrabVolumeEnable(false);

            if (closestGrabbable != null)
            {
                if (closestGrabbable.isGrabbed)
                {
                    ((DistanceGrabber)closestGrabbable.grabbedBy).OffhandGrabbed(closestGrabbable);
                }

                m_grabbedObj = closestGrabbable;

                m_grabbedObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                if (graph == null)
                {
                    graph = TGraph.GlobalVariables.Graph;
                    handIndex = graph.handIndex;
                    graph.handIndex++;
                }


                //current Selection exists
                if (graph.selectedNodes[handIndex] != -1)
                {
                    var graphNode = graph.nodes[graph.selectedNodes[handIndex]];
               
                    graphNode.labelObject.GetComponent<TextMesh>().color = baseColor;
                    graphNode.labelObject.layer = 18;
                    foreach (int nidx in graphNode.connectedNodes)
                    {
                        graph.nodes[nidx].labelObject.layer = 18;
                        graph.nodes[nidx].labelObject.GetComponent<TextMesh>().color = baseColor;
                    }
                    GameObject.Destroy(graphNode.nodeEdgeObject);
                    graphNode.nodeEdgeObject = null;
                }




                TGraph.ReadJSON.MyNode node = graph.nodes[closestGrabbable.transform.GetSiblingIndex()];


               // Debug.Log(closestGrabbable.transform.GetSiblingIndex() + " other has"+ graph.selectedNodes[(handIndex + 1) % 2]);
                

                if (closestGrabbable.transform.GetSiblingIndex() != graph.selectedNodes[(handIndex + 1) % 2] )
                {

                    var edges = new List<TGraph.ReadJSON.MyEdge>();
                    node.labelObject.GetComponent<TextMesh>().color = selectedColor;
                    node.labelObject.layer = 0;
                    foreach (int nidx in node.connectedNodes)
                    {
                        graph.nodes[nidx].labelObject.layer = 0;
                        graph.nodes[nidx].labelObject.GetComponent<TextMesh>().color = connectedColor;
                    }
                    foreach (int idx in node.edgeIndicesIn)
                    {
                        edges.Add(graph.edges[idx]);
                    }
                    foreach (int idx in node.edgeIndicesOut)
                    {
                        edges.Add(graph.edges[idx]);
                    }


                    graph.nodes[closestGrabbable.transform.GetSiblingIndex()].nodeEdgeObject = TGraph.ReadJSON.BuildEdges(edges,ref graph, graph.edgeObject.GetComponent<MeshRenderer>().sharedMaterial);
                    graph.nodes[closestGrabbable.transform.GetSiblingIndex()].nodeEdgeObject.transform.parent = graph.edgeObject.transform.parent;
                    graph.nodes[closestGrabbable.transform.GetSiblingIndex()].nodeEdgeObject.transform.localPosition = Vector3.zero;
                    graph.nodes[closestGrabbable.transform.GetSiblingIndex()].nodeEdgeObject.transform.localEulerAngles = Vector3.zero;

                    graph.selectedNodes[handIndex] = (closestGrabbable.transform.GetSiblingIndex());
                    graph.latestSelection = closestGrabbable.transform.GetSiblingIndex();
                    graph.currentTarget = -1;

                    if (OnSelectionChanged!= null)
                        OnSelectionChanged();


                }
                else
                {
                    graph.selectedNodes[handIndex] = graph.nodes[graph.selectedNodes[(handIndex + 1) % 2]].nr;
                    graph.selectedNodes[(handIndex + 1) % 2] = -1 ;
                }



                Debug.Log(graph.selectedNodes[handIndex ] + " other:" + graph.selectedNodes[(handIndex + 1) % 2]);


                /*
                for (int i = 0; i < removeList.Count; i++)
                {
                    int idx = removeList[i];
           
                    graph.selectedNodes.Remove(graph.nodes[idx]);

                    if (!graph.selectedNodes.Contains(graph.nodes[idx]))
                    {
                        graph.nodes[idx].labelObject.GetComponent<TextMesh>().color = new Color(0.87f, 0.87f, 0.7f);
                        foreach (int nidx in graph.nodes[idx].connectedNodes)
                        {
                            graph.nodes[nidx].labelObject.layer = 18;
                            graph.nodes[nidx].labelObject.GetComponent<TextMesh>().color = new Color(0.87f, 0.87f, 0.7f);
                        }
                        GameObject.Destroy(graph.nodes[idx].nodeEdgeObject);
                    }


                    removeList.Remove(idx);
                    i--;
                   
                }



              if (!graph.selectedNodes.Contains(graph.nodes[closestGrabbable.transform.GetSiblingIndex()]))
              {
                    var edges = new List<TGraph.ReadJSON.MyEdge>();
                    // foreach (TGraph.ReadJSON.MyNode node in graph.selectedNodes)
                    {
                        TGraph.ReadJSON.MyNode node = graph.nodes[closestGrabbable.transform.GetSiblingIndex()];

                        node.labelObject.GetComponent<TextMesh>().color = new Color(0.6f, 0.6f, 0.05f);
                        foreach (int nidx in node.connectedNodes)
                        {
                            graph.nodes[nidx].labelObject.layer = 0;
                            graph.nodes[nidx].labelObject.GetComponent<TextMesh>().color = new Color(0.6f, 0.6f, 0.05f);
                        }
                        foreach (int idx in node.edgeIndicesIn)
                        {
                            edges.Add(graph.edges[idx]);
                        }
                        foreach (int idx in node.edgeIndicesOut)
                        {
                            edges.Add(graph.edges[idx]);
                        }
                    }

                    graph.nodes[closestGrabbable.transform.GetSiblingIndex()].nodeEdgeObject = TGraph.ReadJSON.BuildEdges(edges, graph, graph.nodeDict, graph.edgeObject.GetComponent<MeshRenderer>().sharedMaterial);
                    graph.selectedNodes.Add(graph.nodes[closestGrabbable.transform.GetSiblingIndex()]);
                }*/



                graph.movingNodes.Add(closestGrabbable.transform.GetSiblingIndex());

                m_grabbedObj.GrabBegin(this, closestGrabbableCollider);

                m_movingObjectToHand = true;
                m_lastPos = transform.position;
                m_lastRot = transform.rotation;

                // If it's within a certain distance respect the no-snap.
                Vector3 closestPointOnBounds = closestGrabbableCollider.ClosestPointOnBounds(m_gripTransform.position);
                if(!m_grabbedObj.snapPosition && !m_grabbedObj.snapOrientation && m_noSnapThreshhold > 0.0f && (closestPointOnBounds - m_gripTransform.position).magnitude < m_noSnapThreshhold)
                {
                    Vector3 relPos = m_grabbedObj.transform.position - transform.position;
                    m_movingObjectToHand = false;
                    relPos = Quaternion.Inverse(transform.rotation) * relPos;
                    m_grabbedObjectPosOff = relPos;
                    Quaternion relOri = Quaternion.Inverse(transform.rotation) * m_grabbedObj.transform.rotation;
                    m_grabbedObjectRotOff = relOri;
                }
                else
                {
                    // Set up offsets for grabbed object desired position relative to hand.
                    m_grabbedObjectPosOff = m_gripTransform.localPosition;
                    if (m_grabbedObj.snapOffset)
                    {
                        Vector3 snapOffset = m_grabbedObj.snapOffset.position;
                        if (m_controller == OVRInput.Controller.LTouch) snapOffset.x = -snapOffset.x;
                        m_grabbedObjectPosOff += snapOffset;
                    }

                    m_grabbedObjectRotOff = m_gripTransform.localRotation;
                    if (m_grabbedObj.snapOffset)
                    {
                        m_grabbedObjectRotOff = m_grabbedObj.snapOffset.rotation * m_grabbedObjectRotOff;
                    }
                }

            }
        }

        protected override void MoveGrabbedObject(Vector3 pos, Quaternion rot, bool forceTeleport = false)
        {
            if (m_grabbedObj == null)
            {
                return;
            }

            Rigidbody grabbedRigidbody = m_grabbedObj.grabbedRigidbody;
            Vector3 grabbablePosition = pos + rot * m_grabbedObjectPosOff;
            Quaternion grabbableRotation = rot * m_grabbedObjectRotOff;

            if (m_movingObjectToHand)
            {
                float travel = m_objectPullVelocity * Time.deltaTime;
                Vector3 dir = grabbablePosition - m_grabbedObj.transform.position;
                if(travel * travel * 1.1f > dir.sqrMagnitude)
                {
                    GameObject.Find("LCone").GetComponent<MeshRenderer>().enabled = false;
                    GameObject.Find("RCone").GetComponent<MeshRenderer>().enabled = false;
                    m_movingObjectToHand = false;
                }
                else
                {
                    dir.Normalize();
                    grabbablePosition = m_grabbedObj.transform.position + dir * travel;
                    grabbableRotation = Quaternion.RotateTowards(m_grabbedObj.transform.rotation, grabbableRotation, m_objectPullMaxRotationRate * Time.deltaTime);
                }
            }
            grabbedRigidbody.MovePosition(grabbablePosition);
            grabbedRigidbody.MoveRotation(grabbableRotation);
        }

        static private DistanceGrabbable HitInfoToGrabbable(RaycastHit hitInfo)
        {
            if (hitInfo.collider != null)
            {
                GameObject go = hitInfo.collider.gameObject;
                return go.GetComponent<DistanceGrabbable>() ?? go.GetComponentInParent<DistanceGrabbable>();
            }
            return null;
        }

        protected bool FindTarget(out DistanceGrabbable dgOut, out Collider collOut)
        {
            dgOut = null;
            collOut = null;
            float closestMagSq = float.MaxValue;

            // First test for objects within the grab volume, if we're using those.
            // (Some usage of DistanceGrabber will not use grab volumes, and will only 
            // use spherecasts, and that's supported.)
            foreach (ColorGrabbable cg in m_grabCandidates.Keys)
            {
                DistanceGrabbable grabbable = cg as DistanceGrabbable;
                bool canGrab = grabbable != null && grabbable.InRange && !(grabbable.isGrabbed && !grabbable.allowOffhandGrab);
                if (!canGrab)
                {
                    continue;
                }

                for (int j = 0; j < grabbable.grabPoints.Length; ++j)
                {
                    Collider grabbableCollider = grabbable.grabPoints[j];
                    // Store the closest grabbable
                    Vector3 closestPointOnBounds = grabbableCollider.ClosestPointOnBounds(m_gripTransform.position);
                    float grabbableMagSq = (m_gripTransform.position - closestPointOnBounds).sqrMagnitude;
                    if (grabbableMagSq < closestMagSq)
                    {
                        bool accept = true;
                        if(m_preventGrabThroughWalls)
                        {
                            // NOTE: if this raycast fails, ideally we'd try other rays near the edges of the object, especially for large objects.
                            // NOTE 2: todo optimization: sort the objects before performing any raycasts.
                            Ray ray = new Ray();
                            ray.direction = grabbable.transform.position - m_gripTransform.position;
                            ray.origin = m_gripTransform.position;
                            RaycastHit obstructionHitInfo;
                            Debug.DrawRay(ray.origin, ray.direction, Color.red, 0.1f);

                            if (Physics.Raycast(ray, out obstructionHitInfo, m_maxGrabDistance, 1 << m_obstructionLayer))
                            {
                                float distToObject = (grabbableCollider.ClosestPointOnBounds(m_gripTransform.position) - m_gripTransform.position).magnitude;
                                if(distToObject > obstructionHitInfo.distance * 1.1)
                                {
                                    accept = false;
                                }
                            }
                        }
                        if(accept)
                        {
                            closestMagSq = grabbableMagSq;
                            dgOut = grabbable;
                            collOut = grabbableCollider;
                        }
                    }
                }
            }

            if (dgOut == null && m_useSpherecast)
            {
                return FindTargetWithSpherecast(out dgOut, out collOut);
            }
            return dgOut != null;
        }

        protected bool FindTargetWithSpherecast(out DistanceGrabbable dgOut, out Collider collOut)
        {
            dgOut = null;
            collOut = null;
            Ray ray = new Ray(m_gripTransform.position, m_gripTransform.forward);
            RaycastHit hitInfo;

            // If no objects in grab volume, raycast.
            // Potential optimization: 
            // In DistanceGrabbable.RefreshCrosshairs, we could move the object between collision layers.
            // If it's in range, it would move into the layer DistanceGrabber.m_grabObjectsInLayer,
            // and if out of range, into another layer so it's ignored by DistanceGrabber's SphereCast.
            // However, we're limiting the SphereCast by m_maxGrabDistance, so the optimization doesn't seem
            // essential.
            if (Physics.SphereCast(ray, m_spherecastRadius, out hitInfo, m_maxGrabDistance, 1 << m_grabObjectsInLayer))
            {
                DistanceGrabbable grabbable = null;
                Collider hitCollider = null;
                if (hitInfo.collider != null)
                {
                    grabbable = hitInfo.collider.gameObject.GetComponentInParent<DistanceGrabbable>();
                    hitCollider = grabbable == null ? null : hitInfo.collider;
                }

                if (grabbable != null && m_preventGrabThroughWalls)
                {
                    // Found a valid hit. Now test to see if it's blocked by collision.
                    RaycastHit obstructionHitInfo;
                    ray.direction = hitInfo.point - m_gripTransform.position;

                    dgOut = grabbable;
                    collOut = hitCollider;
                    if (Physics.Raycast(ray, out obstructionHitInfo, 1 << m_obstructionLayer))
                    {
                        DistanceGrabbable obstruction = null;
                        if(hitInfo.collider != null)
                        {
                            obstruction = obstructionHitInfo.collider.gameObject.GetComponentInParent<DistanceGrabbable>();
                        }
                        if (obstruction != grabbable && obstructionHitInfo.distance < hitInfo.distance)
                        {
                            dgOut = null;
                            collOut = null;
                        }
                    }
                }
            }
            return dgOut != null;
        }

        protected override void GrabVolumeEnable(bool enabled)
        {
            if(m_useSpherecast) enabled = false;
            base.GrabVolumeEnable(enabled);
        }

        // Just here to allow calling of a protected member function.
	    protected override void OffhandGrabbed(OVRGrabbable grabbable)
        {
            base.OffhandGrabbed(grabbable);
        }
    }
}
