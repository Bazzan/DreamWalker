using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

namespace GP2_Team7.Objects
{
    public class SoundEmitter : MonoBehaviour
    {
        private Dictionary<string, StudioEventEmitter> _emitters = new Dictionary<string, StudioEventEmitter>();

        void Awake()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                StudioEventEmitter see = transform.GetChild(i).GetComponent<StudioEventEmitter>();

                _emitters.Add(transform.GetChild(i).name, see);
            }
        }

        public void Play(string name)
        {
            if (_emitters.ContainsKey(name))
            {
                _emitters[name].enabled = true;
                _emitters[name].Play();
            }
                
        }

        public void Stop(string name)
        {
            if (_emitters.ContainsKey(name))
            {
                _emitters[name].enabled = false;
                _emitters[name].Stop();
            }
        }

        public void TriggerCue(string name)
        {
            if (_emitters.ContainsKey(name))
            {
                _emitters[name].EventInstance.triggerCue();
            }
        }
    }
}
