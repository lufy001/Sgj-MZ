﻿using System.Collections;
using System.Collections.Generic;
using App.Model;
using App.Util;
using App.Util.Manager;
using Holoville.HOTween;
using UnityEngine;
namespace App.View.Avatar
{
    public class VCharacter : VCharacterBase
    {
        [SerializeField] Anima2D.SpriteMeshInstance head;
        [SerializeField] Anima2D.SpriteMeshInstance hat;
        [SerializeField] Anima2D.SpriteMeshInstance weapon;
        [SerializeField] Anima2D.SpriteMeshInstance weaponRight;
        [SerializeField] Anima2D.SpriteMeshInstance weaponArchery;
        [SerializeField] Anima2D.SpriteMeshInstance weaponString;
        [SerializeField] Anima2D.SpriteMeshInstance weaponArrow;
        [SerializeField] Anima2D.SpriteMeshInstance clothesUpShort;
        [SerializeField] Anima2D.SpriteMeshInstance clothesDownShort;
        [SerializeField] Anima2D.SpriteMeshInstance clothesUpLong;
        [SerializeField] Anima2D.SpriteMeshInstance clothesDownLong;
        [SerializeField] Anima2D.SpriteMeshInstance armLeftShort;
        [SerializeField] Anima2D.SpriteMeshInstance armRightShort;
        [SerializeField] Anima2D.SpriteMeshInstance armLeftLong;
        [SerializeField] Anima2D.SpriteMeshInstance armRightLong;
        [SerializeField] Anima2D.SpriteMeshInstance horseBody;
        [SerializeField] Anima2D.SpriteMeshInstance horseFrontLegLeft;
        [SerializeField] Anima2D.SpriteMeshInstance horseFrontLegRight;
        [SerializeField] Anima2D.SpriteMeshInstance horseHindLegLeft;
        [SerializeField] Anima2D.SpriteMeshInstance horseHindLegRight;
        [SerializeField] Anima2D.SpriteMeshInstance horseSaddle;
        [SerializeField] Anima2D.SpriteMeshInstance legLeft;
        [SerializeField] Anima2D.SpriteMeshInstance legRight;

        private static Vector3 numScale = new Vector3(0.01f, 0.01f, 0.01f);
        private Dictionary<string, Anima2D.SpriteMeshInstance> meshs = new Dictionary<string, Anima2D.SpriteMeshInstance>();
        private Anima2D.SpriteMeshInstance Weapon
        {
            get
            {
                return weapon.gameObject.activeSelf ? weapon : weaponArchery;
            }
        }
        private Anima2D.SpriteMeshInstance ClothesUp
        {
            get
            {
                return clothesUpShort.gameObject.activeSelf ? clothesUpShort : clothesUpLong;
            }
        }
        private Anima2D.SpriteMeshInstance ClothesDown
        {
            get
            {
                return clothesDownShort.gameObject.activeSelf ? clothesDownShort : clothesDownLong;
            }
        }
        private Anima2D.SpriteMeshInstance ArmLeft
        {
            get
            {
                return armLeftShort.gameObject.activeSelf ? armLeftShort : armLeftLong;
            }
        }
        private Anima2D.SpriteMeshInstance ArmRight
        {
            get
            {
                return armRightShort.gameObject.activeSelf ? armRightShort : armRightLong;
            }
        }
        private Anima2D.SpriteMeshInstance[] allSprites;
        public override void SetOrders(Dictionary<string, int> meshOrders)
        {
            foreach (string key in meshOrders.Keys)
            {
                meshs[key].sortingOrder = meshOrders[key];
            }
        }
        protected override void Init()
        {
            if (init)
            {
                return;
            }
            allSprites = this.GetComponentsInChildren<Anima2D.SpriteMeshInstance>(true);
            if (meshs.Count == 0)
            {
                meshs.Add("head", head);
                meshs.Add("hat", hat);
                meshs.Add("weapon", weapon);
                meshs.Add("weaponRight", weaponRight);
                meshs.Add("weaponArchery", weaponArchery);
                meshs.Add("weaponString", weaponString);
                meshs.Add("weaponArrow", weaponArrow);
                meshs.Add("clothesUpShort", clothesUpShort);
                meshs.Add("clothesDownShort", clothesDownShort);
                meshs.Add("clothesUpLong", clothesUpLong);
                meshs.Add("clothesDownLong", clothesDownLong);
                meshs.Add("armLeftShort", armLeftShort);
                meshs.Add("armRightShort", armRightShort);
                meshs.Add("armLeftLong", armLeftLong);
                meshs.Add("armRightLong", armRightLong);
                meshs.Add("horseBody", horseBody);
                meshs.Add("horseFrontLegLeft", horseFrontLegLeft);
                meshs.Add("horseFrontLegRight", horseFrontLegRight);
                meshs.Add("horseHindLegLeft", horseHindLegLeft);
                meshs.Add("horseHindLegRight", horseHindLegRight);
                meshs.Add("horseSaddle", horseSaddle);
                meshs.Add("legLeft", legLeft);
                meshs.Add("legRight", legRight);
            }
            if (materialGray == null)
            {
                materialDefault = head.sharedMaterial;
            }
            num.GetComponent<MeshRenderer>().sortingOrder = clothesDownLong.sortingOrder + 10;
            base.Init();
        }
        protected override bool Gray
        {
            set
            {
                Material material = value ? materialGray : materialDefault;
                foreach (Anima2D.SpriteMeshInstance sprite in allSprites)
                {
                    sprite.sharedMaterial = material;
                }
            }
            get
            {
                return head.sharedMaterial.Equals(materialGray);
            }
        }
        public override float alpha
        {
            set
            {
                foreach (Anima2D.SpriteMeshInstance sprite in allSprites)
                {
                    sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, value);
                }
            }
            get
            {
                return allSprites[0].color.a;
            }
        }
        public override void UpdateView(Model.Character.MCharacter mCharacter)
        {
            base.UpdateView(mCharacter);
            HeadChanged();
            WeaponChanged();
            ClothesChanged();
            HorseChanged();
            ActionChanged();
        }
        private void HeadChanged()
        {
            head.spriteMesh = ImageAssetBundleManager.GetHeadMesh(mCharacter.head);
            hat.spriteMesh = ImageAssetBundleManager.GetHatMesh(mCharacter.hat);
        }
        private void WeaponChanged()
        {
            int weaponId = mCharacter.weapon;
            Model.Equipment.MEquipment mEquipment = mCharacter.equipmentWepon;
            if (mEquipment.equipmentId == 0)
            {
                weapon.gameObject.SetActive(false);
                weaponRight.gameObject.SetActive(false);
                weaponArchery.gameObject.SetActive(false);
                return;
            }
            bool isArchery = (mEquipment.weaponType == WeaponType.archery);
            weapon.gameObject.SetActive(!isArchery);
            weaponArchery.gameObject.SetActive(isArchery);
            if (mEquipment.weaponType == WeaponType.dualWield)
            {
                this.Weapon.spriteMesh = ImageAssetBundleManager.GetLeftWeaponMesh(mEquipment.equipmentId);
                this.weaponRight.spriteMesh = ImageAssetBundleManager.GetRightWeaponMesh(mEquipment.equipmentId);
            }
            else
            {
                this.Weapon.spriteMesh = ImageAssetBundleManager.GetWeaponMesh(mEquipment.equipmentId);
            }
        }
        private void ClothesChanged()
        {
            Model.Equipment.MEquipment mEquipment = mCharacter.equipmentClothes;
            bool isArmor = (mEquipment.clothesType == ClothesType.armor);

            clothesUpShort.gameObject.SetActive(isArmor);
            clothesDownShort.gameObject.SetActive(isArmor);
            armLeftShort.gameObject.SetActive(isArmor);
            armRightShort.gameObject.SetActive(isArmor);

            clothesUpLong.gameObject.SetActive(!isArmor);
            clothesDownLong.gameObject.SetActive(!isArmor);
            armLeftLong.gameObject.SetActive(!isArmor);
            armRightLong.gameObject.SetActive(!isArmor);

            ClothesUp.spriteMesh = ImageAssetBundleManager.GetClothesUpMesh(mEquipment.equipmentId);
            ClothesDown.spriteMesh = ImageAssetBundleManager.GetClothesDownMesh(mEquipment.equipmentId);
        }
        private void HorseChanged()
        {
            Model.Equipment.MEquipment mEquipment = mCharacter.equipmentHorse;
            if (mEquipment.moveType == MoveType.cavalry)
            {
                horseBody.spriteMesh = ImageAssetBundleManager.GetHorseBodyMesh(mEquipment.imageIndex);
                horseFrontLegLeft.spriteMesh = ImageAssetBundleManager.GetHorseFrontLegLeftMesh(mEquipment.imageIndex);
                horseFrontLegRight.spriteMesh = ImageAssetBundleManager.GetHorseFrontLegRightMesh(mEquipment.imageIndex);
                horseHindLegLeft.spriteMesh = ImageAssetBundleManager.GetHorseHindLegLeftMesh(mEquipment.imageIndex);
                horseHindLegRight.spriteMesh = ImageAssetBundleManager.GetHorseHindLegRightMesh(mEquipment.imageIndex);

                horseSaddle.spriteMesh = ImageAssetBundleManager.GetHorseSaddleMesh(mEquipment.saddle);
                legLeft.spriteMesh = ImageAssetBundleManager.GetShoeLeftMesh(App.Util.Global.Constant.shoe_default_index);
                legRight.spriteMesh = ImageAssetBundleManager.GetShoeRightMesh(App.Util.Global.Constant.shoe_default_index);
            }
            else
            {
                legLeft.spriteMesh = ImageAssetBundleManager.GetShoeLeftMesh(mEquipment.imageIndex);
                legRight.spriteMesh = ImageAssetBundleManager.GetShoeRightMesh(mEquipment.imageIndex);
            }
        }
        protected override void ActionChanged()
        {
            string animatorName = string.Format("{0}_{1}_{2}", 
                                                mCharacter.moveType.ToString(), 
                                                WeaponManager.GetWeaponTypeAction(mCharacter.weaponType, mCharacter.action), 
                                                mCharacter.action.ToString());
            if (!this.gameObject.activeInHierarchy)
            {
                return;
            }
            animator.Play(animatorName);
            base.ActionChanged();
        }
        public void OnHeal(Model.Battle.MDamageParam arg)
        {
            this.action = ActionType.block;
            OnHealWithoutAction(arg);
        }
        public void OnDamage(App.Model.Battle.MDamageParam arg)
        {
            this.action = ActionType.hert;
            this.num.text = arg.value.ToString();
            this.num.gameObject.SetActive(true);
            this.num.transform.localPosition = new Vector3(0, 0.2f, 0);
            this.num.color = Color.white;
            this.num.transform.localScale = Vector3.zero;
            Sequence seqHp = new Sequence();
            seqHp.Insert(0f, HOTween.To(this.num.transform, 0.2f, new TweenParms().Prop("localScale", numScale * 2f, false).Ease(EaseType.EaseInQuart)));
            seqHp.Insert(0.2f, HOTween.To(this.num.transform, 0.3f, new TweenParms().Prop("localScale", numScale, false).Ease(EaseType.EaseOutBounce)));
            seqHp.Insert(0.5f, HOTween.To(this.num, 0.2f, new TweenParms().Prop("color", new Color(this.num.color.r, this.num.color.g, this.num.color.b, 0f), false).OnComplete(() => {
                this.num.gameObject.SetActive(false);
            })));
            seqHp.Insert(0f, HOTween.To(this, 0.2f, new TweenParms().Prop("hp", this.mCharacter.hp + arg.value, false).Ease(EaseType.EaseInQuart)));
            seqHp.Play();
        }
        public void OnHealWithoutAction(Model.Battle.MDamageParam arg)
        {
            this.num.text = string.Format("+{0}", arg.value);
            this.num.gameObject.SetActive(true);
            this.num.transform.localPosition = new Vector3(0, 0.2f, 0);
            this.num.color = Color.green;
            this.num.transform.localScale = Vector3.zero;
            Sequence seqHp = new Sequence();
            seqHp.Insert(0f, HOTween.To(this.num.transform, 0.2f, new TweenParms().Prop("localScale", numScale * 2f, false).Ease(EaseType.EaseInQuart)));
            seqHp.Insert(0.2f, HOTween.To(this.num.transform, 0.3f, new TweenParms().Prop("localScale", numScale, false).Ease(EaseType.EaseOutBounce)));
            seqHp.Insert(0.5f, HOTween.To(this.num, 0.2f, new TweenParms().Prop("color", new Color(this.num.color.r, this.num.color.g, this.num.color.b, 0f), false).OnComplete(() => {
                this.num.gameObject.SetActive(false);
            })));
            seqHp.Insert(0f, HOTween.To(this, 0.2f, new TweenParms().Prop("hp", this.mCharacter.hp + arg.value, false).Ease(EaseType.EaseInQuart)));
            seqHp.Play();
        }
    }
}