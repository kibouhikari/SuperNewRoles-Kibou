using System;
using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using SuperNewRoles.CustomRPC;
using SuperNewRoles.Helpers;
using SuperNewRoles.Mode;
using UnityEngine;
using static SuperNewRoles.Helpers.RPCHelper;
using static SuperNewRoles.OutfitManager;

namespace SuperNewRoles.Roles
{
    class Seer
    //&MadSeer & EvilSeer & SeerFriends & JackalSeer & Sidekick(Seer)

    {
        private static Sprite SoulSprite;
        public static Sprite getSoulSprite()
        {
            if (SoulSprite) return SoulSprite;
            SoulSprite = ModHelpers.loadSpriteFromResources("SuperNewRoles.Resources.Soul.png", 500f);
            return SoulSprite;
        }//霊魂のスプライトを取得

        public static class ExileControllerWrapUpPatch
        {
            public static void WrapUpPostfix(GameData.PlayerInfo exiled)
            {
                var role = PlayerControl.LocalPlayer.getRole();
                if (role is RoleId.Seer or RoleId.MadSeer or RoleId.EvilSeer or RoleId.SeerFriends or RoleId.JackalSeer or RoleId.SidekickSeer)
                {
                    List<Vector3> DeadBodyPositions = new();
                    bool limitSoulDuration = false;
                    float soulDuration = 0f;
                    switch (role)
                    {
                        case RoleId.Seer:
                            DeadBodyPositions = RoleClass.Seer.deadBodyPositions;
                            RoleClass.Seer.deadBodyPositions = new List<Vector3>();
                            limitSoulDuration = RoleClass.Seer.limitSoulDuration;
                            soulDuration = RoleClass.Seer.soulDuration;
                            if (RoleClass.Seer.mode is not 0 and not 2) return;
                            break;
                        case RoleId.MadSeer:
                            DeadBodyPositions = RoleClass.MadSeer.deadBodyPositions;
                            RoleClass.MadSeer.deadBodyPositions = new List<Vector3>();
                            limitSoulDuration = RoleClass.MadSeer.limitSoulDuration;
                            soulDuration = RoleClass.MadSeer.soulDuration;
                            if (RoleClass.MadSeer.mode is not 0 and not 2) return;
                            break;
                        case RoleId.EvilSeer:
                            DeadBodyPositions = RoleClass.EvilSeer.deadBodyPositions;
                            RoleClass.EvilSeer.deadBodyPositions = new List<Vector3>();
                            limitSoulDuration = RoleClass.EvilSeer.limitSoulDuration;
                            soulDuration = RoleClass.EvilSeer.soulDuration;
                            if (RoleClass.EvilSeer.mode is not 0 and not 2) return;
                            break;
                        case RoleId.SeerFriends:
                            DeadBodyPositions = RoleClass.SeerFriends.deadBodyPositions;
                            RoleClass.SeerFriends.deadBodyPositions = new List<Vector3>();
                            limitSoulDuration = RoleClass.SeerFriends.limitSoulDuration;
                            soulDuration = RoleClass.SeerFriends.soulDuration;
                            if (RoleClass.SeerFriends.mode is not 0 and not 2) return;//
                            break;
                        case RoleId.JackalSeer:
                        case RoleId.SidekickSeer:
                            DeadBodyPositions = RoleClass.JackalSeer.deadBodyPositions;
                            RoleClass.JackalSeer.deadBodyPositions = new List<Vector3>();
                            limitSoulDuration = RoleClass.JackalSeer.limitSoulDuration;
                            soulDuration = RoleClass.JackalSeer.soulDuration;
                            if (RoleClass.JackalSeer.mode is not 0 and not 2) return;
                            break;
                    }
                    foreach (Vector3 pos in DeadBodyPositions)
                    {
                        GameObject soul = new();
                        soul.transform.position = pos;
                        soul.layer = 5;
                        var rend = soul.AddComponent<SpriteRenderer>();
                        rend.sprite = getSoulSprite();

                        if (limitSoulDuration)
                        {
                            FastDestroyableSingleton<HudManager>.Instance.StartCoroutine(Effects.Lerp(soulDuration, new Action<float>((p) =>
                            {
                                if (rend != null)
                                {
                                    var tmp = rend.color;
                                    tmp.a = Mathf.Clamp01(1 - p);
                                    rend.color = tmp;
                                }
                                if (p == 1f && rend != null && rend.gameObject != null) UnityEngine.Object.Destroy(rend.gameObject);
                            })));
                        }
                    }
                }
            }

            public static class MurderPlayerPatch
            {
                public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
                {
                    var role = PlayerControl.LocalPlayer.getRole();
                    if (role is RoleId.Seer or RoleId.MadSeer or RoleId.EvilSeer or RoleId.SeerFriends or RoleId.JackalSeer or RoleId.SidekickSeer)
                    {
                        bool ModeFlag = false;
                        switch (role)
                        {
                            case RoleId.Seer:
                                if (RoleClass.Seer.deadBodyPositions != null) RoleClass.Seer.deadBodyPositions.Add(target.transform.position);
                                ModeFlag = RoleClass.Seer.mode <= 1;
                                break;
                            case RoleId.MadSeer:
                                if (RoleClass.MadSeer.deadBodyPositions != null) RoleClass.MadSeer.deadBodyPositions.Add(target.transform.position);
                                ModeFlag = RoleClass.MadSeer.mode <= 1;
                                break;
                            case RoleId.EvilSeer:
                                if (RoleClass.EvilSeer.deadBodyPositions != null) RoleClass.EvilSeer.deadBodyPositions.Add(target.transform.position);
                                ModeFlag = RoleClass.MadSeer.mode <= 1;
                                break;
                            case RoleId.SeerFriends:
                                if (RoleClass.SeerFriends.deadBodyPositions != null) RoleClass.SeerFriends.deadBodyPositions.Add(target.transform.position);
                                ModeFlag = RoleClass.SeerFriends.mode <= 1;
                                break;
                            case RoleId.JackalSeer:
                            case RoleId.SidekickSeer:
                                if (RoleClass.JackalSeer.deadBodyPositions != null) RoleClass.JackalSeer.deadBodyPositions.Add(target.transform.position);
                                ModeFlag = RoleClass.JackalSeer.mode <= 1;
                                break;
                        }
                        if (ModeHandler.isMode(ModeId.Default))
                        {
                            if (PlayerControl.LocalPlayer.isAlive() && CachedPlayer.LocalPlayer.PlayerId != target.PlayerId && ModeFlag)
                            {
                                RoleHelpers.ShowFlash(new Color(42f / 255f, 187f / 255f, 245f / 255f));//点滅の制御は[RoleHelpers.ShowFlash]此処は色の制御のみ
                            }
                        }
                        if (PlayerControl.LocalPlayer.IsMod())
                        {
                            if (ModeHandler.isMode(ModeId.SuperHostRoles))
                            {
                                if (PlayerControl.LocalPlayer.isAlive() && CachedPlayer.LocalPlayer.PlayerId != target.PlayerId && ModeFlag)
                                {
                                    //シーアの能力「死の点滅が見える」SHR時の代用で色変更しているコード。
                                    //LateTaskで遅延

                                    RoleClass.Seer.DefaultColor = (byte)CachedPlayer.LocalPlayer.Data.DefaultOutfit.ColorId; //DefaultのColorId(int型)をbyte型に変換しながら取得
                                    var Seer = PlayerControl.LocalPlayer;

                                    SeerDeathFlashSHR.RawSetColorDeathFlashSHR(Seer, 10); //体の色を ColorId 10(SkyBlue) に変更する。
                                    new LateTask(() => { SeerDeathFlashSHR.RawSetColorDeathFlashSHR(Seer, 1); }, 1f, "Blue");// 1s遅延後 LocalPlayerの 体の色を ColorId 0(Blue) に変更する。
                                    new LateTask(() => { SeerDeathFlashSHR.RawSetColorDeathFlashSHR(Seer, 10); }, 2f, "SkyBlue");
                                    new LateTask(() => { SeerDeathFlashSHR.RawSetColorDeathFlashSHR(Seer, RoleClass.Seer.DefaultColor); }, 3f, "DefaultColor");// 3s遅延後 LocalPlayerの 体の色を 元の色 に変更する。
                                }
                            }
                        }
                        if (!PlayerControl.LocalPlayer.IsMod())
                        {
                            if (ModeHandler.isMode(ModeId.SuperHostRoles))
                            {
                                if (PlayerControl.LocalPlayer.isAlive() && CachedPlayer.LocalPlayer.PlayerId != target.PlayerId && ModeFlag)
                                {
                                    //シーアの能力「死の点滅が見える」SHR時の代用で色変更しているコード。
                                    //LateTaskで遅延

                                    RoleClass.Seer.DefaultColor = (byte)CachedPlayer.LocalPlayer.Data.DefaultOutfit.ColorId; //DefaultのColorId(int型)をbyte型に変換しながら取得
                                    var SeerID = CachedPlayer.LocalPlayer.PlayerId;


                                    RPCHelper.RpcSetColorDesync(SeerID, 10); //体の色を ColorId 10(SkyBlue) に変更する。
                                    new LateTask(() => { RPCHelper.RpcSetColorDesync(SeerID, 1); }, 1f, "Blue");// 1s遅延後 LocalPlayerの 体の色を ColorId 0(Blue) に変更する。
                                    new LateTask(() => { RPCHelper.RpcSetColorDesync(SeerID, 10); }, 2f, "SkyBlue");
                                    new LateTask(() => { RPCHelper.RpcSetColorDesync(SeerID, RoleClass.Seer.DefaultColor); }, 3f, "DefaultColor");// 3s遅延後 LocalPlayerの 体の色を 元の色 に変更する。
                                }
                            }
                        }
                    }
                }
            }
            //Mode_0_死の点滅＆幽霊が見える
            //Mode_1_死の点滅が見える
            //Mode_2_幽霊が見える
        }
    }
    public static class SeerDeathFlashSHR
    {
        public static void RawSetColorDeathFlashSHR(this PlayerControl player, byte color)
        {//シーアの能力「死の点滅が見える」SHR時の代用で身体の色変更を制御しているコード
            player.RawSetColor(color);
        }
    }
}

