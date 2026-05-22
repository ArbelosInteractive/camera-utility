using System;
using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arbelos.CameraUtility.Runtime
{
    public class CinemachineStateMachine : MonoBehaviour
    {
        private CinemachineCamera behaviourCamera;
        [SerializeField][Tooltip("Assign the list of cinemachine states you want to add to this state machine")] private List<CinemachineState> states = new List<CinemachineState>();
        [SerializeField][Tooltip("Time taken to smoothly switch from one state to another")] private float switchStateDuration = 0.5f;
        public Camera originalCamera;
        [SerializeField][Tooltip("Original target cinemachine virtual camera game object.")] private GameObject originalVirtualCamera;
        private CinemachineState activeState;
        
        void Awake()
        {
            if(originalVirtualCamera && originalVirtualCamera.GetComponent<CinemachineCamera>())
            {
                StartCoroutine(SetOriginalCamera(originalVirtualCamera.GetComponent<CinemachineCamera>()));
            }
            AssignRefToChildStates();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public CinemachineCamera GetBehaviourCamera()
        {
            if(behaviourCamera != null)
            {
                return behaviourCamera;
            }
            else if(originalVirtualCamera != null)
            {
                return originalVirtualCamera.GetComponent<CinemachineCamera>();
            }
            return null;
        }

        public void SetOriginalCamera(Camera camera)
        {
            originalCamera = camera;
        }

        public IEnumerator SetOriginalCamera(CinemachineCamera virtualCamera)
        {
            originalVirtualCamera = virtualCamera.gameObject;
            var cameras = FindObjectsOfType<CinemachineBrain>(true);
        
            //Wait until cameras are initialized property and a camera has a active virtual camera to search from.
            while (!Array.Exists(cameras,
                       x => x.ActiveVirtualCamera != null && x.ActiveVirtualCamera.IsValid))
            {
                yield return null;
            }
            
            foreach (var camera in cameras)
            {
                if ((CinemachineCamera)camera.ActiveVirtualCamera == originalVirtualCamera.GetComponent<CinemachineCamera>())
                {
                    originalCamera = camera.OutputCamera;
                    break;
                }
            }
        }

        private void AssignRefToChildStates()
        {
            for (int i = 0; i < states.Count; i++) 
            {
                states[i].SetParentStateMachine(this);
            }
        }

        public void BeginState(string _stateName)
        {
            StartCoroutine(ActivateState(_stateName));
        }

        public IEnumerator ActivateState(string _stateName)
        {
            //Wait for original camera to be setup
            while (originalCamera == null)
            {
                yield return null;
            }
            
            var stateToBegin = states.Find(x => x.GetName() == _stateName);
            if(stateToBegin)
            {
                if(stateToBegin.GetStateType() == CinemachineStateType.Shake)
                {
                    stateToBegin.BeginState();
                }
                else
                {
                    //Only instantiate once if behavior camera is null.
                    if(behaviourCamera == null)
                    {
                        //Instantiate a virtual camera
                        GameObject gameObjectToInstantiate = Instantiate(originalVirtualCamera, this.transform);
                        behaviourCamera = gameObjectToInstantiate.GetComponent<CinemachineCamera>();
                        gameObjectToInstantiate.transform.position = originalVirtualCamera.transform.position;
                        gameObjectToInstantiate.transform.rotation = originalVirtualCamera.transform.rotation;
                        gameObjectToInstantiate.transform.localScale = originalVirtualCamera.transform.localScale;
                        behaviourCamera.Priority.Value = originalVirtualCamera.GetComponent<CinemachineCamera>().Priority.Value + 1;
                    }
                    stateToBegin.BeginState();
                }
                
                Debug.Log($"[Cinemachine State Machine] Beginning State: {_stateName}");
            }
            else
            {
                Debug.LogError("No state found with the name: " + _stateName);
            }
        }

        public void EndState()
        {
            if(activeState != null)
            {
                activeState.StopAllCoroutines();
                activeState.EndState(true);

                //For the scenario where a chained state has been staryed again and activate state has been assigned again.
                if(activeState == null)
                    StartCoroutine(RevertToOriginalState());
            }
          
        }

        public void SwitchActiveState()
        {
            if(activeState != null)
            {
                StopAllCoroutines();
                activeState.StopAllCoroutines();
                activeState.EndState(false);
            }
        }

        public float GetSwitchStateDuration()
        {
            return switchStateDuration;
        }

        IEnumerator RevertToOriginalState()
        {
            //Reset the priority back to original
            behaviourCamera.Priority.Value = originalVirtualCamera.GetComponent<CinemachineCamera>().Priority.Value - 1;

            //Wait for late update blend to start before checking if it is complete.
            yield return new WaitForSeconds(0.1f);

            //Wait for switch blend to complete.
            while(originalCamera.GetComponent<CinemachineBrain>().ActiveBlend != null)
            {
                yield return null;
            }


            Destroy(behaviourCamera.gameObject);
            behaviourCamera = null;
        }

        public void SetActiveState(CinemachineState _state)
        {
            activeState = states.Find(x => x == _state);
        }

        public CinemachineState GetActiveState()
        {
            return activeState;
        }


        public void DebugEnterFunction()
        {
            Debug.Log("Enter event function called");
        }

        public void DebugExitFunction()
        {
            Debug.Log("Exit event function called");
        }

        public CinemachineState GetState(string stateName)
        {
            return states.Find(x => x.GetName() == stateName);
        }
    }
}