using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

// Created by Oliver Lebert 12-01-21
namespace GP2_Team7.Objects.Player
{
    using InteractOn = Interactable.InteractOn;
    using Managers;

    public class InteractableRaycaster : MonoBehaviour, IControllable
    {
        public Image crosshair;
        public Color crosshairLookingAtInteractableColor = Color.cyan;
        public Image targetCrosshair;
        
        public float range = 2f;

        [Tooltip(
            "Option to raycast all colliders in the middle of the screen regardless if they are obstructed by other colliders or not (Will still only interact with the closest (currently interactable) interactable object) (this may be performance intensive and may produce unintentional behaviour)")]
        public bool raycastAll = false;

        [Header("Debug")] [Tooltip("Draws the raycast ray in the editor using Gizmos")]
        public bool drawRaycastRay = false;

        [Tooltip("The color of the raycast gizmo (if enabled)")]
        public Color raycastGizmoColor = Color.red;

        [Tooltip("Log if interact button has been pressed and has been sent through to the interactable raycaster")]
        public bool debugInteractionInput = false;

        [Tooltip("Log if the raycast hit an object (runs every fixed update)")]
        public bool debugRaycastHit = false;

        [Tooltip("Log if the raycast hit an interactable object (runs every fixed update)")]
        public bool debugRaycastInteractable = false;

        [SerializeField, Header("Debug")] private Interactable currentInteractableInRange = null;

        private Dictionary<Actions, ActionDelegates> _phaseActions;
        private Transform _transform;
        private Color _oldCrosshairColor;
        private bool _crosshairDisabled = false;

        private void Awake()
        {
            _transform = Camera.main.transform;

            if (crosshair != null)
                _oldCrosshairColor = crosshair.color;
            
            if(targetCrosshair != null)
                targetCrosshair.color = crosshairLookingAtInteractableColor;
        }

        public Renderer[] renderers;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (drawRaycastRay && Application.isPlaying)
            {
                Vector3 pos = _transform.position;
                Gizmos.color = raycastGizmoColor;
                Gizmos.DrawLine(pos, pos + _transform.forward * range);
                Gizmos.color = Color.white;
            }
        }
#endif

        private void OnEnable()
        {
            _phaseActions = new Dictionary<Actions, ActionDelegates>
            {
                {
                    Actions.Interact,
                    new ActionDelegates(() => Interact(Interaction.KeyDown), null, () => Interact(Interaction.KeyUp))
                }
            };
        }

        private void OnDisable()
        {
            foreach (KeyValuePair<Actions, ActionDelegates> dic in _phaseActions)
            {
                dic.Value.Unsubscribe(() => Interact(Interaction.KeyDown));
                dic.Value.Unsubscribe(() => Interact(Interaction.KeyUp));
            }
        }

        private void Update()
        {
            if (CutsceneManager.IsInCutscene)
                return;

            if (raycastAll)
                RaycastAll();
            else
                RaycastSingle();
        }

        private void RaycastSingle()
        {
            if (Physics.Raycast(_transform.position, _transform.forward, out RaycastHit hitInfo, range))
            {
                if (debugRaycastHit)
                    Debug.Log("Raycast Hit");

                Interactable interactable = hitInfo.collider.GetComponent<Interactable>();

                if (interactable != null)
                {
                    if (interactable != currentInteractableInRange)
                    {
                        if (currentInteractableInRange != null && currentInteractableInRange.hint != null)
                            currentInteractableInRange.hint.SetActive(false);

                        if (OutlineObject.Instance != null && currentInteractableInRange != null && currentInteractableInRange.outlineObject != null)
                            OutlineObject.Instance.DisableOutline(currentInteractableInRange.outlineObject.GetComponentsInChildren<Renderer>());
                    }

                    if (interactable.IsCurrentlyInteractable())
                    {
                        currentInteractableInRange = interactable;

                        if (OutlineObject.Instance != null && currentInteractableInRange != null && currentInteractableInRange.outlineObject != null )
                        {
                            OutlineObject.Instance.ApplyOutline(currentInteractableInRange.outlineObject.GetComponentsInChildren<Renderer>());
                            Debug.Log("yoo");
                        }
                        
                        if (crosshair != null)
                            crosshair.color = crosshairLookingAtInteractableColor;
                        
                        if(targetCrosshair != null && !_crosshairDisabled)
                            targetCrosshair.gameObject.SetActive(true);

                        if (debugRaycastInteractable)
                            Debug.Log("Raycasting Hit Interactable");

                        if (currentInteractableInRange.hint != null)
                            currentInteractableInRange.hint.SetActive(true);

                        return;
                    }
                }
            }

            if (debugRaycastHit)
                Debug.Log("Raycast didn't hit");

            if (currentInteractableInRange != null && currentInteractableInRange.hint != null)
                currentInteractableInRange.hint.SetActive(false);

            if (OutlineObject.Instance != null && currentInteractableInRange != null && OutlineObject.Instance.isOutlined && currentInteractableInRange.outlineObject != null)
            {
                OutlineObject.Instance.DisableOutline(currentInteractableInRange.outlineObject.GetComponentsInChildren<Renderer>());
            }

            if (crosshair != null)
                crosshair.color = _oldCrosshairColor;
            
            if(targetCrosshair != null && !_crosshairDisabled)
                targetCrosshair.gameObject.SetActive(false);
            
            currentInteractableInRange = null;
        }

        private void RaycastAll()
        {
            RaycastHit[] hitInfo = Physics.RaycastAll(_transform.position, _transform.forward, range);

            if (hitInfo.Length != 0)
            {
                if (debugRaycastHit)
                    Debug.Log("Raycast Hit");

                List<Interactable> interactables = new List<Interactable>();

                foreach (RaycastHit hit in hitInfo)
                {
                    Interactable inter = hit.collider.GetComponent<Interactable>();
                    if (inter != null)
                    {
                        interactables.Add(inter);
                    }
                }

                if (interactables.Count != 0)
                {
                    if (interactables.Count > 1)
                        interactables.Sort(delegate(Interactable a, Interactable b)
                        {
                            if ((a.transform.position - _transform.position).sqrMagnitude <
                                (b.transform.position - _transform.position).sqrMagnitude) return 1;

                            return -1;
                        });

                    Interactable nextValidInteractable = null;
                    for (int i = 0; i < interactables.Count; i++)
                    {
                        if (interactables[i].IsCurrentlyInteractable())
                        {
                            nextValidInteractable = interactables[i];
                        }
                    }

                    if (nextValidInteractable != currentInteractableInRange)
                    {
                        if (currentInteractableInRange != null && currentInteractableInRange.hint != null)
                            currentInteractableInRange.hint.SetActive(false);

                        if (OutlineObject.Instance != null && currentInteractableInRange != null && currentInteractableInRange.outlineObject != null)
                            OutlineObject.Instance.DisableOutline(currentInteractableInRange.outlineObject.GetComponentsInChildren<Renderer>());
                    }

                    if (nextValidInteractable != null)
                    {
                        currentInteractableInRange = nextValidInteractable;

                        if (currentInteractableInRange.IsCurrentlyInteractable())
                        {
                            if (debugRaycastInteractable)
                                Debug.Log("Raycasting Hit Interactable");

                            if (currentInteractableInRange.hint != null)
                                currentInteractableInRange.hint.SetActive(true);

                            if (OutlineObject.Instance != null && currentInteractableInRange.outlineObject != null)
                            {
                                OutlineObject.Instance.ApplyOutline(currentInteractableInRange.outlineObject.GetComponentsInChildren<Renderer>());
                            }
                            
                            if (crosshair != null)
                                crosshair.color = crosshairLookingAtInteractableColor;
                            
                            if(targetCrosshair != null && !_crosshairDisabled)
                                targetCrosshair.gameObject.SetActive(true);

                            return;
                        }
                    }
                }
            }

            if (debugRaycastHit)
                Debug.Log("Raycast didn't hit a currently interactable interactable");

            if (currentInteractableInRange != null && currentInteractableInRange.hint != null)
                currentInteractableInRange.hint.SetActive(false);

            if (OutlineObject.Instance != null && currentInteractableInRange != null && OutlineObject.Instance.isOutlined && currentInteractableInRange.outlineObject != null)
                OutlineObject.Instance.DisableOutline(currentInteractableInRange.outlineObject.GetComponentsInChildren<Renderer>());
            
            if (crosshair != null)
                crosshair.color = _oldCrosshairColor;
            
            if(targetCrosshair != null && !_crosshairDisabled)
                targetCrosshair.gameObject.SetActive(false);

            currentInteractableInRange = null;
        }

        public void SetCrosshairState(bool enable)
        {
            _crosshairDisabled = !enable;
            
            if(crosshair != null)
                crosshair.gameObject.SetActive(enable);
            
            if (targetCrosshair != null && currentInteractableInRange != null && currentInteractableInRange.IsCurrentlyInteractable())
            {
                targetCrosshair.gameObject.SetActive(enable);
            }
        }

        public void Interact(Interaction currentInteraction)
        {
            if (debugInteractionInput && currentInteractableInRange != null)
                Debug.Log($"Interacted with {currentInteractableInRange}", transform);

            if (currentInteractableInRange == null ||
                currentInteractableInRange.GetType() == typeof(InteractablePressurePlate))
                return;

            Attribute[] attributes = Attribute.GetCustomAttributes(currentInteractableInRange.GetType());

            foreach (Attribute attribute in attributes)
            {
                if (attribute is InteractOn response)
                {
                    foreach (Interaction targetInteractionMode in response.GetModes())
                    {
                        if (targetInteractionMode == currentInteraction)
                            OnInteract();
                    }

                    return;
                }
            }

            OnInteract();

            void OnInteract()
            {
                if (currentInteractableInRange.hint && currentInteractableInRange.hint.GetComponent<Animator>())
                {
                    Animator anim = currentInteractableInRange.hint.GetComponent<Animator>();

                    if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Select"))
                        anim.Play("Select", 0, 0);
                }

                currentInteractableInRange.Interact();
            }
        }

        public ActionDelegates GetActionDelegates(Actions actions)
        {
            return _phaseActions?[actions];
        }
    }
}