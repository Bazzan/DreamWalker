using System;
using GP2_Team7.Objects.Cameras;
using TMPro;
using UnityEngine;

namespace GP2_Team7.Managers
{
    using Objects.Characters;
    using Objects.Player;

    public class CutsceneManager
    {
        private bool _isInCutscene = false;
        private static CamFixedViewSettings _settings;
        
        public static bool IsInCutscene
        {
            get { return Inst._cutscene != null; }
        }

        public static void PlayCutscene(ICutscene cutscene, CamFixedViewSettings settings, float startPosition = 0)
        {
            if (!StopCutscene())
                return;

            Debug.Log("Played cutscene");
            Inst._cutscene = cutscene;
            _settings = settings;

            if (settings.showBlackBars)
                Camera.main.GetComponent<BlackBarHandler>().Animate(true);

            _onCutsceneActiveState?.Invoke(true, settings);

            cutscene.OnCutsceneEnd = Inst.OnCutsceneEnd;

            cutscene.OnCutsceneStart(startPosition);
        }

        /// <summary>
        /// Stops the cutscene currently playing, if interruptible.
        /// Returns whether the cutscene slot is free or not.
        /// </summary>
        public static bool StopCutscene()
        {
            if (Inst._cutscene != null)
            {
                if (Inst._cutscene.IsInterruptible)
                {
                    Inst._cutscene.StopCutscene();
                    return true;
                }

                return false;
            }
            return true;
        }

        private void OnCutsceneEnd(Action[] actions)
        {
            if (actions != null)
            {
                foreach (Action action in actions)
                {
                    action();
                }
            }

            _cutscene.OnCutsceneEnd -= OnCutsceneEnd;

            _cutscene = null;

            _onCutsceneActiveState?.Invoke(false, _settings);
            
            if(_settings.showBlackBars)
                Camera.main.GetComponent<BlackBarHandler>().Animate(false);
        }

        private static CutsceneManager Inst => GameManager.Instance.CutsceneManagerInst;

        private static Action<bool, CamFixedViewSettings> _onCutsceneActiveState;

        private ICutscene _cutscene;

        public void CutsceneUpdate()
        {
            _cutscene?.OnCutsceneUpdate();
        }

        /// <summary>
        /// Assigns an event to occur when a cutscene activates/deactivates.
        /// </summary>
        public static void AssignOnCutsceneActiveState(Action<bool, CamFixedViewSettings> action)
        {
            _onCutsceneActiveState += action;
        }

        /// <summary>
        /// Unassigns the cutscene activation/deactivation event.
        /// </summary>
        public static void UnassignOnCutsceneActiveState(Action<bool, CamFixedViewSettings> action)
        {
            _onCutsceneActiveState -= action;
        }
    }
}
