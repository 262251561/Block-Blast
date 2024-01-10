using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace TH
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioEffectAttach : MonoBehaviour
    {
        private AudioSource __source;
        void Awake()
        {
            __source = gameObject.GetComponent<AudioSource>();
        }

        void OnEnable()
        {
            AudioPlaySystem.s_instance.effectAttachNodes.Add(this);
        }

        void OnDiable()
        {
            AudioPlaySystem.s_instance.effectAttachNodes.Remove(this);
        }

        void Start()
        {
            __source.volume = AudioPlaySystem.s_instance.effectVolume;
        }

        public void OnEffectVolumeChanged(float value)
        {
            __source.volume = value;
        }
    }
}
