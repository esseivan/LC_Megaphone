using System;
using System.Collections.Generic;
using System.Text;
using BepInEx;
using BepInEx.Configuration;

namespace Megaphone;

public class MyConfig
{
    public const int SIREN_NOISE_ID = 1880;

    public static bool CanBuy => configCanBuy.Value;
    public static bool IsScrap => configIsScrap.Value;
    public static int Rarity
    {
        get
        {
            int x = (int)MathF.Max(0, MathF.Min(100, configRarity.Value));
            return x;
        }
    }
    public static int Price => configPrice.Value;
    public static float HearDistance
    {
        get
        {
            float x = configHearDistance.Value;
            if (x <= 0)
                x = 1.0f;
            return x;
        }
    }
    public static float SirenHearDistance
    {
        get
        {
            float x = configSirenHearDistance.Value;
            if (x <= 0)
                x = 1.0f;
            return x;
        }
    }
    public static float SFXHearDistance
    {
        get
        {
            float x = configSFXHearDistance.Value;
            if (x <= 0)
                x = 1.0f;
            return x;
        }
    }
    public static float SFXEnemyHearDistance
    {
        get
        {
            float x = configSFXEnemyHearDistance.Value;
            if (x <= 0)
                x = 1.0f;
            return x;
        }
    }
    public static float LoudVoiceVolume
    {
        get
        {
            float x = configLoudVoiceVolume.Value;
            x = MathF.Min(1.2f, x); // Cap at 1.2
            x = MathF.Max(0.0f, x);
            return x;
        }
    }
    public static float SFXVolume
    {
        get
        {
            float x = configSFXVolume.Value;
            x = MathF.Min(1.2f, x); // Cap at 1.2
            x = MathF.Max(0.0f, x);
            return x;
        }
    }
    public static float RobotVoicePitch
    {
        get
        {
            float x = configRobotVoicePitch.Value;
            x = MathF.Min(2.0f, x); // Cap at 1.2
            x = MathF.Max(0.5f, x);
            return x;
        }
    }

    private static ConfigEntry<bool> configCanBuy;
    private static ConfigEntry<bool> configIsScrap;
    private static ConfigEntry<int> configRarity;
    private static ConfigEntry<int> configPrice;
    private static ConfigEntry<float> configHearDistance;
    private static ConfigEntry<float> configSirenHearDistance;
    private static ConfigEntry<float> configSFXHearDistance;
    private static ConfigEntry<float> configSFXEnemyHearDistance;
    private static ConfigEntry<float> configLoudVoiceVolume;
    private static ConfigEntry<float> configSFXVolume;
    private static ConfigEntry<float> configRobotVoicePitch;

    public static void Setup(BaseUnityPlugin p)
    {
        configCanBuy = p.Config.Bind(
            "Item",
            "CanBuy",
            true,
            @"Can the item be bought from the terminal.
[Client side]"
        );

        configIsScrap = p.Config.Bind(
            "Item",
            "IsScrap",
            true,
            @"Can the item spawn in interiors.
[Host side]"
        );

        configRarity = p.Config.Bind(
            "Item",
            "Rarity",
            100,
            @"Rarity of the object. 0 is never, 100 is often.
Min: 0      Max: 100
[Host side]"
        );

        configPrice = p.Config.Bind(
            "Item",
            "Price",
            15,
            @"Buy cost of the item.
[Client side]"
        );

        configHearDistance = p.Config.Bind(
            "Audio.Distances",
            "HearingDistanceModifier",
            2.0f,
            @"Change the distance multiplier the voices can be heard from when talking in
'loud mode' (switch with Q).
Min: 0.0
[Client side]"
        );

        configSirenHearDistance = p.Config.Bind(
            "Audio.Distances",
            "SirenHearingDistanceModifier",
            2.0f,
            @"Change the distance multiplier the siren can be heard from (switch with Q).
Min: 0.0
[Client side]"
        );

        configSFXHearDistance = p.Config.Bind(
            "Audio.Distances",
            "SFXHearingDistanceModifier",
            2.0f,
            @"Change the distance multiplier the SFX can be heard from.
Min: 0.0
[Client side]"
        );

        configSFXEnemyHearDistance = p.Config.Bind(
            "Audio.Distances",
            "EnemySFXHearingDistanceModifier",
            1.0f,
            @"Change the distance multiplier the SFX can be heard from by enemies.
This multiplier is applied after the base hear distance modifier.
Min: 0.0
[Client side]"
        );

        configLoudVoiceVolume = p.Config.Bind(
            "Audio.Volume",
            "LoudVoiceVolume",
            0.9f,
            @"Volume multiplier of the 'Loud voice' filter. Recommended below 1.0 because
the distortion increases the volume, 1.0 is already higher than normal voice.
Min: 0.0    Max: 1.2
[Client side]"
        );

        configSFXVolume = p.Config.Bind(
            "Audio.Volume",
            "SFXVolume",
            1.0f,
            @"Volume multiplier of the SFX sounds.
Min: 0.0    Max: 1.2
[Client side]"
        );

        configRobotVoicePitch = p.Config.Bind(
            "Audio.Pitch",
            "RobotVoicePitch",
            0.9f,
            @"Set the pitch for the robot voice.
Min: 0.5    Max: 2.0
[Client side]"
        );
    }
}
