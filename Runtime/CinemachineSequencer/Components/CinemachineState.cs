using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Arbelos.CameraUtility.Runtime
{
    [RequireComponent(typeof(CinemachineSmoothPath))]
    public class CinemachineState : MonoBehaviour
    {
        [Header("State Information")]
        [SerializeField] private string stateName;
        [SerializeField] private CinemachineStateType type;
        [SerializeField] private float cameraSpeed;
        [SerializeField] private float duration;
        [SerializeField] private Transform focusObject;
        [SerializeField] private UnityEvent stateEnterEvent;
        [SerializeField] private UnityEvent stateExitEvent;

        [SerializeField] private float shakeAmplitude;
        [SerializeField] private float shakeFrequency;
        [SerializeField] private NoiseSettings shakeSettings;
        [SerializeField] private int numOfCycles;

        [SerializeField] private float zoomTargetFOV;
        [SerializeField] private float zoomDistanceFromTarget;
        [SerializeField] private ZoomDirection zoomInDirection;
        [SerializeField] private AnimationCurve zoomCurve;

        private CinemachineStateMachine parentStateMachine;
        private CinemachineVirtualCamera targetCam;
        private CinemachineSmoothPath path;
        private bool stateActive;
        private float initialFOV;

        // Start is called before the first frame update
        void Start()
        {
            path = gameObject.GetComponent<CinemachineSmoothPath>();
        }

        // Update is called once per frame
        void Update()
        {
        }

        public string GetName()
        {
            return stateName;
        }

        public CinemachineStateMachine GetParentStateMachine()
        {
            return parentStateMachine;
        }

        public void SetParentStateMachine(CinemachineStateMachine stateMachine)
        {
            this.parentStateMachine = stateMachine;
        }

        public CinemachineSmoothPath GetPath()
        {
            return path;
        }

        public void SetPath(CinemachineSmoothPath path)
        {
            this.path = path;
        }

        public CinemachineStateType GetStateType()
        {
            return type;
        }

        public void SetStateType(CinemachineStateType type)
        {
            this.type = type;
        }

        public void BeginState()
        {
            if(!stateActive)
            {
                stateActive = true;
                targetCam = parentStateMachine.GetBehaviourCamera();
                //Every state clears the current active state before starting except for shake state as it can be called multiple times
                stateEnterEvent?.Invoke();
                switch (type)
                {
                    case CinemachineStateType.DollyPath:
                        EndCurrentState();
                        parentStateMachine.SetActiveState(this);
                        BeginDollyPath();
                        break;
                    case CinemachineStateType.ClashZoom:
                        EndCurrentState();
                        parentStateMachine.SetActiveState(this);
                        BeginClashZoom();
                        break;
                    case CinemachineStateType.Shake:
                        BeginShake();
                        break;
                    case CinemachineStateType.AutoPan:
                        EndCurrentState();
                        parentStateMachine.SetActiveState(this);
                        BeginAutoPan();
                        break;
                }
            }  
        }

        private void EndCurrentState()
        {
            if(parentStateMachine.GetActiveState() != null )
            {
                parentStateMachine.SwitchActiveState();
            }
        }

        private void BeginDollyPath()
        {
            if (path.m_Waypoints.Length <= 1)
            {
                Debug.LogError("The path atleast need to have 2 waypoints to travel for DollyPath state!");
                return;
            }
            
            if (focusObject != null)
            {
                targetCam.LookAt = focusObject;
            }
            else
            {
                targetCam.LookAt = null;
            }

            StartCoroutine(TravelDollyPath());
        }

        IEnumerator TravelDollyPath()
        {
            // Calculate the first waypoint's position and rotation
            Vector3 firstWaypointPosition = path.EvaluatePositionAtUnit(0f, CinemachinePathBase.PositionUnits.Distance);
            Quaternion firstWaypointRotation = Quaternion.LookRotation(path.EvaluateTangentAtUnit(0f, CinemachinePathBase.PositionUnits.Distance));

            // Smoothly move the camera from its current position to the first waypoint
            yield return StartCoroutine(SmoothMoveToFirstWaypoint(targetCam.transform, firstWaypointPosition, null, firstWaypointRotation));

            float pathPosition = 0f;
            float pathLength = path.PathLength;
            int currentCycle = 0;
            float cycleDuration = pathLength / cameraSpeed;

            while (currentCycle < 1)
            {
                float elapsedTime = 0f;

                while (elapsedTime < cycleDuration)
                {
                    elapsedTime += Time.deltaTime;
                    pathPosition = (elapsedTime / cycleDuration) * pathLength;

                    // Get the position on the path
                    Vector3 worldPosition = path.EvaluatePositionAtUnit(pathPosition, CinemachinePathBase.PositionUnits.Distance);
                    targetCam.gameObject.transform.position = worldPosition;

                    // Determine the camera's rotation based on whether the lookAt target is set
                    if (targetCam.LookAt != null)
                    {
                        // Look at the lookAt target
                        Vector3 directionToTarget = (targetCam.LookAt.position - targetCam.gameObject.transform.position).normalized;
                        Quaternion lookAtRotation = Quaternion.LookRotation(directionToTarget);
                        targetCam.gameObject.transform.rotation = lookAtRotation;
                    }
                    else
                    {
                        // Look in the direction of the path
                        Quaternion worldRotation = Quaternion.LookRotation(path.EvaluateTangentAtUnit(pathPosition, CinemachinePathBase.PositionUnits.Distance));
                        targetCam.gameObject.transform.rotation = worldRotation;
                    }

                    yield return null;
                }

                currentCycle++;
            }
            parentStateMachine.EndState();
        }

        IEnumerator SmoothMoveToFirstWaypoint(Transform target, Vector3 destinationPosition, Transform lookAtTarget = null, Quaternion? fixedRotation = null)
        {
            Vector3 startPosition = target.position;
            Quaternion startRotation = target.rotation;
            float elapsedTime = 0f;
            float switchDuration = parentStateMachine.GetSwitchStateDuration();

            while (elapsedTime < switchDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / switchDuration;

                // Smooth interpolation for position
                target.position = Vector3.Lerp(startPosition, destinationPosition, t);

                if (lookAtTarget != null)
                {
                    // Continuously look at the target
                    Vector3 directionToLookAt = lookAtTarget.position - target.position;
                    Quaternion lookRotation = Quaternion.LookRotation(directionToLookAt);
                    target.rotation = Quaternion.Slerp(startRotation, lookRotation, t);
                }
                else if (fixedRotation.HasValue)
                {
                    // Smooth interpolation for rotation based on the fixed rotation
                    target.rotation = Quaternion.Slerp(startRotation, fixedRotation.Value, t);
                }

                yield return null;
            }

            // Ensure the final position is exactly at the destination
            target.position = destinationPosition;

            // Final rotation handling
            if (lookAtTarget != null)
            {
                // Final look at the target
                Vector3 finalDirectionToLookAt = lookAtTarget.position - target.position;
                target.rotation = Quaternion.LookRotation(finalDirectionToLookAt);
            }
            else if (fixedRotation.HasValue)
            {
                target.rotation = fixedRotation.Value;
            }
        }


        private void BeginClashZoom()
        {
            if (focusObject == null)
            {
                Debug.LogError("Focus object is not assigned for AutoPan state!");
                return;
            }
            targetCam.LookAt = focusObject;
            initialFOV = targetCam.m_Lens.FieldOfView;
            StartCoroutine(StartClashZoom());
        }

        IEnumerator InitializeZoomPosition()
        {
            Vector3 newPos = new Vector3(0, 0, 0);
            switch (zoomInDirection)
            {
                case ZoomDirection.North:
                    newPos = new Vector3(0, targetCam.LookAt.transform.position.y, targetCam.LookAt.transform.position.z - zoomDistanceFromTarget);
                    break;
                case ZoomDirection.South:
                    newPos = new Vector3(0, targetCam.LookAt.transform.position.y, targetCam.LookAt.transform.position.z + zoomDistanceFromTarget);
                    break;
                case ZoomDirection.East:
                    newPos = new Vector3(targetCam.LookAt.transform.position.x + zoomDistanceFromTarget, targetCam.LookAt.transform.position.y, 0);
                    break;
                case ZoomDirection.West:
                    newPos = new Vector3(targetCam.LookAt.transform.position.x - zoomDistanceFromTarget, targetCam.LookAt.transform.position.y, 0);
                    break;
            }
            // Smoothly move the camera from its current position to the start position
            yield return StartCoroutine(SmoothMoveToFirstWaypoint(targetCam.transform, newPos, targetCam.LookAt));
        }

        IEnumerator StartClashZoom()
        {
            yield return StartCoroutine(InitializeZoomPosition());

            float timeElapsed = 0f; // Track the time elapsed

            // While the time elapsed is less than the duration of the zoom effect
            while (timeElapsed < duration)
            {
                // Normalize the elapsed time (0 to 1 over the duration)
                float normalizedTime = timeElapsed / duration;

                // Use the animation curve to determine the zoom velocity
                float curveValue = zoomCurve.Evaluate(normalizedTime);

                // Calculate the new FOV for this frame based on the curve value
                float newFOV = Mathf.Lerp(initialFOV, zoomTargetFOV, curveValue);

                // Apply the new FOV
                targetCam.m_Lens.FieldOfView = newFOV;

                // Wait for the next frame
                yield return null;

                // Increment the time elapsed
                timeElapsed += Time.deltaTime;
            }

            // Ensure the final FOV is exactly the target FOV
            targetCam.m_Lens.FieldOfView = zoomTargetFOV;

            //Stays in the zoom state for selected duration.
            yield return new WaitForSeconds(duration);

            parentStateMachine.EndState();
        }

        private void BeginShake()
        {
            if(targetCam != null)
            {
                var channel = targetCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                if (channel != null)
                {
                    channel.m_AmplitudeGain = shakeAmplitude;
                    channel.m_NoiseProfile = shakeSettings;
                    channel.m_FrequencyGain = shakeFrequency;
                    StartCoroutine(BeginShaking());
                }
                else
                {
                    Debug.LogError("No CinemachineBasicMultiChannelPerlin component found on the target camera");
                }
            }
        }

        IEnumerator BeginShaking()
        {
            float startTime = Time.time;
            while(Time.time - startTime < duration)
            {
                yield return null;
            }

            EndShake();
        }

        private void BeginAutoPan()
        {
            if(!path.Looped)
            {
                Debug.LogError("AutoPan state can only be used with looped paths!");
                return;
            }
            if(focusObject == null)
            {
                Debug.LogError("Focus object is not assigned for AutoPan state!");
                return;
            }

            targetCam.LookAt = focusObject;

            StartCoroutine(TravelAutoPanPath());
        }

        IEnumerator TravelAutoPanPath()
        {
            // Calculate the first waypoint's position and rotation
            Vector3 firstWaypointPosition = path.EvaluatePositionAtUnit(0f, CinemachinePathBase.PositionUnits.Distance);
            Quaternion firstWaypointRotation = Quaternion.LookRotation(targetCam.LookAt.position - targetCam.gameObject.transform.position);

            // Smoothly move the camera from its current position to the first waypoint
            yield return StartCoroutine(SmoothMoveToFirstWaypoint(targetCam.transform, firstWaypointPosition, targetCam.LookAt));

            int currentCycle = 0;
            float pathPosition = 0f;
            float pathLength = path.PathLength;
            float cycleDuration = pathLength / cameraSpeed;

            while (currentCycle < numOfCycles)
            {
                float elapsedTime = 0f;

                while (elapsedTime < cycleDuration)
                {
                    elapsedTime += Time.deltaTime;
                    pathPosition = (elapsedTime / cycleDuration) * pathLength;

                    // Get the position and rotation on the path
                    Vector3 worldPosition = path.EvaluatePositionAtUnit(pathPosition, CinemachinePathBase.PositionUnits.Distance);
                    Quaternion worldRotation = Quaternion.LookRotation(path.EvaluateTangentAtUnit(pathPosition, CinemachinePathBase.PositionUnits.Distance));

                    // Move the camera to the position and rotation
                    targetCam.gameObject.transform.position = worldPosition;

                    // Make the camera look at the target
                    Vector3 direction = targetCam.LookAt.position - targetCam.gameObject.transform.position;
                    if (direction != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        targetCam.gameObject.transform.rotation = Quaternion.Slerp(targetCam.gameObject.transform.rotation, targetRotation, Time.deltaTime * cameraSpeed);
                    }

                    yield return null;
                }

                currentCycle++;
            }
            parentStateMachine.EndState();
        }

        public void EndState(bool executeExitEvent)
        {
            switch(type)
            {
                case CinemachineStateType.DollyPath:
                    break;
                case CinemachineStateType.ClashZoom:
                    EndClashZoomState();
                    break;
                case CinemachineStateType.AutoPan:
                    break;
            }
            stateActive = false;
            if(executeExitEvent)
            {
                parentStateMachine.SetActiveState(null);
                stateExitEvent?.Invoke();
            }
        }

        private void EndClashZoomState()
        {
            targetCam.m_Lens.FieldOfView = initialFOV;
        }

        private void EndShake()
        {
            if (targetCam != null)
            {
                var channel = targetCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                if (channel != null)
                {
                    channel.m_AmplitudeGain = 0.0f;
                    channel.m_FrequencyGain = 0.0f;
                    channel.m_NoiseProfile = null;
                }
                else
                {
                    Debug.LogError("No CinemachineBasicMultiChannelPerlin component found on the target camera");
                }
            }
            stateActive = false;
        }
    }
}

