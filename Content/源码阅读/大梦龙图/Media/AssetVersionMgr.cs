using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DodGame
{
    public class AssetVersionMgr : BSingleton<AssetVersionMgr>
    {
        AssetVerInfo m_verInfo;

        /// <summary>
        /// 本地已有的资源信息
        /// </summary>
        public Dictionary<string, AssetMd5FileInfo> m_localAssetMd5Dict = new Dictionary<string, AssetMd5FileInfo>();      //所有asset资源列表
        List<string> m_localIndexList = new List<string>();
        private bool m_loadedAllAssetMd5 = false;
        
        /// <summary>
        /// 将要下载更新的文件信息
        /// </summary>
        Dictionary<string, AssetMd5FileInfo> m_downAssetMd5Dict = new Dictionary<string, AssetMd5FileInfo>();        //最新的网络拉取回来的asset资源列表
        List<string> m_downIndexList = new List<string>();       //存储所有md5索引文件的名称信息

        /// <summary>
        /// 最新的md5 list
        /// </summary>
        Dictionary<string, AssetMd5FileInfo> m_latestAssetMd5Dict = new Dictionary<string, AssetMd5FileInfo>(); 
        
        /// <summary>
        /// 本地的root信息
        /// </summary>
        private AssetRootMetaInfo m_localRootMeta = new AssetRootMetaInfo();
        private string m_error;

        private bool m_inited = false;

        /// <summary>
        /// 脚本是否也更新了
        /// </summary>
        private bool m_scriptUpdate = false;

        /// <summary>
        /// 获取当前的错误信息
        /// </summary>
        public string error
        {
            get { return m_error; }
            set { m_error = value; }
        }

        /// <summary>
        /// 判断是否有错误信息
        /// </summary>
        public bool haveErr { get { return !string.IsNullOrEmpty(m_error); } }

        /// <summary>
        /// 获取当前客户端的版本
        /// </summary>
        public string assetVersion { get { return BaseConfigInfo.AppVer + "." + m_localRootMeta.m_version; } }

        /// <summary>
        /// 是否是lua版本
        /// </summary>
        public bool isAssetLua
        {
            get { return m_localRootMeta.m_isLua; }
        }

        /// <summary>
        /// 判断脚本有没有更新
        /// </summary>
        public bool scriptUpdated
        {
            get { return m_scriptUpdate; }
        }

        /// <summary>
        /// 临时下载目录
        /// </summary>
        public string downTempDir
        {
            get { return ReleaseUtil.GetDownTempDir(); }
        }

        public List<AssetMd5FileInfo> ParseAssetMd5List(string url, string content)
        {
            return AssetBundleUtil.ParseAssetMd5List(url, content, ref m_error);
        }

        bool IsNeedUpdate(AssetMd5FileInfo newInfo)
        {
            //新增加的文件
            if (!m_localAssetMd5Dict.ContainsKey(newInfo.m_md5FilePath))
            {
                return true;
            }

            //已经存在的,比较md5文件
            AssetMd5FileInfo existFileInfo = m_localAssetMd5Dict[newInfo.m_md5FilePath];
            if (existFileInfo.m_md5Content != newInfo.m_md5Content ||
                existFileInfo.m_compress != newInfo.m_compress)
            {
                return true;
            }

            return false;
        }

        private byte[] LoadStreamAsset(string path)
        {
            var fullPath = Application.streamingAssetsPath + "/" + path;
            byte[] data = null;
            if (fullPath.Contains("://"))
            {
                if (!DPlatform.IsEditorPlatform() && DPlatform.IsAndroidPlatform())
                {
                    data = DodLibUtil.GetDodLib().LoadStreamAssetFile(fullPath);
                }
            }
            else
            {
                data = AssetBundleUtil.ReadFile(fullPath);
            }

            return data;
        }

        public bool Init()
        {
            if (m_inited)
            {
                return true;
            }

            //清除错误
            m_error = null;

            string configFileName = "Config.bytes";
            string platformName = DodLibUtil.GetDodLib().GetPlatformName();
            if (!string.IsNullOrEmpty(platformName))
            {
                configFileName = string.Format("Config_{0}.bytes", platformName);
            }

            var configData = LoadStreamAsset(configFileName);
            if (configData == null)
            {
                BLogger.Error("read config.bytes failed");
                return false;
            }

            var jsonText = System.Text.Encoding.UTF8.GetString(configData);

            if (!BaseConfigInfo.Init(platformName, jsonText, ref m_error))
            {
                return false;
            }

            if (!ParseLocalConfig(jsonText))
            {
                BLogger.Error("ParseLocalConfig failed");
                return false;
            }

            m_inited = true;
            return true;
        }

        public IEnumerator DownloadLatestIndex(string urlPath)
        {
            //清理数据
            m_downIndexList.Clear();
            m_downAssetMd5Dict.Clear();
            m_latestAssetMd5Dict.Clear();
            //清理文件
            ReleaseUtil.DeleteFolder(downTempDir);

            //md5索引文件的总表            
            string urlMd5Root = urlPath + "/" + AssetBundleUtil.MD5HASH_FILE_ROOT_NAME;
            BLogger.Info("DownloadLatestIndex:{0}", urlMd5Root);

            WWW wwwRoot = new WWW(urlMd5Root);
            yield return wwwRoot;

            if (wwwRoot.error != null)
            {
                BLogger.Warning("www request failed, Url:{0}, error:{1}", urlMd5Root, wwwRoot.error);
                m_error = String.Format(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_UPDATE_READ_LOCAL_FAILED, 1, wwwRoot.error));
                wwwRoot.Dispose();
                yield break;
            }

            List<AssetMd5FileInfo> listMd5Index = ParseAssetMd5List(urlMd5Root, wwwRoot.text);
            if (null == listMd5Index)
            {
                BLogger.Warning("ParseAssetMd5List, Url:{0}", urlMd5Root);
                m_error = StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_UPDATE_READ_LOCAL_FAILED_NO_ERRORCODE, 2);
                wwwRoot.Dispose();
                yield break;
            }

            //临时目录写入root文件
            bool ret = AssetBundleUtil.WriteToFile(downTempDir, AssetBundleUtil.MD5HASH_FILE_ROOT_NAME,
                        wwwRoot.bytes);
            wwwRoot.Dispose();

            if (!ret)
            {
                BLogger.Warning("Write root file failed");
                m_error = StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_UPDATE_WRITE_FAILED, (downTempDir + AssetBundleUtil.MD5HASH_FILE_ROOT_NAME));
                yield break;
            }

            ///记录最新的assetbundle所有信息
            Dictionary<string, AssetMd5FileInfo> latestAssetInfo = m_latestAssetMd5Dict;

            Crc32 crc = new Crc32();
            foreach (AssetMd5FileInfo indexFileInfo in listMd5Index)
            {
                BLogger.Info("new index file[{0}]", indexFileInfo.m_md5FilePath);
                
                //循环读取配置
                //如果是内置资源的方式，则需要把所有的index文件给读取下来
                if (IsNeedUpdate(indexFileInfo))
                {
                    m_downIndexList.Add(indexFileInfo.m_md5FilePath);

                    string urlItem = urlPath + "/" + indexFileInfo.m_md5FilePath;
                    WWW www = new WWW(urlItem);
                    yield return www;

                    if (www.error != null)
                    {
                        BLogger.Warning("update [{0}] failed[{1}]", urlItem, www.error);

                        m_error = String.Format("读取资源配置文件失败(3)[{0}]", www.error);
                        yield break;
                    }
                    
                    ret = AssetBundleUtil.WriteToFile(downTempDir, indexFileInfo.m_md5FilePath,
                        www.bytes);
                    if (!ret)
                    {
                        m_error = StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_UPDATE_WRITE_FAILED,
                            (downTempDir + indexFileInfo.m_md5FilePath)); 
                        yield break;
                    }
                    
                    //检查crc校验
                    if (indexFileInfo.m_crc != 0 && !CheckFileCrc(crc, downTempDir + indexFileInfo.m_md5FilePath, indexFileInfo.m_crc))
                    {
                        m_error = StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_UPDATE_CRC_CHECK_FAILED,
                            (downTempDir + indexFileInfo.m_md5FilePath));
                        yield break;
                    }

                    BLogger.Info("check index crc ok, index file: " + indexFileInfo.m_md5FilePath + ",expcect: " + indexFileInfo.m_crc);

                    indexFileInfo.m_downLoad = true;
                    List<AssetMd5FileInfo> listAsset = ParseAssetMd5List(urlItem, www.text);
                    if (null == listAsset)
                    {
                        yield break;
                    }
                    
                    www.Dispose();

                    //增加到待更新列表中
                    for (int i = 0; i < listAsset.Count; i++)
                    {
                        AssetMd5FileInfo eachAsset = listAsset[i];
                        m_latestAssetMd5Dict.Add(eachAsset.m_md5FilePath, eachAsset);
                    }
                }
            }
        }

        public IEnumerator BeginUpdateIndex(string urlPath)
        {
            if (haveErr)
            {
                yield break;
            }

            var enumerator = m_latestAssetMd5Dict.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var eachAsset = enumerator.Current.Value;
                if (IsNeedUpdate(eachAsset))
                {
                    if (m_downAssetMd5Dict.ContainsKey(eachAsset.m_md5FilePath))
                    {
                        BLogger.Warning("file[{0}] has exist in new asset", eachAsset.m_md5FilePath);
                    }
                    else
                    {
                        m_downAssetMd5Dict.Add(eachAsset.m_md5FilePath, eachAsset);
                    }
                }
            }
        }

        public IEnumerator CheckRepairIndex()
        {
            if (haveErr)
            {
                yield break;
            }

            Crc32 crc = new Crc32();
            string abDir = AssetBundleUtil.GetExternAssetBundleDir();
            var enumerator = m_latestAssetMd5Dict.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var md5FileInfo = enumerator.Current.Value;
                var fileName = md5FileInfo.m_md5FilePath;
                string filePath = abDir + fileName;
                if (!AssetBundleUtil.IsFileExist(filePath))
                {
                    string path = Path.GetFileNameWithoutExtension(fileName);
                    if (!AssetBundleLoader.Instance.IsCanFastCreateAb(path))
                    {
                        AssetMd5FileInfo info;
                        if (!m_localAssetMd5Dict.TryGetValue(fileName, out info) || info.m_crc != md5FileInfo.m_crc)
                        {
                            TryAddDownFile(md5FileInfo);
                        }
                    }
                    else
                    {
                        TryAddDownFile(md5FileInfo);
                    }
                }
                else if (!CheckFileCrc(crc, filePath, md5FileInfo.m_crc))
                {
                    TryAddDownFile(md5FileInfo);
                }
            }
        }

        private void TryAddDownFile(AssetMd5FileInfo info)
        {
            if (m_downAssetMd5Dict.ContainsKey(info.m_md5FilePath))
            {
                BLogger.Warning("file[{0}] has exist in new asset", info.m_md5FilePath);
            }
            else
            {
                m_downAssetMd5Dict.Add(info.m_md5FilePath, info);
            }
        }

        public int GetUpdateFileCount()
        {
            return m_downAssetMd5Dict.Count;
        }
        
        public int GetToUpdateSize()
        {
            int iDownSize = 0;
            foreach (KeyValuePair<string, AssetMd5FileInfo> kv in m_downAssetMd5Dict)
            {
                iDownSize += kv.Value.m_contentSize;
            }

            return iDownSize;
        }

        public int GetUpdateSize()
        {
            int iSize = 0;
            foreach (KeyValuePair<string, AssetMd5FileInfo> kv in m_downAssetMd5Dict)
            {
                if (m_localAssetMd5Dict.ContainsKey(kv.Key) &&
                    m_localAssetMd5Dict[kv.Key].m_md5Content == kv.Value.m_md5Content)
                {
                    continue;
                }

                iSize += kv.Value.m_contentSize;
            }

            return iSize;
        }

        private bool CheckFileCrc(Crc32 crc, string path, uint expectCrc)
        {
            //判断crc是否一致，如果一致，则是已经下载了一半的文件
            byte[] downData = AssetBundleUtil.ReadFile(path);
            if (downData != null)
            {
                uint crcVal = crc.ComputeChecksum(downData);
                return crcVal == expectCrc;
            }

            return false;
        }

        protected bool ExtractAndWriteDownFile(string compressDir, string destDir, string fileName, byte[] data)
        {
            //write to file
            bool bRet = AssetBundleUtil.WriteToFile(compressDir, fileName, data);
            if (!bRet)
            {
                m_error = String.Format("WriteToFile: {0} failed", compressDir + fileName);
                return false;
            }

            //解压缩文件
            string compressFilePath = compressDir + fileName;
            int ret = DodLibUtil.GetDodLib().Decompress7zip(compressFilePath, destDir);
            if (ret != 1)
            {
                BLogger.Error("doDecompress7zip failed: {0}, Ret:{1}", compressFilePath, ret);
                m_error = StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_UPDATE_EXTRACT_DOWNFILE_FAILED, fileName, ret);
                return false;
            }

            //删除压缩文件
            AssetBundleUtil.DeleteFile(compressFilePath);
            return true;
        }

        protected string GetDownloadCompressDir()
        {
            return downTempDir + "../cdowndir/";
        }

        public IEnumerator BeginUpdateAsset(string urlPath, StartupUI ui)
        {
            yield return null;

            float downLoadFileCnt = 0;
            float downLoadSize = 0;
            float totalSize = GetToUpdateSize();
            if (totalSize < 1)
            {
                totalSize = 1;
            }

            List<string> listDownloadFile = new List<string>();
            string downCompressDir = GetDownloadCompressDir();//downTempDir + "../cdowndir/";

            Crc32 crc = new Crc32();

            ui.ShowProgressBar(true);
            ui.ShowDownInfo(downLoadSize, totalSize, true, 0, true);

            foreach(KeyValuePair<string, AssetMd5FileInfo> kv in m_downAssetMd5Dict)
            {
               // showInfo("正在下载资源包: " + string.Format("({0})", AssetUpdater.Instance.NewVersion), null);
                ui.ShowDownInfo(downLoadSize, totalSize, true, 0);
                string destFilePath = downTempDir + kv.Key;

                ///检查文件是否已经下载到本地了
                if (kv.Value.m_crc > 0 && AssetBundleUtil.IsFileExist(destFilePath))
                {
                    if (CheckFileCrc(crc, destFilePath, kv.Value.m_crc))
                    {
                        listDownloadFile.Add(kv.Key);
                        kv.Value.m_downLoad = true;
                        downLoadFileCnt++;
                        downLoadSize += kv.Value.m_contentSize;

                        ui.SetProgress((uint) downLoadSize, (uint) totalSize);
                        ui.ShowDownInfo(downLoadSize, totalSize, true, 0);
                        continue;
                    }
                }

                ui.BeginDownload();
                //begin download
                string url = urlPath + "/" + kv.Key;
                WWW www = new WWW(url);
                yield return www;

                //有错误，返回失败
                if (www.error != null)
                {
                    m_error = string.Format(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_UPDATE_ASSET_DOWNLOAD_FAILED, www.error, kv.Key));
                    BLogger.Warning("download file:{0} failed:{1}", kv.Key, www.error);
                    yield break;
                }

                if (!ExtractAndWriteDownFile(downCompressDir, downTempDir, kv.Key, www.bytes))
                {
                    string text = StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_DECOMPRESS_FAILED);
                    if (!string.IsNullOrEmpty(text))
                    {
                        m_error = string.Format(text, kv.Key);

                    }
                    else
                    {
                        m_error = string.Format("解压文件{0}失败，请点击重试!", kv.Key);
                    }
                    yield break;
                }

                //比较下下载的文件crc,避免一些cdn不同步导致的下载不全的问题
                if (kv.Value.m_crc > 0 && !CheckFileCrc(crc, downTempDir + kv.Key, kv.Value.m_crc))
                {
                    BLogger.Error("check file crc error: {0}", kv.Key);

                    string text = StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_CHECKCRC_FAILED);
                    if (!string.IsNullOrEmpty(text))
                    {
                        m_error = string.Format(text, kv.Key);

                    }
                    else
                    {
                        m_error = string.Format("文件({0})校验错误，请点击重试!", kv.Key);
                    }
                    yield break;
                }
                
                listDownloadFile.Add(kv.Key);
                kv.Value.m_downLoad = true;
                
                downLoadSize += www.bytes.Length;
                ui.SetProgress((uint) downLoadSize, (uint) totalSize);
                downLoadFileCnt++;
                ui.ShowDownInfo(downLoadSize, totalSize, true, www.bytes.Length);
                www.Dispose();
            }

            ui.ShowDownInfo(0, 0, false, 0);
            ///如果已经下载完了，那么开始copy到目标目录中
            ///先copyindex 文件，md5最后copy
            foreach (string indexFile in m_downIndexList)
            {
                listDownloadFile.Add(indexFile);
            }
            listDownloadFile.Add(AssetBundleUtil.MD5HASH_FILE_ROOT_NAME);

            //开始copy数据
            //uiSlider.gameObject.SetActive(false);

            //开始copy数据
            string fromDir = downTempDir;
            string destDir = AssetBundleUtil.GetExternAssetBundleDir();
            int copyDownFile = 0;
            foreach (string downFile in listDownloadFile)
            {
                byte[] content = AssetBundleUtil.ReadFile(fromDir + downFile);
                bool ret = AssetBundleUtil.WriteToFile(destDir, downFile, content);
                if (!ret)
                {
                    m_error = "copy file failed: " + destDir + downFile;
                    yield break;
                }

                int percent = copyDownFile*100/listDownloadFile.Count;
                copyDownFile++;

                ui.ShowText(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_UPDATE_ASSET_INSTALL_PROGRESS, percent));
                ui.SetProgress((uint)copyDownFile * 100, (uint)listDownloadFile.Count);

                //判断资源是否是脚本文件
                if (downFile.IndexOf(".dll.bytes") >= 0)
                {
                    BLogger.Info("download script file: {0}", downFile);
                    m_scriptUpdate = true;
                }

                yield return null;
            }

            ui.ShowProgressBar(false);
            ui.ShowText(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_UPDATE_ASSET_SUCCESS));
        }

        private bool LoadRootBaseInfo(string content)
        {
            m_localRootMeta = AssetBundleUtil.ParseRootMeta(content);
            if (string.IsNullOrEmpty(m_localRootMeta.m_version))
            {
                m_error = "read local version failed";
                return false;
            }

            BaseConfigInfo.AbVersion = m_localRootMeta.m_version;
            BaseConfigInfo.IsAssetLua = m_localRootMeta.m_isLua;

            return true;
        }

        private bool ParseLocalConfig(string configText)
        {
            Dictionary<string, object> configJson = MiniJSON.Json.Deserialize(configText) as Dictionary<string, object>;
            if (configJson == null)
            {
                m_error = "load resource config failed";
                return false;
            }

            return true;
        }

        public bool LoadLocalAllAssetMd5()
        {
            if (m_loadedAllAssetMd5)
            {
                BLogger.Warning("local asset md5 has loaded");
                return true;
            }

            m_loadedAllAssetMd5 = true;
            string path = AssetBundleUtil.GetExternAssetBundleDir() + AssetBundleUtil.MD5HASH_FILE_ROOT_NAME;

            //读取本地所有的文件配置信息
            foreach (string indexFile in m_localIndexList)
            {
                path = AssetBundleUtil.GetExternAssetBundleDir() + indexFile;
                string content = AssetBundleUtil.ReadTextFile(path);
                if (content == null)
                {
                    m_error = "Read file failed: " + path;
                    return false;
                }

                var list = ParseAssetMd5List(path, content);
                if (list != null)
                {
                    foreach (AssetMd5FileInfo indexFileInfo in list)
                    {
                        if (m_localAssetMd5Dict.ContainsKey(indexFileInfo.m_md5FilePath))
                        {
                            BLogger.Error("file entry repeated: {0}", indexFileInfo.m_md5FilePath);
                            return false;
                        }

                        m_localAssetMd5Dict.Add(indexFileInfo.m_md5FilePath, indexFileInfo);
                    }
                }
            }

            return true;
        }

        public bool ReadBaseInfo()
        {
            m_error = null;
            //m_allMd5List = new Dictionary<string, AssetMd5FileInfo>();
            m_localAssetMd5Dict.Clear();
            m_localIndexList.Clear();
            m_loadedAllAssetMd5 = false;

            string path = AssetBundleUtil.GetExternAssetBundleDir() + AssetBundleUtil.MD5HASH_FILE_ROOT_NAME;
            string content = AssetBundleUtil.ReadTextFile(path);
            if (content == null)
            {
                m_error = "Read file failed: " + path;
                return false;
            }

            if (!LoadRootBaseInfo(content))
            {
                return false;
            }

            List<AssetMd5FileInfo> list = ParseAssetMd5List(path, content);
            if (list != null)
            {
                m_localIndexList.Capacity = list.Count;
                foreach(AssetMd5FileInfo indexFileInfo in list)
                {
                    m_localIndexList.Add(indexFileInfo.m_md5FilePath);
                    
                    if (m_localAssetMd5Dict.ContainsKey(indexFileInfo.m_md5FilePath))
                    {
                        BLogger.Error("file entry repeated: {0}", indexFileInfo.m_md5FilePath);
                        return false;
                    }

                    m_localAssetMd5Dict.Add(indexFileInfo.m_md5FilePath, indexFileInfo);
                }
            }
            
            return true;
        }

        //保留更新记录
        public void SaveResult()
        {

        }

        /// <summary>
        /// 比较版本号
        /// </summary>
        /// <param name="leftVersion"></param>
        /// <param name="rightVersion"></param>
        /// <returns></returns>
        public bool CompareAbVersion(string leftVersion, string rightVersion)
        {
            string[] leftSplits = leftVersion.Split('.');
            if (leftSplits.Length != 2)
            {
                return false;
            }
            string[] rightSplits = rightVersion.Split('.');
            if (rightSplits.Length != 2)
            {
                return false;
            }
            try
            {
                if (int.Parse(leftSplits[0]) == int.Parse(rightSplits[0]))
                {
                    if (int.Parse(leftSplits[1]) < int.Parse(rightSplits[1]))
                    {
                        return false;
                    }
                }
                else
                {
                    if (int.Parse(leftSplits[0]) < int.Parse(rightSplits[0]))
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("catch compare exception " + e);
            }
            return true;
        }
    }
}
