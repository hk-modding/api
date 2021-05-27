using MonoMod;

// ReSharper disable All
#pragma warning disable 1591, 0108, 0169, 0649, 0626

namespace Modding.Patches
{
    [MonoModPatch("global::MenuSetting")]
    public class MenuSetting : global::MenuSetting
    {
        public delegate void ApplySetting(MenuSetting self, int settingIndex);
        public delegate void RefreshSetting(MenuSetting self, bool alsoApplySetting);

        public ApplySetting customApplySetting { get; set; }
        public RefreshSetting customRefreshSetting { get; set; }

        public extern void orig_UpdateSetting(int settingIndex);
        public extern void orig_RefreshValueFromGameSettings(bool alsoApplySetting);

        public void UpdateSetting(int settingIndex)
        {
            if (
                (MenuSettingType)this.settingType == MenuSettingType.CustomSetting &&
                this.customApplySetting != null
            )
            {
                this.customApplySetting?.Invoke(this, settingIndex);
            }
            else
            {
                orig_UpdateSetting(settingIndex);
            }
        }

        public void RefreshValueFromGameSettings(bool alsoApplySetting = false)
        {
            if (
                (MenuSettingType)this.settingType == MenuSettingType.CustomSetting &&
                this.customRefreshSetting != null
            )
            {
                this.customRefreshSetting?.Invoke(this, alsoApplySetting);
            }
            else
            {
                orig_RefreshValueFromGameSettings(alsoApplySetting);
            }
        }

        public enum MenuSettingType
        {
            // what the fuck is this
            Resolution = 10,
            FullScreen,
            VSync,
            // where did 13 go
            MonitorSelect = 14,
            FrameCap,
            ParticleLevel,
            ShaderQuality,
            // HUH????
            GameLanguage = 33,
            GameBackerCredits,
            NativeAchievements,
            NativeInput,
            ControllerRumble,
            // peepoHappy
            CustomSetting
        }
    }
}