using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using DodGame;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class XGameStartup : MonoBehaviour, StartupUI
{
    //开始scene
    public string m_startScene = "SceneMain";

    // 信息显示
    public Text m_title;

    //进度条
    public Slider m_process;

    //下载确认窗口
    public ReleaseConfirmBehaviour m_confirmUI;

    //下载确认窗口Ios
    public ReleaseConfirmBehaviourIos m_confirmUIIos;

    //当前版本
    public Text m_labelVersion;

    //刷新等待
    private Coroutine m_tickRefreshCort = null;
    
    private float m_totaldownSize;

    private float m_curspeeddownSize;

    private float m_curdownSize;

    private float m_downBeginTime;

    private BTimerTick m_downTick;

    //获取AB路径
    private string GetAbDir()
    {
        return ReleaseUtil.GetDataDir() + "/StreamingAssets/ab/";
    }

    private void InitFileProtocol()
    {
#if UNITY_EDITOR
        AssetBundleUtil.SetFileProtocol("file:///");
#else
        AssetBundleUtil.SetFileProtocol("file://");
#endif
    }

    void InitAbStartup()
    {
        AssetBundleConfig.InitConfig(true);
        AssetBundleUtil.SetExternAssetBundleDir(GetAbDir());

#if UNITY_IPHONE
        XFileUtil.SetFolderNoSaveImp((folderPath) =>
        {
            UnityEngine.iOS.Device.SetNoBackupFlag(folderPath);
        });
#endif

        AssetBundleUtil.m_abHashMd5 = false;
        GameCoreConfig.UseAssetBundle = true;

#if UNITY_EDITOR
        AssetBundleUtil.traceAssetBundleDebug = false;
#endif
        
        InitFileProtocol();

        BLogger.Info("******dataPath: {0}", Application.dataPath);
        BLogger.Info("******streamingAssetsPath:{0}", Application.streamingAssetsPath);
        BLogger.Info("******temporaryCachePath:{0}", Application.temporaryCachePath);
        BLogger.Info("******persistentDataPath:{0}", Application.persistentDataPath);
        BLogger.Info("******asset bundle dir: {0}", GetAbDir());
        BLogger.Info("******storage free space:{0}", ReleaseUtil.GetStorageFreeSpace());
    }
    
	// Use this for initialization
	void Start ()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;


        DodSdkListener.Init();
        ///初始native库
        DodNativeMgr.Init();
        ///初始化基础库
        BaseLibMgr.InitLib();
        AssetVersionMgr.Instance.Init();

	    if (XGameStartParamData.fromGrayUpdate)
	    {
            StartCoroutine(CrtUpdateFromGrayUpdate(XGameStartParamData.grayVerInfo));
	        return;
        }

        if (XGameStartParamData.fromRepairClient)
        {
            StartCoroutine(CrtUpdateFromRepairClient());
            return;
        }

        InitAbStartup();

        EventLogReport.Instance.ReportEvent(EventLogType.event_startup);

        ///判断是否有可用的存储空间
        string dataPath = ReleaseUtil.GetDataDir();
        if (string.IsNullOrEmpty(dataPath))
        {
            EventLogReport.Instance.ReportEvent(EventLogType.event_fail_startup,"find sdcard failed");
            ShowText(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_FIND_SDCARD_FAILD));
            return;
        }

        //添加logo 处理
        var imgLogo = DUnityUtil.FindChildComponent<Image>(transform, "Logo");
        if (imgLogo != null)
        {
            UIReleaseTextureHelper.SetReleaseLogo(imgLogo, UIReleaseTextureHelper.LOGO_TEXTURE_NAME, true);
        }

        var imgBg = DUnityUtil.FindChildComponent<Image>(transform, "Background1");
        if (imgBg != null)
        {
            UIReleaseTextureHelper.SetReleaseLogo(imgBg, UIReleaseTextureHelper.START_TEXTURE_NAME, false);
        }

        StartCoroutine(GameStartup());
	}

    IEnumerator CrtUpdateFromGrayUpdate(AssetVerInfo verInfo)
    {
        yield return null;
        if (verInfo.m_updateType == AssetUpdateType.UPDATE_ASSET)
        {
            ///读取本地
            AssetVersionMgr.Instance.LoadLocalAllAssetMd5();

            //获取更新的大小
            StartTickTitle(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_CHECK_UPDATE_CONTENT));
            yield return StartCoroutine(AssetVersionMgr.Instance.DownloadLatestIndex(verInfo.m_assetUrl));
            yield return StartCoroutine(AssetVersionMgr.Instance.BeginUpdateIndex(verInfo.m_assetUrl));
            StopTickTitle();

            StartCoroutine(ContinueDownloadAb(verInfo));
        }
        else
        {
            StartCoroutine(ContinueDownloadProg(verInfo));
        }
    }

    IEnumerator CrtUpdateFromRepairClient()
    {
        AssetUpdater updater = AssetUpdater.Instance;
        yield return StartCoroutine(updater.CheckUpdate(true, this));
        StopTickTitle();

        AssetVerInfo svrVerInfo = updater.svrVerInfo;
        if (svrVerInfo == null)
        {
            ShowConfirm(
                StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_CHECK_VERSION_FAILED,
                    AssetVersionMgr.Instance.error), false, RetryUpdate);
            yield break;
        }

        ///读取本地
        AssetVersionMgr.Instance.LoadLocalAllAssetMd5();

        //获取更新的大小
        StartTickTitle(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_CHECK_UPDATE_CONTENT));
        yield return StartCoroutine(AssetVersionMgr.Instance.DownloadLatestIndex(svrVerInfo.m_assetUrl));
        //yield return StartCoroutine(AssetVersionMgr.Instance);
        StopTickTitle();

        if (AssetVersionMgr.Instance.haveErr)
        {
            ShowConfirm(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_CHECK_UPDATE_CONTENT_FAILED,
                AssetVersionMgr.Instance.error), false, RetryUpdate);
            yield break;
        }

        ShowConfirm(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_ASSET_UPDATE_INFO,
                AssetVersionMgr.Instance.GetUpdateFileCount(), GetDataSize(AssetVersionMgr.Instance.GetToUpdateSize())), true,
            OnUpdateConfirmOK, OnUpdateConfirmCancel);
    }

    private void StartTickTitle(string titlePrefix)
    {
        StopTickTitle();
        m_tickRefreshCort = StartCoroutine(TickRefreshTitle(titlePrefix));
    }

    private void StopTickTitle()
    {
        if (m_tickRefreshCort != null)
        {
            StopCoroutine(m_tickRefreshCort);
            m_tickRefreshCort = null;
        }
    }

    private IEnumerator TickRefreshTitle(string titlePrefix)
    {
        yield return null;
        string dotAppend = string.Empty;
        int dotNum = 0;
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            if (dotNum < 3)
            {
                dotNum++;
                dotAppend += ".";
            }
            else
            {
                dotNum = 0;
                dotAppend = string.Empty;
            }

            if (!AssetVersionMgr.Instance.haveErr)
            {
                ShowText(titlePrefix + dotAppend);
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// 准备预处理AB资源，从jar文件挪到data目录
    /// </summary>
    /// <returns></returns>
    IEnumerator GameStartup()
    {
        //m_process.gameObject.SetActive(false);
        AssetBundlePrepare abPrepare = AssetBundlePrepare.Instance;

        ShowText(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_GAME_ETNERING));
        m_labelVersion.text = "";

        yield return StartCoroutine(abPrepare.CheckNeedPrepare());

        ShowText(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_GAME_ETNERING));

        if (BaseConfigInfo.enableObbSplitMode)
        {
            if (ObbMgr.Instance.IsNeedDownloadObb())
            {
                SceneManager.LoadScene("Scene_obbdownload");
                yield break;
            }
        }

        var isReportPrepareEvent = false;
        var watchReport = new BTickWatcher();
        if (!abPrepare.IsHaveErr() && abPrepare.NeedPrepare)
        {
            EventLogReport.Instance.ReportEvent(EventLogType.event_start_unpack);
            isReportPrepareEvent = true;
        }

        yield return StartCoroutine(abPrepare.StartPrepareAb(this));

        if (abPrepare.IsHaveErr())
        {
            if (isReportPrepareEvent)
            {
                EventLogReport.Instance.ReportEvent(EventLogType.event_fail_unpack, abPrepare.error);
            }

            ShowConfirm(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_PREPARE_FAILED, abPrepare.error),false,
                DodLib.DoQuit);
            yield break;
        }

        if (isReportPrepareEvent)
        {
            EventLogReport.Instance.ReportEvent(EventLogType.event_end_unpack, watchReport.ElapseTime());
        }

        ///开始检测版本
        AssetVersionMgr.Instance.ReadBaseInfo();

        if (AssetVersionMgr.Instance.haveErr)
        {
            ShowConfirm(
                StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_ASSETVER_INIT_FAILED,
                    AssetVersionMgr.Instance.error), false,
                DodLib.DoQuit);
            yield break;
        }

        DBugly.Init(BaseConfigInfo.PlatformName, AssetVersionMgr.Instance.assetVersion);

//         //统计检查版本成功还是失败
//         PlatformUtil.StatisticsEventBegin("boot_update");

        //获取版本号
        string version = AssetVersionMgr.Instance.assetVersion;
        m_labelVersion.text = StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_VERSION_INFO, version);

        StartTickTitle(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_CHECK_UPDATE));
        AssetUpdater updater = AssetUpdater.Instance;
        yield return StartCoroutine(updater.CheckUpdate(false, this));
        StopTickTitle();

        AssetVerInfo svrVerInfo = updater.svrVerInfo;
        if (svrVerInfo == null)
        {
//             PlatformUtil.StatisticsEventFailed("boot_update", "network");
            ShowConfirm(
                StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_CHECK_VERSION_FAILED,
                    AssetVersionMgr.Instance.error), false, RetryUpdate);
            yield break;
        }

//         PlatformUtil.StatisticsEventEnd("boot_update");

        if (svrVerInfo.m_updateType == AssetUpdateType.UPDATE_NONE)
        {
            StartEnterGameScene();
        }
        else if (svrVerInfo.m_updateType == AssetUpdateType.UPDATE_ASSET)
        {
            ///读取本地
            AssetVersionMgr.Instance.LoadLocalAllAssetMd5();

            //获取更新的大小
            StartTickTitle(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_CHECK_UPDATE_CONTENT));
            yield return StartCoroutine(AssetVersionMgr.Instance.DownloadLatestIndex(svrVerInfo.m_assetUrl));
            yield return StartCoroutine(AssetVersionMgr.Instance.BeginUpdateIndex(svrVerInfo.m_assetUrl));
            StopTickTitle();

            if (AssetVersionMgr.Instance.haveErr)
            {
                ShowConfirm(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_CHECK_UPDATE_CONTENT_FAILED,
                        AssetVersionMgr.Instance.error), false, RetryUpdate);
                yield break;
            }
#if !UNITY_EDITOR
            if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
            {
                //直接下载
                OnUpdateConfirmOK();
            }
            else
#endif
            ShowConfirm(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_ASSET_UPDATE_INFO,
                    AssetVersionMgr.Instance.GetUpdateFileCount(), GetDataSize(AssetVersionMgr.Instance.GetToUpdateSize())), true,
                OnUpdateConfirmOK, OnUpdateConfirmCancel);
        }
        else if (svrVerInfo.m_updateType == AssetUpdateType.UPDATE_PROGRAME)
        {
            if (string.IsNullOrEmpty(svrVerInfo.m_help))
            {
                ShowConfirm(svrVerInfo.m_err, true,
                    OnUpdateConfirmOK, OnUpdateConfirmCancel);
            }
            else
            {
                ShowConfirmIos(svrVerInfo.m_err, OnUpdateConfirmOK, () =>
                    {
                        Application.OpenURL(svrVerInfo.m_help);
                    });
            }
        }
        else if (svrVerInfo.m_updateType == AssetUpdateType.UPDATE_ERROR)
        {
            ShowText(svrVerInfo.m_err);
        }
        else
        {
            ShowText(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_CHECK_UPDATE_JSON_DETAIL,
                svrVerInfo.m_updateType));
        }
    }

    private void StartEnterGameScene()
    {
        ShowText(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_GAME_ETNERING));
        //m_process.gameObject.SetActive(false);

        ReleaseUtil.OnGameEnter();
        StartCoroutine(DelayLoadScene());
    }

    /// <summary>
    /// 读取ab缓存的配置
    /// </summary>
    /// <returns></returns>
    private bool LoadAbCacheConfig()
    {
        TextAsset needPersistAsset = XResource.LoadAsset<TextAsset>("Config/AbConfig/need_persist_assetbundle");
        if (needPersistAsset == null)
        {
            BLogger.Error("read resource failed: {0}", "need_persist_assetbundle");
            return false;
        }

        List<string> listAbPath = AssetBundleUtil.ReadTextStringList(needPersistAsset.bytes);
        List<string> listMd5Path = new List<string>();
        foreach (string abPath in listAbPath)
        {
            listMd5Path.Add(AssetBundleUtil.GetPathHash(abPath));
        }

        AssetBundlePool.Instance.RegPersistAssetBundlePath(listMd5Path);
        //Logger.Error("regist persist assetbundle count: {0}", listMd5Path.Count);
        return true;
    }

    private VfsSystem VfsAndroidCreateAbSystem()
    {
        VfsSystem abVfs = new VfsSystem();

        //优先读取sdcard里的ab目录
        var localPath = AssetBundleUtil.GetExternAssetBundleDir();
        abVfs.AddVfsDriver(new VfsLocalFsDriver(localPath));//, "file://"));

        BLogger.Info("Init local fs driver: {0}", localPath);

        string zipPath = Application.dataPath;
        abVfs.AddVfsDriver(new ZipVfsDriver(zipPath));

        BLogger.Info("Init driver zipPath: {0}", zipPath);

        return abVfs;
    }

    private VfsSystem VfsWindowsCreateAbSystem()
    {
        VfsSystem abVfs = new VfsSystem();

        //优先读取sdcard里的ab目录
        abVfs.AddVfsDriver(new VfsLocalFsDriver(AssetBundleUtil.GetExternAssetBundleDir()));//, "file:///"));

        /*abVfs.AddVfsDriver(new VfsLocalFsDriver(Application.dataPath + "/../ab_enc/cab/",
            "file:///"));*/
        abVfs.AddVfsDriver(new VfsLocalFsDriver(Application.dataPath + "/../ab_enc/ab/"));/*,
            "file:///"));*/
        return abVfs;
    }

    private VfsSystem VfsIphoneCreateAbSystem()
    {
        VfsSystem abVfs = new VfsSystem();

        //优先读取sdcard里的ab目录
        abVfs.AddVfsDriver(new VfsLocalFsDriver(AssetBundleUtil.GetExternAssetBundleDir()));//, "file://"));
        abVfs.AddVfsDriver(new VfsLocalFsDriver(Application.streamingAssetsPath + "/ab/"));//, "file://"));
        return abVfs;
    }

    private void InitCustomAbVfs()
    {
        AssetBundleFileMgr abFileMgr = AssetBundleFileMgr.Instance;

#if UNITY_EDITOR
        abFileMgr.RegCustomCreateVfs(VfsWindowsCreateAbSystem);
#elif UNITY_ANDROID
        abFileMgr.RegCustomCreateVfs(VfsAndroidCreateAbSystem);
#elif UNITY_IPHONE
        abFileMgr.RegCustomCreateVfs(VfsIphoneCreateAbSystem);
#endif
    }

    IEnumerator DelayLoadScene()
    {
        yield return null;

        var watchReport = new BTickWatcher();
        EventLogReport.Instance.ReportEvent(EventLogType.event_start_enter_game);
        
        InitCustomAbVfs();
        
        if (!AssetBundleLoader.Instance.Init(true))
        {
            EventLogReport.Instance.ReportEvent(EventLogType.event_fail_enter_game, "AssetBundleLoader.Init failed");

            BLogger.Error("AssetBundleLoader init failed");
            ShowText("进入游戏失败(资源错误)");
            yield break;
        }
        
        //读取缓存策略
        LoadAbCacheConfig();

        SceneAssetLoader loader = SceneAssetLoader.Instance;
        var watchStep = new BTickWatcher();

        float preloadConfigPercent = 0.3f;

        EventLogReport.Instance.ReportEvent(EventLogType.event_preload_config, watchStep.ElapseTime());
        watchStep.Refresh();

        float currPercent = preloadConfigPercent;
        yield return AssetBundleLoader.Instance.StartCoroutine(UnitySceneLoader.LoadSceneAsync(m_startScene,
            (progress) =>
            {
                float actualProgress = currPercent + (1.0f - currPercent) * progress;
                int currProgress = (int)(actualProgress * 100);
                if (m_title != null)
                {
                    m_title.text = StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_GAME_ETNERING) +
                                    currProgress.ToString() + "%";
                    SetProgress((uint)currProgress, 100);
                }

                BLogger.Info("scene loading process:{0}", currProgress);
            }, null));

        EventLogReport.Instance.ReportEvent(EventLogType.event_load_scene0, watchStep.ElapseTime());
        watchStep.Refresh();

        if (loader.haveError)
        {
            ShowConfirm(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_ENTER_GAME_FAILED,
                loader.lastError), false, DodLib.DoQuit);
            yield break;
        }

        EventLogReport.Instance.ReportEvent(EventLogType.event_end_enter_game, watchReport.ElapseTime());
    }

    private string GetDataSize(int size)
    {
        float mSize = ((float) size)/1024/1024;
        string disSize = mSize.ToString("f3") + "M";

        return disSize;
    }

    void Update()
    {
        if (m_downTick != null)
        {
            m_downTick.OnUpdate();
        }
    }

    public void OnUpdateConfirmCancel()
    {
        DodLib.DoQuit();
    }

    #region StartUP

    public void ShowText(string title)
    {        
        m_title.text = (title == null ? "" : title);
    }

    public void ShowDownInfo(float downzise, float totalsize, bool show, float speendown, bool init = false)
    {
        if (show)
        {
            if (init)
            {
                m_curspeeddownSize = speendown;
                m_curdownSize = downzise;

                m_totaldownSize = totalsize;
            }
            else
            {
                m_curspeeddownSize += speendown;
                m_curdownSize = downzise;
            }

            SetDonwInfo();
        }
        else
        {
            m_downBeginTime = 0;
            m_curspeeddownSize = 0;
            m_curdownSize = 0;
            m_totaldownSize = 0;
            m_downTick = null;
        }
    }

    public void BeginDownload()
    {
        if (m_downBeginTime <= 0 || m_downTick == null)
        {
            m_downBeginTime = Time.realtimeSinceStartup;
            SetDonwInfo();
            m_downTick = new BTimerTick(1f, SetDonwInfo);
        }
    }

    public void ShowConfirm(string detail, bool showCancel = true, 
        Action confirmAction = null, Action cancelAction = null)
    {
        m_confirmUI.ShowConfirm(detail, showCancel, confirmAction, cancelAction);
    }

    public void ShowConfirmIos(string detail,Action confirmAction = null, Action helpAction = null)
    {
        m_confirmUIIos.ShowConfirm(detail, confirmAction, helpAction);
    }

    #endregion

    private string GetDownDataSize(int size)
    {
        if (size > 1024 * 1024)
        {
            float mSize = ((float)size) / 1024 / 1024;
            return mSize.ToString("f2") + "M";
        }
        else if (size > 1024)
        {
            float kSize = ((float)size) / 1024;
            return kSize.ToString("f2") + "KB";
        }
        else
        {
            return size + "B";
        }
    }

    private void SetDonwInfo()
    {
        string totaldown = GetDataSize((int)m_totaldownSize);
        string nowdown = GetDataSize((int)m_curdownSize);
        int percent = (int)(m_curdownSize / m_totaldownSize * 100);

        float downloadSize = Application.internetReachability == NetworkReachability.NotReachable ? 0 : m_curspeeddownSize;
        float time = Time.realtimeSinceStartup - m_downBeginTime;
        string download = GetDownDataSize((int)(downloadSize / time));

        string progress = StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_TXT_DOWNINFO,
                nowdown, totaldown, percent, download);

        ShowText(progress);
    }

    public void SetProgress(uint curValue, uint maxValue, bool isPercentage = false)
    {
        if (m_process != null)
        {
            m_process.minValue = 0;
            m_process.maxValue = maxValue;
            m_process.value = curValue;
        }
    }

    public void ShowProgressBar(bool show)
    {
        // 策划的需求，不隐藏进度条
        //if (m_process != null)
        //{
        //    m_process.gameObject.SetActive(show);
        //}
    }

    #region 下载相关

    IEnumerator ContinueDownloadAb(AssetVerInfo svrVerInfo)
    {
        var watchReport = new BTickWatcher();
        EventLogReport.Instance.ReportEvent(EventLogType.event_start_update_ab);

        //开始更新数据包
        ShowText(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_ASSET_UPDATE));
        yield return StartCoroutine(AssetVersionMgr.Instance.BeginUpdateAsset(svrVerInfo.m_assetUrl, this));
        if (AssetVersionMgr.Instance.haveErr)
        {
            EventLogReport.Instance.ReportEvent(EventLogType.event_fail_update_ab, AssetVersionMgr.Instance.error);

            ShowConfirm(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_ASSET_UPDATE_FAILED,
                AssetVersionMgr.Instance.error), true, RetryUpdate);
            yield break;
        }

        EventLogReport.Instance.ReportEvent(EventLogType.event_end_update_ab, watchReport.ElapseTime());

        //判断是否更新了脚本，如果更新了脚本，那么需要重新启动进程
        if (AssetVersionMgr.Instance.scriptUpdated ||
            XGameStartParamData.fromGrayUpdate || /*如果是灰度更新界面切过来的，那么肯定要重启*/
            XGameStartParamData.fromRepairClient)
        {
            ShowConfirm(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_UPDATE_NEED_RESTART), false,
                DodLib.RestartApp);
            yield break;
        }

        //如果是资源更新了，那么重新读取本地版本信息
        AssetVersionMgr.Instance.ReadBaseInfo();

        StartEnterGameScene();
    }

    /// <summary>
    /// 开始下载整包
    /// </summary>
    /// <returns></returns>
    IEnumerator ContinueDownloadProg(AssetVerInfo svrVerInfo)
    {
        OnManualUpdate();
        yield break;

//         yield return null;
//         //yield return null;
//         BLogger.Error("StartDownload prog: {0}", svrVerInfo.m_progUrl);
//         ReleaseUtil.StartDownLoadProg(svrVerInfo.m_progUrl);
//         yield return null;

        //如果是Ios的话，就可以直接返回了
#if !UNITY_ANDROID
        DodLib.DoQuit();
        yield break;
#endif

        SetProgress(0, 100);

        bool isDownRun = true;
        while (isDownRun)
        {
            ProgDownloadState stateData = ReleaseUtil.GetDownloadState(svrVerInfo.m_progUrl);
            if (stateData == null ||
                stateData.status == ProgDownloadStatus.STATUS_NONE)
            {
                ShowText(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_UPDATE_FINISH));
                isDownRun = false;

                ShowManualDownload();
                break;
            }

            if (stateData.totalSize > 0)
            {
                SetProgress((uint)stateData.downloadedSize, (uint)stateData.totalSize);
            }

            switch (stateData.status)
            {
                case ProgDownloadStatus.STATUS_FAILED:
                    {
                        isDownRun = false;
                        ShowText(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_DOWNLOAD_PROG_FAILED,
                            stateData.statusReason));
                        ShowManualDownload();
                    }
                    break;

                case ProgDownloadStatus.STATUS_PAUSED:
                    {
                        ShowText(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_DOWNLOAD_PROG_PAUSE,
                            stateData.statusReason));
                        ShowManualDownload();
                    }
                    break;

                case ProgDownloadStatus.STATUS_PENDING:
                    {
                        ShowText(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_DOWNLOAD_PROG_WAIT));
                        ShowManualDownload();
                    }
                    break;

                case ProgDownloadStatus.STATUS_RUNNING:
                    {
                        if (stateData.totalSize > 0)
                        {
                            ShowText(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_DOWNLOAD_PROG_PROGRESS,
                                GetDataSize(stateData.downloadedSize) + "/" + GetDataSize(stateData.totalSize)));
                        }
                        else
                        {
                            ShowText(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_DOWNLOAD_PROG_START));
                        }
                    }
                    break;

                case ProgDownloadStatus.STATUS_SUCCESSFUL:
                    {
                        isDownRun = false;
                        ShowText(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_DOWNLOAD_PROG_SUCCESS));

#if UNITY_ANDROID //当前下载任务正在进行中
                        //直接调用安装
                        bool ret = DodLib.InstallApk(svrVerInfo.m_progUrl);
                        BLogger.Assert(ret);
#endif
                    }
                    break;

                default:
                    break;
            }
            yield return null;
            yield return null;
            yield return null;
        }
    }

    /// <summary>
    /// 更新点击确定
    /// </summary>
    private void OnUpdateConfirmOK()
    {
        AssetUpdater updater = AssetUpdater.Instance;
        AssetVerInfo verInfo = updater.svrVerInfo;
        BLogger.Assert(verInfo != null);
        SDKCallback.OnUpgrade();

        if (verInfo.m_updateType == AssetUpdateType.UPDATE_ASSET || verInfo.m_updateType == AssetUpdateType.UPDATE_REPAIR_CLIENT)
        {
            StartCoroutine(ContinueDownloadAb(verInfo));
        }
        else
        {
            StartCoroutine(ContinueDownloadProg(verInfo));
        }
    }

    /// <summary>
    /// 重新下载点击确定
    /// </summary>
    private void RetryUpdate()
    {
        if (XGameStartParamData.fromGrayUpdate)
        {
            StartCoroutine(CrtUpdateFromGrayUpdate(XGameStartParamData.grayVerInfo));
            return;
        }

        if (XGameStartParamData.fromRepairClient)
        {
            StartCoroutine(CrtUpdateFromRepairClient());
            return;
        }

        StartCoroutine(GameStartup());
    }

    protected void ShowManualDownload()
    {
        //先去除手动下载的逻辑
    }

    /// <summary>
    /// 点击手动下载
    /// </summary>
    public void OnManualUpdate()
    {
        AssetUpdater updater = AssetUpdater.Instance;
        AssetVerInfo verInfo = updater.svrVerInfo;
        BLogger.Assert(verInfo != null);

        ReleaseUtil.StartManualDownLoadProg(verInfo.m_progUrl);
    }
    #endregion
}

