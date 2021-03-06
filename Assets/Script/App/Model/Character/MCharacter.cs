﻿using System;
using System.Collections.Generic;
using App.Model.Common;
using App.Model.Equipment;
using App.Util.Cacher;
using App.Util.Manager;
using JsonFx;

namespace App.Model.Character
{
    public class MCharacter: MBase
    {
        #region 服务器数据
        [JsonName(Name = "character_id")]
        public int characterId;
        public int fragment;
        #endregion

        #region 存档数据
        public int level
        {
            get
            {
                return fileCharacter.level;
            }
        }
        public int exp
        {
            get
            {
                return fileCharacter.exp;
            }
        }
        public MSkill[] skills
        {
            get
            {
                if((fileCharacter.skills == null || fileCharacter.skills.Length == 0) && master.skills.Length > 0) {
                    List<MSkill> skillList = new List<MSkill>();
                    Array.ForEach(master.skills, (skill) => {
                        MSkill mSkill = new MSkill();
                        mSkill.skillId = skill.skillId;
                        mSkill.level = 1;
                        skillList.Add(mSkill);

                    });
                    fileCharacter.skills = skillList.ToArray();
                }
                return fileCharacter.skills;
            }
            set {
                fileCharacter.skills = value;
            }
        }
        #endregion
        MEquipment _equipmentWepon;
        public MEquipment equipmentWepon
        {
            get
            {
                if (_equipmentWepon == null) {
                    _equipmentWepon = CacherBase<UserEquipmentCacher, MBase>.Instance.GetEquipment(weapon, EquipmentType.weapon, master.weapon) as MEquipment;
                }
                return _equipmentWepon;
            }
        }
        public int weapon
        {
            get
            {
                return fileCharacter.weapon;
            }
            set
            {
                fileCharacter.weapon = value;
                _equipmentWepon = null;
            }
        }
        public WeaponType weaponType
        {
            get
            {
                return equipmentWepon.weaponType;
            }
        }
        MEquipment _equipmentHorse;
        public MEquipment equipmentHorse
        {
            get
            {
                if (_equipmentHorse == null)
                {
                    _equipmentHorse = CacherBase<UserEquipmentCacher, MBase>.Instance.GetEquipment(horse, EquipmentType.horse, master.horse) as MEquipment;
                }
                return _equipmentHorse;
            }
        }
        public int horse
        {
            get
            {
                return fileCharacter.horse;
            }
            set
            {
                fileCharacter.horse = value;
                _equipmentHorse = null;
            }
        }
        public MoveType moveType
        {
            get
            {
                return equipmentHorse.moveType;
            }
        }
        MEquipment _equipmentClothes;
        public MEquipment equipmentClothes
        {
            get
            {
                if (_equipmentClothes == null)
                {
                    _equipmentClothes = CacherBase<UserEquipmentCacher, MBase>.Instance.GetEquipment(clothes, EquipmentType.clothes, master.clothes) as MEquipment;
                }
                return _equipmentClothes;
            }
        }
        public int clothes
        {
            get
            {
                return fileCharacter.clothes;
            }
            set
            {
                fileCharacter.clothes = value;
                _equipmentClothes = null;
            }
        }

        public Mission mission;
        public int isSelected;
        public MCharacter target;
        public int head
        {
            get
            {
                return master.head;
            }
        }
        public int hat
        {
            get
            {
                return master.hat;
            }
        }
        public string name
        {
            get
            {
                return master.name;
            }
        }
        public int staticAvatar
        {
            get {
                return master.staticAvatar;
            }
        }
        public int movingPower{
            get{
                return ability.movingPower;
            }
        }
        public int star;
        public int hp;
        public int mp;
        public int roadLength;
        public Belong belong;
        public ActionType action;
        public MSkill currentSkill;
        public MCharacterAbility ability;
        public bool boutEventComplete;
        public bool isHide = false;
        public bool actionOver;
        public UnityEngine.Vector2Int coordinate = new UnityEngine.Vector2Int(0, 0);
        public MCharacter()
        {
            this.mission = Mission.initiative;
        }
        public static MCharacter Create(Master.MNpc npc)
        {
            MCharacter mCharacter = new MCharacter();
            mCharacter.id = npc.id;
            mCharacter.characterId = npc.character_id;
            mCharacter.horse = npc.horse;
            mCharacter.clothes = npc.clothes;
            mCharacter.weapon = npc.weapon;
            mCharacter.star = npc.star;
            mCharacter.action = ActionType.idle;
            return mCharacter;
        }
        public MCharacter Clone()
        {
            MCharacter mCharacter = new MCharacter();
            mCharacter.characterId = characterId;
            return mCharacter;
        }
        public void StatusInit()
        {
            if (this.currentSkill == null)
            {
                if (this.skills != null && this.skills.Length > 0)
                {
                    this.currentSkill = Array.Find(this.skills, s => Master.MSkill.IsWeaponType(s.master, this.weaponType));
                }
            }
            if (this.ability == null)
            {
                this.ability = MCharacterAbility.Create(this);
            }
            else
            {
                this.ability.Update(this);
            }
            this.hp = this.ability.hpMax;
            this.mp = this.ability.mpMax;
            this._status = new List<MBase>();
        }
        public int physicalAttack{
            get{
                return ability.physicalAttack;
            }
        }
        public int magicAttack
        {
            get
            {
                return ability.magicAttack;
            }
        }
        public int physicalDefense
        {
            get
            {
                return ability.physicalDefense;
            }
        }
        public int magicDefense
        {
            get
            {
                return ability.magicDefense;
            }
        }
        public Master.MSkill boutFixedDamageSkill
        {
            get
            {
                foreach (MSkill skill in skills)
                {
                    Master.MSkill mSkill = skill.master;
                    if (mSkill.effect.special != SkillEffectSpecial.bout_fixed_damage)
                    {
                        continue;
                    }
                    return mSkill;
                }
                return null;
            }
        }
        public List<int[]> skillDistances
        {
            get{
                List<int[]> arr = new List<int[]>();
                Array.ForEach(this.skills, (skill)=> {
                    Master.MSkill skillMaster = skill.master;
                    if (skillMaster.effect.special != SkillEffectSpecial.attack_distance)
                    {
                        return;
                    }
                    arr.Add(skillMaster.distance);
                });
                return arr;
            }
        }
        public bool canHeal
        {
            get
            {
                return Array.Exists(skills, s => Master.MSkill.IsSkillType(s.master, SkillType.heal));
            }
        }
        /// <summary>
        /// 是否残血
        /// </summary>
        /// <value><c>true</c> if this instance is pant; otherwise, <c>false</c>.</value>
        public bool isPant
        {
            get
            {
                return hp / ability.hpMax < Util.Global.Constant.is_pant;
            }
        }
        /// <summary>
        /// 枪剑类兵器
        /// </summary>
        /// <value><c>true</c> if this instance is pike; otherwise, <c>false</c>.</value>
        public bool isPike
        {
            get
            {
                return WeaponManager.IsPike(weaponType);
            }
        }
        /// <summary>
        /// 斧类兵器
        /// </summary>
        /// <value><c>true</c> if this instance is ax; otherwise, <c>false</c>.</value>
        public bool isAx
        {
            get
            {
                return WeaponManager.IsAx(weaponType);
            }
        }
        /// <summary>
        /// 刀类兵器
        /// </summary>
        /// <value><c>true</c> if this instance is knife; otherwise, <c>false</c>.</value>
        public bool isKnife
        {
            get
            {
                return WeaponManager.IsKnife(weaponType);
            }
        }
        /// <summary>
        /// 长兵器
        /// </summary>
        /// <value><c>true</c> if this instance is long weapon; otherwise, <c>false</c>.</value>
        public bool isLongWeapon
        {
            get
            {
                return WeaponManager.IsLongWeapon(weaponType);
            }
        }
        /// <summary>
        /// 短兵器
        /// </summary>
        /// <value><c>true</c> if this instance is short weapon; otherwise, <c>false</c>.</value>
        public bool isShortWeapon
        {
            get
            {
                return WeaponManager.IsShortWeapon(weaponType);
            }
        }
        /// <summary>
        /// 远程类兵器
        /// </summary>
        /// <value><c>true</c> if this instance is archery; otherwise, <c>false</c>.</value>
        public bool isArcheryWeapon
        {
            get
            {
                return WeaponManager.IsArcheryWeapon(weaponType);
            }
        }


        public float TileAid(View.Map.VTile vTile)
        {
            int aid = 0;
            foreach (MSkill skill in skills)
            {
                Master.MSkill mSkill = skill.master;
                if (!Master.MSkill.IsSkillType(mSkill, SkillType.help) || mSkill.effect.special != SkillEffectSpecial.tile)
                {
                    continue;
                }
                if (mSkill.wild > 0 && Array.Exists(Util.Global.Constant.tile_wild, v => v == vTile.tileId))
                {
                    aid += mSkill.wild;
                }
                if (mSkill.swim > 0 && Array.Exists(Util.Global.Constant.tile_swim, v => v == vTile.tileId))
                {
                    aid += mSkill.swim;
                }
            }
            return aid == 0 ? 1f : (100 + aid) * 0.01f;
        }
        private bool IsSkillEffectSpecial(SkillEffectSpecial special)
        {
            foreach (MSkill skill in this.skills)
            {
                if (skill.master.effect.special != special)
                {
                    continue;
                }
                return true;
            }
            return false;
        }
        public bool isForceBackAttack
        {
            get
            {
                return IsSkillEffectSpecial(SkillEffectSpecial.force_back_attack);
            }
        }
        public bool isNoBackAttack
        {
            get
            {
                return IsSkillEffectSpecial(SkillEffectSpecial.no_back_attack);
            }
        }
        public bool isMoveAfterAttack
        {
            get
            {
                return IsSkillEffectSpecial(SkillEffectSpecial.move_after_attack);
            }
        }
        public bool isForceHit{
            get{
                return IsSkillEffectSpecial(SkillEffectSpecial.force_hit);
            }
        }
        private List<MBase> _status;
        public List<MBase> status
        {
            get
            {
                return _status;
            }
        }
        public void AddStatus(Master.MStrategy mStrategy)
        {
            this.status.Add(mStrategy);
        }
        public void RemoveStatus(Master.MStrategy mStrategy)
        {
            this.status.Remove(mStrategy);
        }

        /// <summary>
        /// 攻击动作结束后，将受到的技能
        /// </summary>
        public List<App.Model.Master.MSkillEffect> attackEndEffects = new List<App.Model.Master.MSkillEffect>();

        /// <summary>
        /// 本地数据
        /// </summary>
        private File.MCharacter _fileCharacter = null;
        public File.MCharacter fileCharacter
        {
            get
            {
                if (_fileCharacter == null)
                {
                    _fileCharacter = FileCharacterCacher.Instance.GetFromCharacterId(characterId);
                }
                return _fileCharacter;
            }
        }
        /// <summary>
        /// The master.
        /// </summary>
        private Master.MCharacter _master = null;
        public Master.MCharacter master
        {
            get
            {
                if (_master == null)
                {
                    _master = CharacterCacher.Instance.Get(characterId);
                }
                return _master;
            }
        }
    }
}
