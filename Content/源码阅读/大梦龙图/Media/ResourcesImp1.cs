//using BGame;
//using GameBase;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using Object = UnityEngine.Object;

//namespace DodGame
//{
//    public class ResourceBehaviour : BBehaviourSingleton<ResourceBehaviour>
//    {
//    }

//    public class ResourcesImp : IResources
//    {
//        #region 资源管理
//        public void MarkResourceHotUnload(string resPath)
//        {
//            XResource.MarkResourceAbHotUnload(resPath);
//        }

//        public void SetGoPoolEnable(bool enable)
//        {
//            XResource.EnableGoPoolRecycle = enable;
//        }

//        /// <summary>
//        /// 设置是否需要打印内存池满的日志
//        /// </summary>
//        /// <param name="enable"></param>
//        public void SetPoolFullLog(bool enable)
//        {
//            XResource.LogWhenPoolFull = enable;
//        }

//        public bool IsGoPoolEnable()
//        {
//            return XResource.EnableGoPoolRecycle;
//        }

//        public int GetAllAssetBundleCount()
//        {
//            if (GameCoreConfig.UseAssetBundle)
//            {
//                return AssetBundlePool.Instance.GetAllAssetBundleCount();
//            }

//            return 0;
//        }

//        /// <summary>
//        /// 设置允许同时异步载入的资源个数
//        /// </summary>
//        /// <param name="maxLoadNum"></param>
//        public void SetMaxResourceLoadNum(int maxLoadNum)
//        {
//            XResource.SetMaxResourceAsyncLoadNum(maxLoadNum);
//        }

//        /// 获取当前允许同时异步载入的资源个数
//        /// </summary>
//        /// <returns></returns>
//        public int GetMaxResourceLoadNum()
//        {
//            return GameCoreConfig.ResourceMaxAsyncLoadNum;
//        }

//        /// <summary>
//        /// 卸载不使用的内存资源
//        /// </summary>
//        public void UnloadUnusedAssets()
//        {
//            Resources.UnloadUnusedAssets();
//        }

//        /// <summary>
//        /// 卸载不使用的assetbundle
//        /// </summary>
//        public void UnloadUnusedAb(bool unloadNeverExpireAb)
//        {
//            XResource.UnloadUnusedAb(unloadNeverExpireAb);
//        }


//        #endregion

//        public void DestroyObject(UnityEngine.Object go)
//        {
//            if (go is GameObject)
//            {
//                XResource.FreeGameObject(go as GameObject);
//            }
//            else
//            {
//                Object.Destroy(go);
//            }
//        }

//        public void DestroyObject(UnityEngine.Object go, float delayTime)
//        {
//            if (go is GameObject)
//            {
//                XResource.FreeGameObject(go as GameObject, delayTime);
//            }
//            else
//            {
//                Object.Destroy(go, delayTime);
//            }
//        }

//        public Shader FindShader(string shaderName)
//        {
//            return XResource.FindShader(shaderName);
//        }


//        public GameObject AllocGameObject(string resPath, Transform parent, bool initEnable)
//        {
//            return XResource.AllocOrNewInstanceGo(resPath, parent, initEnable);
//        }

//        public GameObject AllocGameObject(string resPath, Transform parent, Vector3 localPos,
//            Quaternion localRot, bool initEnable)
//        {
//            return XResource.AllocOrNewInstanceGo(resPath, parent, true, localPos, localRot, initEnable);
//        }

//        public void AllocGameObjectAsync(string resPath, Action<GameObject> onLoaded, Transform parent, bool initEnable, BAsyncOper oper)
//        {
//            XResource.AllocOrNewInstanceGoAsync(resPath, onLoaded, parent, oper, initEnable);
//        }

//        public void AllocGameObjectAsync(string resPath, Action<GameObject> onLoaded, Transform parent, Vector3 localPos,
//            Quaternion localRot, bool initEnable, BAsyncOper oper)
//        {
//            XResource.AllocOrNewInstanceGoAsync(resPath, onLoaded, parent, true, localPos, localRot, oper, initEnable);
//        }

//        public byte[] LoadStreamAsset(string path)
//        {
//            var fullPath = Application.streamingAssetsPath + "/" + path;
//            byte[] data = null;
//            if (fullPath.Contains("://"))
//            {
//#if UNITY_ANDROID && !UNITY_EDITOR
//                data = DodLib.LoadStreamAssetFile(fullPath);
//#endif
//            }
//            else
//            {
//                data = AssetBundleUtil.ReadFile(fullPath);
//            }

//            return data;
//        }

//        public string LoadStreamTextAsset(string path)
//        {
//            var binData = LoadStreamAsset(path);
//            if (binData != null && binData.Length > 0)
//            {
//                return StringUtility.UTF8BytesToString(binData);
//            }

//            return null;
//        }

//        public UnityEngine.Object LoadResourceAsset(string path, Type type, bool logNotFound)
//        {
//            if (type == null)
//            {
//                BLogger.Fatal("Invalid asset type");
//                return null;
//            }

//            LoadFromType loadFromType;
//            return XResource.LoadAsset(path, type, out loadFromType, logNotFound);
//        }

//        public void LoadResourceAssetAsync(string resPath, Type type, Action<UnityEngine.Object> onLoaded, BAsyncOper oper)
//        {
//            XResource.LoadAsync(resPath, type, onLoaded, oper);
//        }

//        public void PreloadResourceAssetAsync(string path, Action onLoaded, BAsyncOper oper)
//        {
//            LoadResourceAssetAsync(path, typeof(UnityEngine.Object), (loadedObj) =>
//            {
//                if (onLoaded != null)
//                {
//                    onLoaded();
//                }
//            }, oper);
//        }

//        public void LoadScene(string sceneName, bool persist)
//        {
//            UnitySceneLoader.LoadScene(sceneName, persist);
//        }

//        public void LoadSceneAsync(string sceneName, Action<float> sceneProgress, Action onLoaded)
//        {
//            ResourceInst.Instance.StartCoroutine(UnitySceneLoader.LoadSceneAsync(sceneName, sceneProgress, onLoaded));
//        }

//        /// <summary>
//        /// 单位KB
//        /// </summary>
//        /// <returns></returns>
//        public int GetLuaMemUsed()
//        {
//            if (LuaApp.HasInstance)
//            {
//                var env = LuaApp.Instance.LuaEnv;
//                return env.Memroy;
//            }

//            return 0;
//        }

//        public bool IsLuaGcRunning()
//        {
//            if (LuaApp.HasInstance)
//            {
//                var env = LuaApp.Instance.LuaEnv;
//                return env.IsGcRunning;
//            }

//            return false;
//        }

//        public bool IsUseAssetBundle()
//        {
//            return GameCoreConfig.UseAssetBundle;
//        }

//        /// <summary>
//        /// 获取对象池中对象的总数
//        /// </summary>
//        /// <returns></returns>
//        public int GetGoPoolObjectCount()
//        {
//            return XResource.GetGoPoolObjectCount();
//        }

//        public int GetDelayFreeObjectCount()
//        {
//            return XResource.GetDelayDestroyCount();
//        }

//        public int GetFreedDelayCount()
//        {
//            return XResource.GetFreedDelayCount();
//        }

//        public void ClearAllDelayDestroy()
//        {
//            XResource.ClearAllDelayDestroy();
//        }

//        /// <summary>
//        /// 获取缓存资源的个数
//        /// </summary>
//        /// <returns></returns>
//        public int GetCacheResourceCount()
//        {
//            return XResource.GetCacheResourceCount();
//        }

//        /// <summary>
//        /// 清除所有的缓存和对象
//        /// </summary>
//        public void FreeCacheAndPool()
//        {
//            XResource.FreeAllCacheAndGo();
//        }

//        /// <summary>
//        /// 暂停所有的缓存行为，一般是用在载入场景的过程中
//        /// </summary>
//        public void PauseAllCache()
//        {
//            XResource.PauseAllCache();
//        }

//        public void StopAssetBundleAutoExpire()
//        {
//            XResource.StopAssetBundleAutoExpire();
//        }

//        public void StartAssetBundleAutoExpire()
//        {
//            XResource.StartAssetBundleAutoExpire();
//        }

//        /// <summary>
//        /// 恢复所遇的缓存行为
//        /// </summary>
//        public void ResumeAllCache()
//        {
//            XResource.ResumeAllCache();
//        }

//        /// <summary>
//        /// 下载配置的资源，这个资源一般是立刻卸载的
//        /// </summary>
//        /// <param name="resPath"></param>
//        public void UnloadBinResource(string resPath)
//        {
//            XResource.UnloadResource(resPath);
//            XResource.UnloadBinAssetBundle(resPath);
//        }

//        /// <summary>
//        /// 初始化缓存配置
//        /// </summary>
//        public void InitCacheConfig()
//        {
//            ReadResourceCacheConfig();
//        }

//        #region 缓存管理


//        /// <summary>
//        /// 读取资源的缓存策略
//        /// </summary>
//        void ReadResourceCacheConfig()
//        {
//            string cachePath = "Config/CacheConfig/need_cache_list";
//            TextAsset needCacheListAsset = XResource.LoadAsset<TextAsset>(cachePath);
//            TextAsset needPersistListAsset = XResource.LoadAsset<TextAsset>("Config/CacheConfig/need_persist_list");

//            if (needCacheListAsset != null)
//            {
//                string configText = StringUtility.UTF8BytesToString(needCacheListAsset.bytes);
//                if (!LoadCacheAndTimeConfig(configText))
//                {
//                    BLogger.Warning("-------------LoadCacheAndTimeConfig failed: {0}", cachePath);
//                }
//            }
//            else
//            {
//                BLogger.Error("read need cache resource list config failed");
//            }

//            if (needPersistListAsset != null)
//            {
//                List<string> needPersistList = AssetBundleUtil.ReadTextStringList(needPersistListAsset.bytes);
//                if (needPersistList != null)
//                {
//                    XResource.RegPersistResPath(needPersistList);
//                    BLogger.Info("-------------register need persist res list: {0}", needPersistList.Count);
//                }
//            }
//            else
//            {
//                BLogger.Error("read need persist resource list config failed");
//            }

//        }

//        bool LoadCacheAndTimeConfig(string configText)
//        {
//            try
//            {
//                var allConfigList = MiniJSON.Json.Deserialize(configText) as List<object>;
//                if (null == allConfigList)
//                {
//                    BLogger.Error("parse depends json error");
//                    return false;
//                }

//                for (int i = 0; i < allConfigList.Count; i++)
//                {
//                    Dictionary<string, object> dictItem = allConfigList[i] as Dictionary<string, object>;
//                    string assetPath = AssetBundleUtil.ReadJsonKey<string>(dictItem, "asset");
//                    int cacheTime = (int)AssetBundleUtil.ReadJsonKey<Int64>(dictItem, "time");
//                    int poolCnt = (int)AssetBundleUtil.ReadJsonKey<Int64>(dictItem, "poolcnt");
//                    XResource.RegCacheResPath(assetPath, cacheTime, poolCnt);

//                    BLogger.Info("[{0}]cache resource[{1}] cache time: {2} pool max count:{3}", i, assetPath, cacheTime, poolCnt);
//                }

//                return true;
//            }
//            catch (Exception e)
//            {
//                BLogger.Error("LoadCacheAndTimeConfig failed: " + e.ToString());
//                return false;
//            }
//        }
//        #endregion
//    }
//}
