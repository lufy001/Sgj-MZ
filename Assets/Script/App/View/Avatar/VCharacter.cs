﻿using System.Collections;
using System.Collections.Generic;
using App.Model;
using App.Util;
using App.Util.Cacher;
using App.Util.Manager;
using App.View.Common;
using Holoville.HOTween;
using UnityEngine;
namespace App.View.Avatar
{
    public class VCharacter : VBase
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
        [SerializeField] Transform content;
        [SerializeField] SpriteRenderer hpSprite;
        [SerializeField] TextMesh num;
        [SerializeField] UnityEngine.Rendering.SortingGroup sortingGroup;
        [SerializeField] SpriteRenderer[] status;
        private static Vector3 numScale = new Vector3(0.01f, 0.01f, 0.01f);
        private Sequence sequenceStatus;
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
        public Model.Character.MCharacter mCharacter { get; private set; }
        private static Material materialGray;
        private static Material materialDefault;
        private static Dictionary<Belong, Color32> hpColors = new Dictionary<Belong, Color32>{
            {Belong.self, new Color32(255,0,0,255)},
            {Belong.friend, new Color32(0,255,0,255)},
            {Belong.enemy, new Color32(0,0,255,255)}
        };
        private Anima2D.SpriteMeshInstance[] allSprites;
        private bool init = false;
        public Direction direction{
            set{
                content.localScale = new Vector3(value == Direction.left ? 1 : -1, 1, 1);
            }
            get{
                return content.localScale.x > 0 ? Direction.left : Direction.right;
            }
        }
        public float X
        {
            get{
                return transform.localPosition.x;
            }
            set
            {
                float oldvalue = transform.localPosition.x;
                if (value > oldvalue)
                {
                    direction = Direction.right;
                }
                else if (value < oldvalue)
                {
                    direction = Direction.left;
                }
                transform.localPosition = new Vector3(value, transform.localPosition.y, 0f);
            }
        }
        public float Y
        {
            get
            {
                return transform.localPosition.y;
            }
            set {
                transform.localPosition = new Vector3(transform.localPosition.x, value, 0f);
            }
        }
        private Animator _animator;
        private Animator animator
        {
            get
            {
                if (_animator == null)
                {
                    _animator = GetComponentInChildren<Animator>();
                }
                return _animator;
            }
        }
        public void SetOrders(Dictionary<string, int> meshOrders)
        {
            foreach (string key in meshOrders.Keys)
            {
                meshs[key].sortingOrder = meshOrders[key];
            }
        }
        private void Init()
        {
            if (init)
            {
                return;
            }
            init = true;
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
                materialGray = Resources.Load("Material/GrayMaterial") as Material;
                materialDefault = head.sharedMaterial;
            }
            num.GetComponent<MeshRenderer>().sortingOrder = clothesDownLong.sortingOrder + 10;
            num.gameObject.SetActive(false);
            //BelongChanged(ViewModel.Belong.Value, ViewModel.Belong.Value);
        }
        private bool Gray
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
        public float alpha
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
        public void UpdateView(Model.Character.MCharacter mCharacter)
        {
            this.mCharacter = mCharacter;
            Init();
            HeadChanged();
            WeaponChanged();
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
            App.Model.Master.MEquipment mEquipment = null;
            if (weaponId == 0)
            {

                App.Model.Master.MCharacter character = mCharacter.master;
                mEquipment = EquipmentCacher.Instance.GetEquipment(character.weapon, EquipmentType.weapon);
                weaponId = character.weapon;
            }
            else
            {
                mEquipment = EquipmentCacher.Instance.GetEquipment(weaponId, EquipmentType.weapon);
            }
            if (mEquipment == null)
            {
                weapon.gameObject.SetActive(false);
                weaponRight.gameObject.SetActive(false);
                weaponArchery.gameObject.SetActive(false);
                return;
            }
            bool isArchery = (mEquipment.weaponType == App.Model.WeaponType.archery);
            weapon.gameObject.SetActive(!isArchery);
            weaponArchery.gameObject.SetActive(isArchery);
            if (mEquipment.weaponType == App.Model.WeaponType.dualWield)
            {
                //this.weaponRight.gameObject.SetActive(true);
                this.Weapon.spriteMesh = ImageAssetBundleManager.GetLeftWeaponMesh(weaponId);
                this.weaponRight.spriteMesh = ImageAssetBundleManager.GetRightWeaponMesh(weaponId);
            }
            else
            {
                //this.weaponRight.gameObject.SetActive(false);
                this.Weapon.spriteMesh = ImageAssetBundleManager.GetWeaponMesh(weaponId);
            }
        }
        public ActionType action{
            set{
                mCharacter.action = value;
                ActionChanged();
            }
            get{
                return mCharacter.action;
            }
        }
        private void ActionChanged()
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
            if (mCharacter.action != ActionType.idle)
            {
                this.controller.SendMessage("AddDynamicCharacter", this, SendMessageOptions.DontRequireReceiver);
                return;
            }
            if (mCharacter.hp > 0)
            {
                this.StartCoroutine(RemoveDynamicCharacter());
                return;
            }
            HOTween.To(this, 1f, new TweenParms().Prop("alpha", 0f).OnComplete(() => {
                this.gameObject.SetActive(false);
                this.alpha = 1f;
                if (sequenceStatus != null)
                {
                    sequenceStatus.Kill();
                }
                if (App.Util.AppManager.CurrentScene != null)
                {
                    App.Util.AppManager.CurrentScene.StartCoroutine(RemoveDynamicCharacter());
                }
            }));
        }
        private IEnumerator RemoveDynamicCharacter()
        {
            while (this.gameObject.activeSelf && this.num.gameObject.activeSelf)
            {
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
            this.controller.SendMessage("RemoveDynamicCharacter", this, SendMessageOptions.DontRequireReceiver);
        }
        public void AttackToHert()
        {
            if (mCharacter.target == null)
            {
                return;
            }
            if (mCharacter.currentSkill.useToEnemy)
            {
                Global.battleEvent.OnDamage(this);
                //this.controller.SendMessage("OnDamage", this, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                this.controller.SendMessage("OnHeal", this, SendMessageOptions.DontRequireReceiver);
            }
        }
        public void ActionEnd()
        {
            this.action = ActionType.idle;
        }
        public int hp{
            get{
                return mCharacter.hp;
            }
            set{
                mCharacter.hp = value;
                float hpValue = value * 1f / mCharacter.ability.hpMax;
                hpSprite.transform.localPosition = new Vector3((hpValue - 1f) * 0.5f, 0f, 0f);
                hpSprite.transform.localScale = new Vector3(hpValue, 1f, 1f);
            }
        }
        public void OnBlock()
        {
            this.action = ActionType.block;
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