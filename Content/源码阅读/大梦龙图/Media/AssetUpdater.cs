using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DodGame
{
    public class AssetUpdater : BBehaviourSingleton<AssetUpdater>
    {
        private bool m_canEnterGame = false;
        private bool m_needRestart = false;

        private AssetVerInfo m_svrVerInfo = null;

        public AssetVerInfo svrVerInfo
        {
            get { return m_svrVerInfo; }
        }

        public string NewVersion
        {
            get { return m_svrVerInfo.m_version; }
        }

        public IEnumerator CheckUpdate(bool repairClient, StartupUI startUI)
        {
            m_svrVerInfo = null;

            //获取当前的版本
            string fullUrl = DPlatform.GetCheckVersionUrl(repairClient);
            BLogger.Info("version check:{0}", fullUrl);

            WWWEx wwwEx = WWWEx.Init(15f);
            yield return StartCoroutine(wwwEx.Request(fullUrl));
            if (wwwEx.isTimeOut)
            {
                AssetVersionMgr.Instance.error = StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_CHECK_UPDATE_TIMEOUT);

                BLogger.Warning("url reueqest timeout: {0}", fullUrl);
                yield break;
            }

            WWW www = wwwEx.result;
            if (www.error != null)
            {
                BLogger.Warning("check version update error: " + www.error);
                string wwwerror = www.error;
                www.Dispose();
                AssetVersionMgr.Instance.error = StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_CHECK_UPDATE_WWW_ERROR, wwwerror);
                yield break;
            }

            if (startUI != null)
            {
                startUI.ShowText(StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_GAME_ETNERING));
            }

            Dictionary<string, object> verJson = (Dictionary<string, object>)MiniJSON.Json.Deserialize(www.text);
            if (verJson == null || verJson.Count == 0)
            {
                BLogger.Warning("parse version json data error: {0}", www.text);
                AssetVersionMgr.Instance.error =
                    StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_CHECK_UPDATE_JSON_ERROR, 1);
                
//                 DBugly.ReportException("json data error", "request error url:" + fullUrl, "parse version json data error: " + www.text);
                www.Dispose();
                yield break;
            }

            try
            {
                m_svrVerInfo = ParseVerInfo(verJson);
            }
            catch (Exception e)
            {
                BLogger.Warning("parse ver info failed:{0}", e.ToString());
                DBugly.ReportException("parse ver info", "request error url:" + fullUrl, "parse ver info failed: " + e.ToString());
                AssetVersionMgr.Instance.error =
                    StartupTextConfigMgr.Instance.GetText(StartupTextDefine.ID_STARTUP_CHECK_UPDATE_JSON_ERROR, 2);
                m_svrVerInfo = null;
            }

            www.Dispose();
        }

        public AssetVerInfo ParseVerInfo(Dictionary<string, object> verJson)
        {
            AssetVerInfo newInfo = new AssetVerInfo();
            newInfo.m_name = ReadJsonKey<string>(verJson, "name");///verJson["name"] as string;
            newInfo.m_assetUrl = ReadJsonKey<string>(verJson, "asseturl");//verJson["asseturl"] as string;
            newInfo.m_version = ReadJsonKey<string>(verJson, "version"); //verJson["version"] as string;
            newInfo.m_updateType = (AssetUpdateType)ReadJsonKey<Int64>(verJson, "update");  //(AssetUpdateType)((int)verJson["update"]);
//             if (verJson.ContainsKey("review"))
//             {
//                 AssetVersionMgr.Instance.ReviewStatus = ReadJsonKey<Int64>(verJson, "review") == 1;
//             }

            if (newInfo.m_updateType == AssetUpdateType.UPDATE_PROGRAME)
            {
                newInfo.m_err = ReadJsonKey<string>(verJson, "err");    //标题
                newInfo.m_progSize = (int)ReadJsonKey<Int64>(verJson, "prog_size");    //安装包大小
                newInfo.m_progUrl = ReadJsonKey<string>(verJson, "prog_url");   //下载路径
                newInfo.m_help = ReadJsonKey<string>(verJson, "prog_help");   //帮助页面路径
            }
            if (newInfo.m_updateType == AssetUpdateType.UPDATE_ERROR)
            {
                newInfo.m_err = ReadJsonKey<string>(verJson, "err");    //错误信息
            }

            return newInfo;
        }
        
        private T ReadJsonKey<T>(Dictionary<string, object> json, string key)
        {
            return AssetBundleUtil.ReadJsonKey<T>(json, key);
        }

    }

}
