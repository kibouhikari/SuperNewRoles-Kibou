using System;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using SuperNewRoles.Buttons;
using SuperNewRoles.CustomRPC;
using UnityEngine;

namespace SuperNewRoles.Roles
{
    class TeleportingJackal
    {
        public static void resetCoolDown()
        {
            HudManagerStartPatch.JackalKillButton.MaxTimer = RoleClass.TeleportingJackal.KillCoolDown;
            HudManagerStartPatch.JackalKillButton.Timer = RoleClass.TeleportingJackal.KillCoolDown;
        }
        public static void EndMeeting()
        {
            resetCoolDown();
            HudManagerStartPatch.SheriffKillButton.MaxTimer = RoleClass.TeleportingJackal.CoolTime;
            RoleClass.TeleportingJackal.ButtonTimer = DateTime.Now;
        }
        public static void setPlayerOutline(PlayerControl target, Color color)
        {
            if (target == null || target.MyRend == null) return;

            target.MyRend().material.SetFloat("_Outline", 1f);
            target.MyRend().material.SetColor("_OutlineColor", color);
        }
        public class JackalFixedPatch
        {
            public static PlayerControl TeleportingJackalsetTarget(bool onlyCrewmates = false, bool targetPlayersInVents = false, List<PlayerControl> untargetablePlayers = null, PlayerControl targetingPlayer = null)
            {
                PlayerControl result = null;
                float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
                if (!MapUtilities.CachedShipStatus) return result;
                if (targetingPlayer == null) targetingPlayer = PlayerControl.LocalPlayer;
                if (targetingPlayer.Data.IsDead || targetingPlayer.inVent) return result;

                if (untargetablePlayers == null)
                {
                    untargetablePlayers = new();
                }

                Vector2 truePosition = targetingPlayer.GetTruePosition();
                Il2CppSystem.Collections.Generic.List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
                for (int i = 0; i < allPlayers.Count; i++)
                {
                    GameData.PlayerInfo playerInfo = allPlayers[i];
                    //下記TeleportingJackalがbuttonのターゲットにできない役職の設定
                    if (playerInfo.Object.isAlive() && playerInfo.PlayerId != targetingPlayer.PlayerId && !playerInfo.Object.IsJackalTeamJackal() && !playerInfo.Object.IsJackalTeamSidekick())
                    {
                        PlayerControl @object = playerInfo.Object;
                        if (untargetablePlayers.Any(x => x == @object))
                        {
                            // if that player is not targetable: skip check
                            continue;
                        }

                        if (@object && (!@object.inVent || targetPlayersInVents))
                        {
                            Vector2 vector = @object.GetTruePosition() - truePosition;
                            float magnitude = vector.magnitude;
                            if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                            {
                                result = @object;
                                num = magnitude;
                            }
                        }
                    }
                }
                return result;
            }
            static void TeleportingJackalPlayerOutLineTarget()
            {
                setPlayerOutline(TeleportingJackalsetTarget(), RoleClass.TeleportingJackal.color);
            }
            public static void Postfix(PlayerControl __instance)
            {
                if (PlayerControl.LocalPlayer.isRole(RoleId.TeleportingJackal))
                {
                    TeleportingJackalPlayerOutLineTarget();
                }
            }
        }
        public static void ResetCoolDown()
        {
            HudManagerStartPatch.TeleporterButton.MaxTimer = RoleClass.TeleportingJackal.CoolTime;
            RoleClass.TeleportingJackal.ButtonTimer = DateTime.Now;
        }
        public static void TeleportStart()
        {
            List<PlayerControl> aliveplayers = new();
            foreach (PlayerControl p in CachedPlayer.AllPlayers)
            {
                if (p.isAlive() && p.CanMove)
                {
                    aliveplayers.Add(p);
                }
            }
            var player = ModHelpers.GetRandom<PlayerControl>(aliveplayers);
            CustomRPC.RPCProcedure.TeleporterTP(player.PlayerId);
            MessageWriter Writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.CustomRPC.TeleporterTP, Hazel.SendOption.Reliable, -1);
            Writer.Write(player.PlayerId);
            AmongUsClient.Instance.FinishRpcImmediately(Writer);
        }
        public static bool IsTeleportingJackal(PlayerControl Player)
        {
            if (Player.isRole(RoleId.TeleportingJackal))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
