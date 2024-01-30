using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using Newtonsoft.Json;

namespace AmmoManager
{
    public class AmmoManager : BasePlugin
    {
        public override string ModuleName => "AmmoManager";
        public override string ModuleAuthor => "Oylsister";
        public override string ModuleVersion => "1.0";

        public Dictionary<string, AmmoSetting> AmmoSetting = new();

        public override void Load(bool hotReload)
        {
            var configPath = Path.Combine(ModuleDirectory, "ammosetting.json");
            AmmoSetting = JsonConvert.DeserializeObject<Dictionary<string, AmmoSetting>>(File.ReadAllText(configPath))!;

            RegisterListener<Listeners.OnEntityCreated>(OnAnyEntityCreated);
        }

        public void OnAnyEntityCreated(CEntityInstance entity)
        {
            if (!entity.DesignerName.Contains("weapon_"))
            {
                return;
            }

            ApplyNewAmmo(entity);
        }

        private void ApplyNewAmmo(CEntityInstance entity)
        {
            CBasePlayerWeapon weapon = new CBasePlayerWeapon(entity.Handle);
            var weaponname = FindWeaponItemDefinition(weapon, weapon.DesignerName);

            if (AmmoSetting.ContainsKey(weaponname))
            {
                var clip = AmmoSetting[weaponname].ClipSize;
                var reserved = AmmoSetting[weaponname].ReserveAmmo;

                var weaponbase = weapon.As<CCSWeaponBase>();

                if (clip > -1 || reserved > -1)
                {
                    Server.NextFrame(() =>
                    {
                        if (clip > -1)
                        {
                            weaponbase.VData!.MaxClip1 = clip;
                            weaponbase.VData.DefaultClip1 = clip;
                            weaponbase.Clip1 = clip;
                            Utilities.SetStateChanged(weaponbase, "CBasePlayerWeapon", "m_Clip1");
                        }

                        if (reserved > -1)
                        {
                            weaponbase.VData!.PrimaryReserveAmmoMax = reserved;
                            weaponbase.ReserveAmmo[0] = reserved;
                            Utilities.SetStateChanged(weaponbase, "CBasePlayerWeapon", "m_pReserveAmmo");
                        }
                    });
                }
            }
        }

        public string FindWeaponItemDefinition(CBasePlayerWeapon weapon, string weaponstring)
        {
            var item = (ItemDefinition)weapon.AttributeManager.Item.ItemDefinitionIndex;

            if (weaponstring == "weapon_m4a1")
            {
                switch (item)
                {
                    case ItemDefinition.M4A1_S: return "weapon_m4a1_silencer";
                    case ItemDefinition.M4A4: return "weapon_m4a1";
                }
            }

            else if (weaponstring == "weapon_hkp2000")
            {
                switch (item)
                {
                    case ItemDefinition.P2000: return "weapon_hkp2000";
                    case ItemDefinition.USP_S: return "weapon_usp_silencer";
                }
            }

            else if (weaponstring == "weapon_mp7")
            {
                switch (item)
                {
                    case ItemDefinition.MP7: return "weapon_mp7";
                    case ItemDefinition.MP5_SD: return "weapon_mp5sd";
                }
            }

            return weaponstring;
        }
    }
}

public class AmmoSetting
{
    [JsonProperty(PropertyName = "clip")]
    public int ClipSize { get; set; } = -1;

    [JsonProperty(PropertyName = "reserved")]
    public int ReserveAmmo { get; set; } = -1;
}
