﻿using System;
using System.Collections.Generic;
using App.Model;
using App.Model.Character;
using App.View.Map;
using UnityEngine;

namespace App.Util.Manager
{
    public class BattleTilesManager
    {
        private List<VTile> currentMovingTiles;
        private List<VTile> currentAttackTiles;
        public BattleTilesManager()
        {

        }
        public void ShowCharacterMovingArea(MCharacter mCharacter, int movingPower = 0)
        {
            currentMovingTiles = Global.battleManager.cBattle.breadthFirst.Search(mCharacter, movingPower, true);
            Global.battleEvent.DispatchEventMovingTiles(currentMovingTiles, mCharacter.belong);
            Controller.Battle.CBattlePanel cBattle = Global.battleManager.cBattle;
            cBattle.battleMode = BattleMode.show_move_tiles;
        }

        public void ShowCharacterSkillArea(MCharacter mCharacter)
        {
            //技能攻击扩展范围
            List<int[]> distances = mCharacter.skillDistances;
            distances.Add(mCharacter.currentSkill == null ? new int[] { 0, 0 } : mCharacter.currentSkill.master.distance);
            int maxDistance = 0;
            foreach (int[] distance in distances)
            {
                if (distance[1] > maxDistance)
                {
                    maxDistance = distance[1];
                }
            }
            currentAttackTiles = Global.battleManager.cBattle.breadthFirst.Search(mCharacter, maxDistance);
            VTile characterTile = currentAttackTiles.Find(v => v.coordinate.Equals(mCharacter.coordinate));
            currentAttackTiles = currentAttackTiles.FindAll((tile) => {
                int length = Global.battleManager.cBattle.mapSearch.GetDistance(tile, characterTile);
                return distances.Exists(d => length >= d[0] && length <= d[1]);
            });
            if (mCharacter.currentSkill == null)
            {
                return;
            }
            Global.battleEvent.DispatchEventAttackTiles(currentAttackTiles, mCharacter.belong);
            if (mCharacter.belong == Belong.self && !mCharacter.actionOver)
            {
                ShowCharacterSkillTween(mCharacter, currentAttackTiles);
            }
        }
        public void ShowCharacterSkillTween(MCharacter mCharacter, List<VTile> tiles)
        {
            foreach (VTile tile in tiles)
            {
                if (tile.isAttackTween)
                {
                    continue;
                }
                MCharacter character = Global.battleManager.charactersManager.GetCharacter(tile.coordinate, Global.battleManager.cBattle.characters);
                if (character == null || character.hp == 0 || character.isHide)
                {
                    continue;
                }
                bool sameBelong = Global.battleManager.charactersManager.IsSameBelong(character.belong, mCharacter.belong);
                bool useToEnemy = mCharacter.currentSkill.useToEnemy;
                //TODO:
                if (useToEnemy ^ sameBelong)
                {
                    tile.ShowAttackIcon();
                }
            }
        }

    }
}
