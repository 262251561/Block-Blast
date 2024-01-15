using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using UnityEngine;

namespace Game.Config
{
    public static class ConfigLoader
    {
        public enum ConfigType
        {
        }

        public static IEnumerator LoadAllConfigs()
        {
            //loadingTask.Add(Addressables.LoadAssetAsync<TextAsset>("Assets/AssetBundles/Config/Item.csv"));
            //loadingTask.Add(Addressables.LoadAssetAsync<TextAsset>("Assets/AssetBundles/Config/CityBuilding.csv"));
            //loadingTask.Add(Addressables.LoadAssetAsync<TextAsset>("Assets/AssetBundles/Config/CityBuildingEnable.csv"));
            //loadingTask.Add(Addressables.LoadAssetAsync<TextAsset>("Assets/AssetBundles/Config/CityBuildingProduction.csv"));
            //loadingTask.Add(Addressables.LoadAssetAsync<TextAsset>("Assets/AssetBundles/Config/CityInitMap.csv"));
            //loadingTask.Add(Addressables.LoadAssetAsync<TextAsset>("Assets/AssetBundles/Config/Hero.csv"));
            //loadingTask.Add(Addressables.LoadAssetAsync<TextAsset>("Assets/AssetBundles/Config/HeroLevelUp.csv"));
            //loadingTask.Add(Addressables.LoadAssetAsync<TextAsset>("Assets/AssetBundles/Config/HeroStarUp.csv"));
            //loadingTask.Add(Addressables.LoadAssetAsync<TextAsset>("Assets/AssetBundles/Config/Profession.csv"));
            //loadingTask.Add(Addressables.LoadAssetAsync<TextAsset>("Assets/AssetBundles/Config/Dispatch.csv"));
            //loadingTask.Add(Addressables.LoadAssetAsync<TextAsset>("Assets/AssetBundles/Config/DispatchEffect.csv"));
            //loadingTask.Add(Addressables.LoadAssetAsync<TextAsset>("Assets/AssetBundles/Config/Chapter.csv"));
            //loadingTask.Add(Addressables.LoadAssetAsync<TextAsset>("Assets/AssetBundles/Config/MainStage.csv"));
            //loadingTask.Add(Addressables.LoadAssetAsync<TextAsset>("Assets/AssetBundles/Config/DrawCard.csv"));
            //loadingTask.Add(Addressables.LoadAssetAsync<TextAsset>("Assets/AssetBundles/Config/Language.csv"));
            //loadingTask.Add(Addressables.LoadAssetAsync<TextAsset>("Assets/AssetBundles/Config/ConstantValue.json"));
            //loadingTask.Add(Addressables.LoadAssetAsync<TextAsset>("Assets/AssetBundles/Config/BuildingCraftsmanAni.bytes"));
            //loadingTask.Add(Addressables.LoadAssetAsync<TextAsset>("Assets/AssetBundles/Config/ClientConstantValue.bytes"));
            //loadingTask.Add(Addressables.LoadAssetAsync<TextAsset>("Assets/AssetBundles/Config/BattleEffectInfo.bytes"));
            //loadingTask.Add(Addressables.LoadAssetAsync<TextAsset>("Assets/AssetBundles/Config/BattleRoleRes.bytes"));
            //loadingTask.Add(Addressables.LoadAssetAsync<TextAsset>("Assets/AssetBundles/Config/ACGameLib.bytes"));

            //int i, length = loadingTask.Count;
            //for (i = 0; i < length; ++i)
            //    yield return loadingTask[i].Task;

            //i = 0;
            //LoadConfig<CityItemConfigManager>(loadingTask[i++].Result.text);
            //LoadConfig<CityBuildingConfigManager>(loadingTask[i++].Result.text);
            //LoadConfig<CityBuildingEnableConfigManager>(loadingTask[i++].Result.text);
            //LoadConfig<CityBuildingProductionConfigManager>(loadingTask[i++].Result.text);
            //LoadConfig<CityInitMapConfigManager>(loadingTask[i++].Result.text);
            //LoadConfig<HeroConfigManager>(loadingTask[i++].Result.text);
            //LoadConfig<HeroLevelupConfigManager>(loadingTask[i++].Result.text);
            //LoadConfig<HeroStarupConfigManager>(loadingTask[i++].Result.text);
            //LoadConfig<ProfessionConfigManager>(loadingTask[i++].Result.text);
            //LoadConfig<DispatchSkillConfigManager>(loadingTask[i++].Result.text);
            //LoadConfig<DispatchSkillEffectConfigManager>(loadingTask[i++].Result.text);
            //LoadConfig<ChapterConfigManager>(loadingTask[i++].Result.text);
            //LoadConfig<MainStageConfigManager>(loadingTask[i++].Result.text);
            //LoadConfig<DrawCardConfigManager>(loadingTask[i++].Result.text);
            //LoadConfig<LanguageConfigManager>(loadingTask[i++].Result.text);

            //var consantValue = new ConstantValueConfigManager();
            //consantValue.Load(loadingTask[i++].Result.text);
            //s_allConfigs.Add(consantValue);
            //LoadConfig<CityBuildingCraftmanAniConfigManager>(loadingTask[i++].Result.bytes);
            //LoadConfig<ClientConstantValueConfigManager>(loadingTask[i++].Result.bytes);
            //LoadConfig<BattleEffectInfoConfigManager>(loadingTask[i++].Result.bytes);
            //LoadConfig<BattleRoleResConfigManager>(loadingTask[i++].Result.bytes);

            //LoadCoreConfigs(loadingTask[i++].Result.bytes);

            //length = loadingTask.Count;
            //for (i = 0; i < length; ++i)
            //    Addressables.Release(loadingTask[i]);

            yield return null;
        }

        public static void AddConfig<DerType>()
            where DerType : IConfigTable, IDisposable, new()
        {
            DerType configManager = new DerType();
        }

        public static void LoadConfig<DerType>(string contents)
            where DerType : IConfigTable, IDisposable, new()
        {
            DerType configManager = new DerType();
            configManager.ProcessCSV(contents);
        }
    }
}
