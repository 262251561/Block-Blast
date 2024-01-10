using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace TH
{
    public class AudioPlaySystem : MonoBehaviour
    {
        private static float s_CheckUnloadTime = 10.0f;

        public static AudioPlaySystem s_instance = null;

        public struct AudioLoadNode
        {
            public int instanceId;
            public bool isBGM;
            public bool isLoop;
            public int resPath;
            public bool isLoadFromResource;
            public Transform followNode;
            public Vector3 position;
        }

        public struct AudioUnloadNode
        {
            public int instanceId;
        }

        private struct AudioRes
        {
            public AudioClip resClip;
            public int refCount;

            public bool isDone
            {
                get
                {
                    return resClip != null ? true : false;
                }
            }

            public AudioClip clip
            {
                get
                {
                    return resClip;
                }
            }
        }

        private struct AudioNode
        {
            public int instanceId;
            public bool isBGM;
            public bool isLoop;
            public Transform followNode;
            public AudioSource audioSource;
            public float keepTime;
        }

        public List<AudioLoadNode> audioLoadQueue;
        public List<AudioUnloadNode> audioUnloadQueue;

        public float bgmVolume
        {
            private set;
            get;
        }

        public float effectVolume
        {
            private set;
            get;
        }

        public float fadeDurationTime = 1.0f;

        private AudioSource __currentAudioPrefab;
        private Stack<AudioSource> __audioSourcePool;

        private float __currentCheckTime;
        private List<AudioNode> __currentAudioQueue;
        private Dictionary<int, AudioRes> __currentAudioResMap;

        private AudioNode __currentBGMNode;
        private List<AudioLoadNode> __currentBGMAudioQueue;

        public List<AudioEffectAttach> effectAttachNodes
        {
            private set;
            get;
        }

        private void Awake()
        {
            GameObject.DontDestroyOnLoad(gameObject);
            __currentBGMAudioQueue = new List<AudioLoadNode>();
            effectAttachNodes = new List<AudioEffectAttach>();

            bgmVolume = effectVolume = 1.0f;

            StartCoroutine(__PlayBGMAudio());
        }

        public void PlayBGM(int resUrl, int instanceId, bool isLoadFromResource)
        {
            var loadNode = new AudioPlaySystem.AudioLoadNode();
            loadNode.isBGM = true;
            loadNode.isLoop = true;
            loadNode.isLoadFromResource = isLoadFromResource;
            loadNode.instanceId = instanceId;
            loadNode.resPath = resUrl;

            audioLoadQueue.Add(loadNode);
        }

        void Start()
        {
            audioLoadQueue = new List<AudioLoadNode>();
            audioUnloadQueue = new List<AudioUnloadNode>();

            __currentCheckTime = 0.0f;
            __currentAudioQueue = new List<AudioNode>();
            __currentAudioResMap = new Dictionary<int, AudioRes>();

            __audioSourcePool = new Stack<AudioSource>();

            GameObject audioPrefab = new GameObject("AudioPlayNode");
            GameObject.DontDestroyOnLoad(audioPrefab);

            __currentAudioPrefab = audioPrefab.AddComponent<AudioSource>();

            for (int i = 0; i < 10; ++i)
            {
                var node = AudioSource.Instantiate(__currentAudioPrefab);
                GameObject.DontDestroyOnLoad(node);

                node.gameObject.SetActive(false);
                __audioSourcePool.Push(node);
            }

            __isApplicationQuit = false;
            s_instance = this;
        }

        private bool __isApplicationQuit;
        private void OnApplicationQuit()
        {
            __isApplicationQuit = true;
        }

        void OnDestroy()
        {
            if (__isApplicationQuit)
                return;

            s_instance = null;

            __currentAudioResMap.Clear();

            __currentAudioPrefab = null;
            __audioSourcePool.Clear();
            __currentAudioQueue.Clear();
        }

        public void ApplyBGMVolume(float value)
        {
            bgmVolume = value;
            if (__currentBGMNode.audioSource != null)
                __currentBGMNode.audioSource.volume = value;
        }

        public void ApplyEffectVolume(float value)
        {
            effectVolume = value;
            int i, length = __currentAudioQueue.Count;
            for (i = 0; i < length; ++i)
            {
                var audioNode = __currentAudioQueue[i];
                audioNode.audioSource.volume = value;
            }

            length = effectAttachNodes.Count;
            for (i = 0; i < length; ++i)
            {
                var item = effectAttachNodes[i];
                if(item != null)
                    item.OnEffectVolumeChanged(value);
                else
                {
                    effectAttachNodes.RemoveAtSwapBack(i);
                    --i;
                    --length;
                }
            }
        }

        AudioSource AllocateAudioSource(AudioClip clip, float volume)
        {
            AudioSource node = null;
            if(__audioSourcePool.Count > 0)
            {
                node = __audioSourcePool.Pop();
            }
            else
            {
                node = AudioSource.Instantiate(__currentAudioPrefab);
                GameObject.DontDestroyOnLoad(node);
            }

            node.gameObject.SetActive(true);
            node.volume = volume;
            node.clip = clip;
            node.Play();
            return node;
        }

        void FreeAdudioSource(AudioSource audioSource)
        {
            audioSource.gameObject.SetActive(false);
            __audioSourcePool.Push(audioSource);
        }

        IEnumerator __FadeMusic(AudioSource source, float volume, float durtion)
        {
            float currentDurition = 0.0f;
            float durtionInvert = 1.0f / durtion;
            float startVolume = source.volume;
            bool isBreak = false;
            while (currentDurition <= durtion)
            {
                source.volume = Mathf.Lerp(startVolume, volume, currentDurition * durtionInvert);
                if (isBreak)
                    break;
                currentDurition += Time.deltaTime;
                if (currentDurition >= durtion)
                {
                    currentDurition = durtion;
                    isBreak = true;
                }

                yield return null;
            }
        }

        IEnumerator __PlayBGMAudio()
        {
            Transform mainCamera = null;

            while (true)
            {
                if (__currentBGMAudioQueue.Count > 0)
                {
                    if(mainCamera == null)
                        mainCamera = Camera.main.gameObject.transform;

                    //fade out
                    if (__currentBGMNode.audioSource != null)
                    {
                        yield return __FadeMusic(__currentBGMNode.audioSource, 0.0f, fadeDurationTime);
                        __currentBGMNode.audioSource.Stop();
                    }

                    var currentBGMNode = __currentBGMAudioQueue[0];
                    __currentBGMAudioQueue.RemoveAt(0);

                    var clip = __currentAudioResMap[currentBGMNode.resPath].clip;

                    __currentBGMNode = new AudioNode();
                    __currentBGMNode.audioSource = mainCamera.GetComponent<AudioSource>();
                    __currentBGMNode.audioSource.volume = 0.0f;
                    __currentBGMNode.audioSource.loop = true;
                    __currentBGMNode.audioSource.clip = clip;
                    __currentBGMNode.audioSource.transform.position = mainCamera.position;
                    __currentBGMNode.audioSource.Play();

                    //fade in
                    yield return __FadeMusic(__currentBGMNode.audioSource, bgmVolume, fadeDurationTime);
                    ApplyBGMVolume(bgmVolume);
                }
                else
                    yield return UnityExtension.WaitForSeconds(0.2f);
            }
        }

        void LateUpdate()
        {
            if (Camera.main == null)
                return;

            AudioRes audioRes;
            AudioNode currentNode;
            AudioLoadNode loadNode;
            var stringMapInstance = StringMap.instance;
            int i, length = audioLoadQueue.Count;
            for(i=0; i<length; ++i)
            {
                loadNode = audioLoadQueue[i];
                if(!__currentAudioResMap.TryGetValue(loadNode.resPath, out audioRes))
                {
                    var loadRes = stringMapInstance.GetValue(loadNode.resPath);
                    if (loadNode.isLoadFromResource)
                        audioRes.resClip = Resources.Load<AudioClip>(loadRes);
                    audioRes.refCount = 0;
                    __currentAudioResMap[loadNode.resPath] = audioRes;
                    continue;
                }
                else
                {
                    if (audioRes.isDone)
                    {
                        ++audioRes.refCount;
                        __currentAudioResMap[loadNode.resPath] = audioRes;
                    }
                    else
                        continue;
                }

                var clip = audioRes.clip;

                if (!loadNode.isBGM)
                {
                    currentNode = new AudioNode();
                    currentNode.isBGM = false;
                    currentNode.audioSource =AllocateAudioSource(clip, effectVolume);

                    currentNode.instanceId = loadNode.instanceId;
                    currentNode.isLoop = loadNode.isLoop;
                    currentNode.keepTime = !loadNode.isLoop ? clip.length : -1.0f;
                    currentNode.followNode = loadNode.followNode;
                    currentNode.audioSource.loop = loadNode.isLoop;
                    if (currentNode.followNode == null)
                        currentNode.audioSource.transform.position = loadNode.position;

                    __currentAudioQueue.Add(currentNode);
                }
                else
                {
                    __currentBGMAudioQueue.Add(loadNode);
                }
                
                audioLoadQueue.RemoveAtSwapBack(i);
                --i;
                length = audioLoadQueue.Count;
            }

            length = audioUnloadQueue.Count;
            if(length > 0)
            {
                AudioUnloadNode unloadNode;
                for (i = 0; i < length; ++i)
                {
                    unloadNode = audioUnloadQueue[i];

                    var index = __currentAudioQueue.FindIndex((item) => { return item.instanceId == unloadNode.instanceId; });
                    if (index != -1)
                    {
                        currentNode = __currentAudioQueue[index];
                        if(!currentNode.isBGM)
                            FreeAdudioSource(currentNode.audioSource);

                        __currentAudioQueue.RemoveAtSwapBack(index);
                    }
                }

                audioUnloadQueue.Clear();
            }

            length = __currentAudioQueue.Count;
            for(i=0; i<length; ++i)
            {
                currentNode = __currentAudioQueue[i];
                if(!currentNode.isBGM)
                {
                    if(currentNode.followNode != null)
                        currentNode.audioSource.transform.position = currentNode.followNode.position;

                    if(!currentNode.isLoop)
                    {
                        currentNode.keepTime -= Time.deltaTime;
                        if (currentNode.keepTime <= 0.0f)
                        {
                            FreeAdudioSource(currentNode.audioSource);
                            __currentAudioQueue.RemoveAtSwapBack(i);
                            --i;
                            length = __currentAudioQueue.Count;
                        }
                        else
                            __currentAudioQueue[i] = currentNode;
                    }
                }
            }

            __currentCheckTime += Time.deltaTime;

            if (__currentCheckTime >= s_CheckUnloadTime)
            {
                if(__currentAudioResMap.Count > 0)
                {
                    NativeList<int> deleteArray = new NativeList<int>(Allocator.Temp);

                    foreach (var item in __currentAudioResMap)
                    {
                        if (item.Value.refCount == 0)
                        {
                            deleteArray.Add(item.Key);
                        }
                    }

                    length = deleteArray.Length;
                    for (i = 0; i < length; ++i)
                    {
                        audioRes = __currentAudioResMap[deleteArray[i]];

                        __currentAudioResMap.Remove(deleteArray[i]);
                    }

                    deleteArray.Dispose();
                }

                __currentCheckTime = 0.0f;
            }
        }
    }
}
