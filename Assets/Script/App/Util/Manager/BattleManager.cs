﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using App.Controller.Battle;
using App.Model;
using App.Model.Character;
using App.Util.Cacher;
using App.Util.Event;
using App.Util.LSharp;
using App.Util.Search;
using App.View.Avatar;
using App.View.Map;
using Holoville.HOTween;
using UnityEngine;

namespace App.Util.Manager
{
    public class BattleManager
    {
        public Belong currentBelong { get; set; }
        public BattleMode battleMode { get; set; }
        public BattleTilesManager tilesManager { get; set; }
        public BattleCalculateManager calculateManager { get; set; }
        public AIManager aiManager { get; set; }
        public BreadthFirst breadthFirst { get; set; }
        private MCharacter currentMCharacter;
        private VCharacterBase currentVCharacter;
        private System.Action returnAction;
        private List<VCharacterBase> actionCharacterList = new List<VCharacterBase>();
        private Vector2Int _oldCoordinate = new Vector2Int();
        public Vector2Int oldCoordinate
        {
            get {
                return _oldCoordinate;
            }
        }
        public bool characterIsRunning
        {
            get
            {
                return currentMCharacter != null;
            }
        }
        public BattleManager()
        {
            tilesManager = new BattleTilesManager();
            calculateManager = new BattleCalculateManager();
            aiManager = new AIManager();
            breadthFirst = new BreadthFirst();
        }
        public void Init()
        {
            Global.charactersManager.mCharacters.Clear();
            Global.charactersManager.vCharacters.Clear();

        }
        public void ClickNoneNode(Vector2Int coordinate)
        {
            MCharacter mCharacter = Global.charactersManager.GetCharacter(coordinate);
            if (mCharacter != null)
            {
                currentVCharacter = Global.charactersManager.GetVCharacter(mCharacter);
                this.currentMCharacter = mCharacter;
                this.currentMCharacter.roadLength = 0;
                tilesManager.ShowCharacterMovingArea(mCharacter);
                tilesManager.ShowCharacterSkillArea(mCharacter);
                Global.battleEvent.DispatchEventCharacterPreview(mCharacter);
                _oldCoordinate.x = mCharacter.coordinate.x;
                _oldCoordinate.y = mCharacter.coordinate.y;
                ActionType action = currentVCharacter.action;
                float x = currentVCharacter.X;
                Direction direction = currentVCharacter.direction;
                if (mCharacter.belong == Belong.self)
                {
                    Global.battleEvent.DispatchEventOperatingMenu(true);
                }
                returnAction = () =>
                {
                    this.currentMCharacter.coordinate.y = _oldCoordinate.y;
                    this.currentMCharacter.coordinate.x = _oldCoordinate.x;
                    currentVCharacter.X = x;
                    currentVCharacter.direction = direction;
                    currentVCharacter.action = action;
                };
            }
        }
        public void ClickMovingNode(Vector2Int coordinate)
        {
            if (this.currentMCharacter.belong != currentBelong || this.currentMCharacter.actionOver)
            {
                CharacterReturnNone();
                return;
            }
            MCharacter mCharacter = Global.charactersManager.GetCharacter(coordinate);
            if (mCharacter != null)
            {
                bool sameBelong = Global.charactersManager.IsSameBelong(mCharacter.belong, this.currentMCharacter.belong);
                bool useToEnemy = this.currentMCharacter.currentSkill.useToEnemy;
                if (useToEnemy ^ sameBelong)
                {
                    ClickSkillNode(coordinate);
                }
                return;
            }
            if (this.tilesManager.IsInMovingCurrentTiles(coordinate))
            {
                MoveStart(coordinate);
            }
            else if (battleMode != Model.BattleMode.move_after_attack)
            {
                CharacterReturnNone();
            }
        }
        private void MoveStart(Vector2Int coordinate)
        {
            VTile startTile = Global.mapSearch.GetTile(this.currentMCharacter.coordinate);
            VTile endTile = Global.mapSearch.GetTile(coordinate);
            //cBattlefield.MapMoveToPosition(this.mCharacter.CoordinateX, this.mCharacter.CoordinateY);
            Holoville.HOTween.Core.TweenDelegate.TweenCallback moveComplete;
            if (battleMode == Model.BattleMode.move_after_attack)
            {
                moveComplete = () =>
                {
                    this.currentMCharacter.coordinate.y = endTile.coordinate.y;
                    this.currentMCharacter.coordinate.x = endTile.coordinate.x;
                    //cBattle.MapMoveToPosition(this.mCharacter.CoordinateX, this.mCharacter.CoordinateY);
                    App.Util.AppManager.CurrentScene.StartCoroutine(ActionOverNext());
                };
            }
            else
            {
                moveComplete = () =>
                {
                    currentVCharacter.action = Model.ActionType.idle;
                    this.tilesManager.ClearCurrentTiles();
                    battleMode = Model.BattleMode.move_end;
                    this.currentMCharacter.coordinate.y = endTile.coordinate.y;
                    this.currentMCharacter.coordinate.x = endTile.coordinate.x;
                    /*
                    cBattlefield.MapMoveToPosition(this.mCharacter.CoordinateX, this.mCharacter.CoordinateY);
                    */
                    this.tilesManager.ShowCharacterSkillArea(this.currentMCharacter);
                    Global.battleEvent.DispatchEventOperatingMenu(true);
                };
            }
            List<VTile> tiles = Global.aStar.Search(currentMCharacter, startTile, endTile);
            this.currentMCharacter.roadLength = tiles.Count;
            if (tiles.Count > 0)
            {
                Global.battleEvent.DispatchEventOperatingMenu(false);
                this.tilesManager.ClearCurrentTiles();
                currentVCharacter.action = Model.ActionType.move;
                battleMode = Model.BattleMode.moving;
                Sequence sequence = new Sequence();
                foreach (VTile tile in tiles)
                {
                    TweenParms tweenParms = new TweenParms()
                        .Prop("X", tile.transform.localPosition.x, false)
                        .Prop("Y", tile.transform.localPosition.y, false)
                        .Ease(EaseType.Linear);
                    if (tile.coordinate.Equals(endTile.coordinate))
                    {
                        tweenParms.OnComplete(moveComplete);
                    }
                    sequence.Append(HOTween.To(currentVCharacter, 0.5f, tweenParms));
                }
                sequence.Play();
            }
            else
            {
                moveComplete();
            }
        }
        public void ClickSkillNode(Vector2Int coordinate)
        {
            MCharacter mCharacter = Global.charactersManager.GetCharacter(coordinate);
            if (mCharacter == null || !Global.charactersManager.IsInSkillDistance(mCharacter, currentMCharacter))
            {
                CharacterReturnNone();
                return;
            }
            bool sameBelong = Global.charactersManager.IsSameBelong(mCharacter.belong, currentMCharacter.belong);
            bool useToEnemy = currentMCharacter.currentSkill.useToEnemy;
            if (!(useToEnemy ^ sameBelong))
            {
                Controller.Dialog.CAlertDialog.Show("belong不对");
                return;
            }
            currentMCharacter.target = mCharacter;
            mCharacter.target = currentMCharacter;

            VCharacterBase vCharacter = Global.charactersManager.GetVCharacter(mCharacter);

            if (useToEnemy)
            {
                bool forceFirst = (mCharacter.currentSkill != null && mCharacter.currentSkill.master.effect.special == Model.SkillEffectSpecial.force_first);
                if (forceFirst && Global.charactersManager.IsInSkillDistance(currentMCharacter, mCharacter))
                {
                    //先手攻击
                    SetActionCharacterList(vCharacter, currentVCharacter, true);
                }
                else
                {
                    SetActionCharacterList(currentVCharacter, vCharacter, true);
                }
            }
            else
            {
                SetActionCharacterList(currentVCharacter, vCharacter, false);
            }
            this.tilesManager.ClearCurrentTiles();
            Global.battleEvent.DispatchEventOperatingMenu(false);
            Global.battleEvent.DispatchEventCharacterPreview(null);
            battleMode = Model.BattleMode.actioning;
            Global.battleEvent.ActionEndHandler += OnActionComplete;
            OnActionComplete();
        }
        private void SetActionCharacterList(VCharacterBase actionCharacter, VCharacterBase targetCharacter, bool canCounter)
        {
            int count = this.calculateManager.SkillCount(actionCharacter.mCharacter, targetCharacter.mCharacter);
            int countBack = count;
            while (count-- > 0)
            {
                actionCharacterList.Add(actionCharacter);
            }
            if (!canCounter || !this.calculateManager.CanCounterAttack(actionCharacter.mCharacter, targetCharacter.mCharacter, actionCharacter.mCharacter.coordinate, targetCharacter.mCharacter.coordinate))
            {
                return;
            }
            count = this.calculateManager.CounterAttackCount(actionCharacter.mCharacter, targetCharacter.mCharacter);
            while (count-- > 0)
            {
                actionCharacterList.Add(targetCharacter);
            }
            //反击后反击
            if (actionCharacter.mCharacter.currentSkill.master.effect.special == Model.SkillEffectSpecial.attack_back_attack)
            {
                while (countBack-- > 0)
                {
                    actionCharacterList.Add(actionCharacter);
                }
            }
        }
        /// <summary>
        /// 动作结束时，判断是否继续进行
        /// </summary>
        public void OnActionComplete()
        {
            Debug.LogError("OnActionComplete");
            if (actionCharacterList.Count > 0)
            {
                VCharacterBase vCharacter = actionCharacterList[0];
                actionCharacterList.RemoveAt(0);
                bool isContinue = ActionStart(vCharacter);
                if (isContinue)
                {
                    return;
                }
            }
            Global.battleEvent.ActionEndHandler -= OnActionComplete;
            AppManager.CurrentScene.StartCoroutine(ActionOver());
        }
        /// <summary>
        /// 开始动作
        /// </summary>
        /// <returns><c>true</c>, if start was actioned, <c>false</c> otherwise.</returns>
        /// <param name="vCharacter">Current character.</param>
        private bool ActionStart(VCharacterBase vCharacter){
            Debug.LogError("ActionStart");
            if (vCharacter.mCharacter.hp > 0)
            {
                //目标已死
                if (vCharacter.mCharacter.target.hp == 0)
                {
                    return false;
                }
                ActionStartRun(vCharacter);
                return true;
            }
            actionCharacterList.Clear();
            if (Global.charactersManager.IsSameCharacter(vCharacter.mCharacter, currentMCharacter))
            {
                return true;
            }
            //是否引导攻击
            bool continueAttack = (currentMCharacter.currentSkill.master.effect.special == Model.SkillEffectSpecial.continue_attack);
            if (continueAttack)
            {
                VTile vTile = Global.mapSearch.GetTile(currentMCharacter.coordinate);
                MCharacter mCharacter = Global.charactersManager.mCharacters.Find((c) => {
                    if (c.hp == 0)
                    {
                        return false;
                    }
                    if (Global.charactersManager.IsSameBelong(currentMCharacter.belong, c.belong))
                    {
                        return false;
                    }
                    bool canAttack = Global.charactersManager.IsInSkillDistance(c.coordinate, vTile.coordinate, currentMCharacter);
                    return canAttack;
                });
                if (mCharacter != null)
                {
                    Global.battleEvent.ActionEndHandler -= OnActionComplete;
                    VTile tile = Global.mapSearch.GetTile(mCharacter.coordinate);
                    ClickSkillNode(tile.coordinate);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 开始动作
        /// </summary>
        /// <returns><c>true</c>, if start run was actioned, <c>false</c> otherwise.</returns>
        /// <param name="vCharacter">Current character.</param>
        private void ActionStartRun(VCharacterBase vCharacter)
        {
            Debug.LogError("ActionStartRun");
            System.Action actionStartEvent = () =>
            {
                //cBattlefield.MapMoveToPosition(currentCharacter.CoordinateX, currentCharacter.CoordinateY);
                //vCharacter.direction = (vCharacter.mCharacter.coordinate.x > vCharacter.mCharacter.target.coordinate.x ? Direction.left : Direction.right);
                vCharacter.direction = Global.mapSearch.GetDirection(vCharacter.mCharacter.coordinate, vCharacter.mCharacter.target.coordinate);
                vCharacter.action = ActionType.attack;
                App.Model.Master.MSkill skillMaster = vCharacter.mCharacter.currentSkill.master;
                if (!string.IsNullOrEmpty(skillMaster.animation))
                {
                    VTile vTile = Global.mapSearch.GetTile(vCharacter.mCharacter.target.coordinate);
                    //this.cBattlefield.CreateEffect(skillMaster.animation, vTile.transform);
                }
            };
            if (vCharacter.mCharacter.currentSkill.master.effect.special == SkillEffectSpecial.back_thrust)
            {
                //回马枪
                VTile currentTile = Global.mapSearch.GetTile(vCharacter.mCharacter.coordinate);
                VTile targetTile = Global.mapSearch.GetTile(vCharacter.mCharacter.target.coordinate);
                Direction direction = Global.mapSearch.GetDirection(targetTile, currentTile);
                VTile backTile = Global.mapSearch.GetTile(currentTile, direction);
                if (Global.charactersManager.GetCharacter(backTile.coordinate) != null)
                {
                    actionStartEvent();
                    return;
                }
                Sequence sequence = new Sequence();
                TweenParms tweenParms = new TweenParms()
                .Prop("X", backTile.transform.localPosition.x, false)
                    .Prop("Y", backTile.transform.localPosition.y, false)
                .Ease(EaseType.Linear);
                TweenParms tweenParmsTarget = new TweenParms()
                .Prop("X", currentTile.transform.localPosition.x, false)
                    .Prop("Y", currentTile.transform.localPosition.y, false)
                .Ease(EaseType.Linear);
                Holoville.HOTween.Core.TweenDelegate.TweenCallback moveComplete = () =>
                {
                    vCharacter.mCharacter.coordinate.y = backTile.coordinate.y;
                    vCharacter.mCharacter.coordinate.x = backTile.coordinate.x;
                    vCharacter.mCharacter.target.coordinate.y = currentTile.coordinate.y;
                    vCharacter.mCharacter.target.coordinate.x = currentTile.coordinate.x;
                    actionStartEvent();
                };
                tweenParms.OnComplete(moveComplete);
                VCharacterBase vTarget = Global.charactersManager.GetVCharacter(vCharacter.mCharacter.target);
                sequence.Insert(0f, HOTween.To(vCharacter, 0.5f, tweenParms));
                sequence.Insert(0f, HOTween.To(vTarget, 0.5f, tweenParmsTarget));
                sequence.Play();
            }
            else
            {
                actionStartEvent();
            }
        }
        /// <summary>
        /// 动作结束后处理
        /// </summary>
        public IEnumerator ActionOver(){
            Debug.LogError("ActionOver");
            if (currentMCharacter.target != null)
            {
                if (currentMCharacter.target.hp > 0 && currentMCharacter.target.attackEndEffects.Count > 0)
                {
                    foreach (App.Model.Master.MSkillEffect mSkillEffect in currentMCharacter.target.attackEndEffects)
                    {
                        AddAidToCharacter(mSkillEffect, new MCharacter[] { currentMCharacter.target });
                    }
                    /*while (VEffectAnimation.IsRunning)
                    {
                        yield return new WaitForEndOfFrame();
                    }*/
                    yield return new WaitForEndOfFrame();
                    currentMCharacter.target.attackEndEffects.Clear();
                }
                currentMCharacter.target.target = null;
                currentMCharacter.target = null;

            }
            if (!Global.charactersManager.mCharacters.Exists(c => c.hp > 0 && !c.isHide && c.belong == Belong.enemy))
            {
                //敌军全灭
                Debug.LogError("敌军全灭");
                List<string> script = new List<string>();
                script.Add("Call.battle_win();");
                script.Add("Battle.win();");
                LSharpScript.Instance.Analysis(script);
                yield break;
            }
            else if (!Global.charactersManager.mCharacters.Exists(c => c.hp > 0 && !c.isHide && c.belong == Belong.self))
            {
                //我军全灭
                Debug.LogError("我军全灭");
                //cBattle.StartCoroutine(Global.AppManager.ShowDialog(Prefabs.BattleFailDialog));
                yield break;
            }
            if (currentMCharacter.hp > 0 && currentMCharacter.isMoveAfterAttack 
            && currentMCharacter.ability.movingPower - currentMCharacter.roadLength > 0)
            {
                tilesManager.ShowCharacterMovingArea(currentMCharacter, currentMCharacter.ability.movingPower - currentMCharacter.roadLength);
                battleMode = BattleMode.move_after_attack;
                if (currentMCharacter.belong != Belong.self)
                {
                    //cBattle.ai.MoveAfterAttack();
                }
            }
            else
            {
                Debug.LogError("ActionOverNext");
                App.Util.AppManager.CurrentScene.StartCoroutine(ActionOverNext());
            }
        }
        /// <summary>
        /// 特技导致武将状态改变
        /// </summary>
        /// <param name="skill">Skill.</param>
        private void AddAidToCharacter(App.Model.Master.MSkillEffect mSkillEffect, MCharacter[] targetCharacters)
        {
            List<int> aids = mSkillEffect.strategys.ToList();
            int index = 0;
            List<App.Model.Master.MStrategy> strategys = new List<App.Model.Master.MStrategy>();
            while (index++ < mSkillEffect.count)
            {
                int i = UnityEngine.Random.Range(0, aids.Count - 1);
                App.Model.Master.MStrategy strategy = StrategyCacher.Instance.Get(aids[i]);
                aids.RemoveAt(i);
                strategys.Add(strategy);
            }
            foreach (App.Model.Master.MStrategy strategy in strategys)
            {
                foreach (MCharacter target in targetCharacters)
                {
                    //特效
                    if (strategy.effect_type == StrategyEffectType.aid)
                    {
                        VTile vTile = Global.mapSearch.GetTile(target.coordinate);
                        //cBattle.CreateEffect(strategy.effect, vTile.transform);
                    }
                    else if (strategy.effect_type ==StrategyEffectType.status)
                    {
                        VTile vTile = Global.mapSearch.GetTile(target.coordinate);
                        //cBattle.CreateEffect(strategy.effect, vTile.transform);
                    }
                }
            }
            AppManager.CurrentScene.StartCoroutine(AddAidToCharacterComplete(strategys, targetCharacters));
        }
        private IEnumerator AddAidToCharacterComplete(List<App.Model.Master.MStrategy> strategys, MCharacter[] targetCharacters)
        {
            /*while (VEffectAnimation.IsRunning)
            {
                yield return new WaitForEndOfFrame();
            }*/
            yield return 0;
            foreach (App.Model.Master.MStrategy strategy in strategys)
            {
                foreach (MCharacter target in targetCharacters)
                {
                    if (strategy.effect_type == StrategyEffectType.aid)
                    {
                        //target.AddAid(strategy);
                    }
                    else if (strategy.effect_type == StrategyEffectType.status)
                    {
                        //target.AddStatus(strategy);
                    }
                }
            }
        }

        /// <summary>
        /// 动作结束后处理
        /// </summary>
        public IEnumerator ActionOverNext()
        {
            if (battleMode == BattleMode.moving)
            {
                currentVCharacter.action = ActionType.idle;
            }
            yield return AppManager.CurrentScene.StartCoroutine(ActionEndSkillsRun());
            currentVCharacter.actionOver = true;
            currentMCharacter.roadLength = 0;
            tilesManager.ClearCurrentTiles();
            Global.battleEvent.DispatchEventCharacterPreview(null);
            battleMode = BattleMode.none;
            Belong belong = currentMCharacter.belong;
            this.currentMCharacter = null;
            this.currentVCharacter = null;
            if (!Global.charactersManager.mCharacters.Exists(c => c.hp > 0 && !c.isHide && c.belong == belong && !c.actionOver))
            {
                ChangeBelong(belong);
            }
            else
            {
                Global.battleEvent.DispatchEventOperatingMenu(false);
            }
        }
        public void ChangeBelong(Belong belong)
        {
            if (belong == Belong.self)
            {
                if (Global.charactersManager.mCharacters.Exists(c => c.hp > 0 && !c.isHide && c.belong == Belong.friend && !c.actionOver))
                {
                    Global.battleEvent.DispatchEventBelongChange(Belong.friend);
                }
                else
                {
                    Global.battleEvent.DispatchEventBelongChange(Belong.enemy);
                }
            }
            else if (belong == Belong.friend)
            {
                Global.battleEvent.DispatchEventBelongChange(Belong.enemy);
            }
            else if (belong == Belong.enemy)
            {
                Global.battleEvent.DispatchEventBelongChange(Belong.self);
            }
        }
        private IEnumerator ActionEndSkillsRun() {
            yield break;
        }
        public void CharacterReturnNone()
        {
            returnAction();
            this.currentMCharacter = null;
            this.tilesManager.ClearCurrentTiles();
            Global.battleEvent.DispatchEventOperatingMenu(false);
            Global.battleEvent.DispatchEventCharacterPreview(null);
            battleMode = Model.BattleMode.none;
        }

    }
}
